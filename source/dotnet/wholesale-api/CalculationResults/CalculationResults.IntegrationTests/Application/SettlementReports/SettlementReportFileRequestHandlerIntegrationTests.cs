// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using AutoFixture;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Microsoft.Extensions.Options;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Application.SettlementReports;

[Collection(nameof(SettlementReportFileCollectionFixture))]
public sealed class SettlementReportFileRequestHandlerIntegrationTests : TestBase<SettlementReportFileRequestHandler>,
    IClassFixture<MigrationsFreeDatabricksSqlStatementApiFixture>
{
    private const string GridAreaA = "018";
    private readonly string[] _gridAreaCodes = [GridAreaA];
    private readonly Instant _january1St = Instant.FromUtc(2022, 1, 1, 0, 0, 0);
    private readonly Instant _january5Th = Instant.FromUtc(2022, 1, 31, 0, 0, 0);

    private readonly MigrationsFreeDatabricksSqlStatementApiFixture _databricksSqlStatementApiFixture;
    private readonly SettlementReportFileBlobStorageFixture _settlementReportFileBlobStorageFixture;

    public SettlementReportFileRequestHandlerIntegrationTests(
        MigrationsFreeDatabricksSqlStatementApiFixture databricksSqlStatementApiFixture,
        SettlementReportFileBlobStorageFixture settlementReportFileBlobStorageFixture)
    {
        _databricksSqlStatementApiFixture = databricksSqlStatementApiFixture;
        _settlementReportFileBlobStorageFixture = settlementReportFileBlobStorageFixture;

        var mockedOptions = new Mock<IOptions<DeltaTableOptions>>();
        mockedOptions.Setup(x => x.Value).Returns(new DeltaTableOptions
        {
            SettlementReportSchemaName = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.SCHEMA_NAME,
            SCHEMA_NAME = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.SCHEMA_NAME,
        });

        var calc = new CalculationDto(null, Guid.Empty, DateTimeOffset.Now, DateTimeOffset.Now, "a", "b", DateTimeOffset.Now, DateTimeOffset.Now, CalculationState.Completed, true, [], CalculationType.Aggregation, Guid.Empty, 1, CalculationOrchestrationState.Calculated);

        var calculationsClientMock = new Mock<ICalculationsClient>();
        calculationsClientMock
            .Setup(x => x.GetAsync(It.IsAny<Guid>()))
            .ReturnsAsync(calc);

        var settlementReportDataRepository = new SettlementReportEnergyResultRepository(new SettlementReportEnergyResultQueries(
            mockedOptions.Object,
            databricksSqlStatementApiFixture.GetDatabricksExecutor(),
            calculationsClientMock.Object));

        var settlementReportWholesaleRepository = new SettlementReportWholesaleRepository(new SettlementReportWholesaleResultQueries(
            mockedOptions.Object,
            _databricksSqlStatementApiFixture.GetDatabricksExecutor(),
            calculationsClientMock.Object));

        var settlementReportChargeLinkPeriodsRepository = new SettlementReportChargeLinkPeriodsRepository(new SettlementReportChargeLinkPeriodsQueries(
            mockedOptions.Object,
            _databricksSqlStatementApiFixture.GetDatabricksExecutor(),
            calculationsClientMock.Object));

        var settlementReportMeteringPointMasterDataRepository = new SettlementReportMeteringPointMasterDataRepository(
            new SettlementReportMeteringPointMasterDataQueries(
                mockedOptions.Object,
                _databricksSqlStatementApiFixture.GetDatabricksExecutor(),
                calculationsClientMock.Object));

        var settlementReportMeteringPointTimeSeriesResultRepository = new SettlementReportMeteringPointTimeSeriesResultRepository(new SettlementReportMeteringPointTimeSeriesResultQueries(
            mockedOptions.Object,
            _databricksSqlStatementApiFixture.GetDatabricksExecutor(),
            calculationsClientMock.Object));

        var settlementReportMonthlyAmountRepository = new SettlementReportMonthlyAmountRepository(new SettlementReportMonthlyAmountQueries(
            mockedOptions.Object,
            _databricksSqlStatementApiFixture.GetDatabricksExecutor(),
            calculationsClientMock.Object));

        var settlementReportChargePriceRepository = new SettlementReportChargePriceRepository(new SettlementReportChargePriceQueries(
            mockedOptions.Object,
            _databricksSqlStatementApiFixture.GetDatabricksExecutor(),
            calculationsClientMock.Object));

        Fixture.Inject<ISettlementReportFileGeneratorFactory>(new SettlementReportFileGeneratorFactory(
            settlementReportDataRepository,
            settlementReportWholesaleRepository,
            settlementReportChargeLinkPeriodsRepository,
            settlementReportMeteringPointMasterDataRepository,
            settlementReportMeteringPointTimeSeriesResultRepository,
            settlementReportMonthlyAmountRepository,
            settlementReportChargePriceRepository));

        var blobContainerClient = settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        Fixture.Inject<ISettlementReportFileRepository>(new SettlementReportFileBlobStorage(blobContainerClient));
    }

    [Fact]
    public async Task RequestFileAsync_ForWholesaleFixing_ReturnsExpectedCsv()
    {
        // Arrange
        var calculationId = Guid.Parse("51d60f89-bbc5-4f7a-be98-6139aab1c1b2");
        var filter = new SettlementReportRequestFilterDto(
            _gridAreaCodes.ToDictionary(x => x, _ => (CalculationId?)new CalculationId(calculationId)),
            _january1St.ToDateTimeOffset(),
            _january5Th.ToDateTimeOffset(),
            CalculationType.WholesaleFixing,
            null,
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var fileRequest = new SettlementReportFileRequestDto(
            requestId,
            SettlementReportFileContent.EnergyResult,
            new SettlementReportPartialFileInfo(Guid.NewGuid().ToString(), true),
            filter,
            1);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME,
            [
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'0'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "1.100"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'0'", "'47433af6-03c1-46bd-ab9b-dd0497035306'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-11T18:30:00.000+00:00'", "2.100"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'0'", "'47433af6-03c1-46bd-ab9b-dd0497035307'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-12T23:15:00.000+00:00'", "2.200"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'0'", "'47433af6-03c1-46bd-ab9b-dd0497035308'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-13T12:00:00.000+00:00'", "1.200"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'0'", "'47433af6-03c1-46bd-ab9b-dd0497035309'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-14T12:15:00.000+00:00'", "3.200"],
            ]);

        // Act
        var actual = await Sut.RequestFileAsync(fileRequest);

        // Assert
        Assert.Equal(requestId, actual.RequestId);

        var container = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        var generatedFileBlob = container.GetBlobClient($"settlement-reports/{requestId.Id}/{actual.StorageFileName}");
        var generatedFile = await generatedFileBlob.DownloadContentAsync();
        var fileContents = generatedFile.Value.Content.ToString();
        var fileLines = fileContents.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("METERINGGRIDAREAID,ENERGYBUSINESSPROCESS,STARTDATETIME,RESOLUTIONDURATION,TYPEOFMP,SETTLEMENTMETHOD,ENERGYQUANTITY", fileLines[0]);
        Assert.Equal("018,D05,2022-01-10T03:15:00Z,PT15M,E17,E02,1.100", fileLines[1]);
        Assert.Equal("018,D05,2022-01-11T18:30:00Z,PT15M,E17,E02,2.100", fileLines[2]);
        Assert.Equal("018,D05,2022-01-12T23:15:00Z,PT15M,E17,E02,2.200", fileLines[3]);
        Assert.Equal("018,D05,2022-01-13T12:00:00Z,PT15M,E17,E02,1.200", fileLines[4]);
        Assert.Equal("018,D05,2022-01-14T12:15:00Z,PT15M,E17,E02,3.200", fileLines[5]);
    }

    [Fact]
    public async Task RequestFileAsync_ForChargeLinkPeriods_ReturnsExpectedCsv()
    {
        // Arrange
        var calculationId = Guid.Parse("51d60f89-bbc5-4f7a-be98-6139aab1c1b3");
        var filter = new SettlementReportRequestFilterDto(
            _gridAreaCodes.ToDictionary(x => x, _ => (CalculationId?)new CalculationId(calculationId)),
            _january1St.ToDateTimeOffset(),
            _january5Th.ToDateTimeOffset(),
            CalculationType.WholesaleFixing,
            null,
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var fileRequest = new SettlementReportFileRequestDto(
            requestId,
            SettlementReportFileContent.ChargeLinksPeriods,
            new SettlementReportPartialFileInfo(Guid.NewGuid().ToString(), true),
            filter,
            1);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager
            .InsertAsync<SettlementReportChargeLinkPeriodsViewColumns>(
                _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value
                    .CHARGE_LINK_PERIODS_V1_VIEW_NAME,
                [
                    [
                        "'51d60f89-bbc5-4f7a-be98-6139aab1c1b3'", "'wholesale_fixing'",
                        "'15cba911-b91e-4786-bed4-f0d28418a9eb'", "'consumption'", "'tariff'", "'40000'",
                        "'6392825108998'", "46", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-03T02:00:00.000+00:00'",
                        "'018'", "'8397670583196'"
                    ],
                    [
                        "'51d60f89-bbc5-4f7a-be98-6139aab1c1b3'", "'wholesale_fixing'",
                        "'15cba911-b91e-4786-bed4-f0d28418a9e2'", "'consumption'", "'tariff'", "'40000'",
                        "'6392825108998'", "46", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-03T02:00:00.000+00:00'",
                        "'018'", "'8397670583191'"
                    ],
                ]);

        var actual = await Sut.RequestFileAsync(fileRequest);

        // Assert
        Assert.Equal(requestId, actual.RequestId);

        var container = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        var generatedFileBlob = container.GetBlobClient($"settlement-reports/{requestId.Id}/{actual.StorageFileName}");
        var generatedFile = await generatedFileBlob.DownloadContentAsync();
        var fileContents = generatedFile.Value.Content.ToString();
        var fileLines = fileContents.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(
            "METERINGPOINTID,TYPEOFMP,CHARGETYPE,CHARGETYPEOWNERID,CHARGETYPEID,CHARGEOCCURRENCES,PERIODSTART,PERIODEND",
            fileLines[0]);
        Assert.Equal(
            "15cba911-b91e-4786-bed4-f0d28418a9e2,E17,D03,6392825108998,40000,46,2022-01-02T02:00:00Z,2022-01-03T02:00:00Z",
            fileLines[1]);
        Assert.Equal(
            "15cba911-b91e-4786-bed4-f0d28418a9eb,E17,D03,6392825108998,40000,46,2022-01-02T02:00:00Z,2022-01-03T02:00:00Z",
            fileLines[2]);
    }

    [Theory]
    [InlineData(SettlementReportFileContent.Pt15M, "400000000000000004,Exchange,2022-01-02T02:00:00Z,678.900,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,")]
    [InlineData(SettlementReportFileContent.Pt1H, "400000000000000004,Exchange,2022-01-02T02:00:00Z,679.900,,,,,,,,,,,,,,,,,,,,,,,,")]
    public async Task RequestFileAsync_ForWholesaleFixingMeteringPointTimeSeries_ReturnsExpectedCsv(SettlementReportFileContent content, string expected)
    {
        // Arrange
        var calculationId = Guid.Parse("891b7070-b80f-4731-8714-76221e27c366");
        var gridAreaCode = "404";
        var filter = new SettlementReportRequestFilterDto(
            new Dictionary<string, CalculationId?>
            {
                {
                    gridAreaCode, new CalculationId(calculationId)
                },
            },
            _january1St.ToDateTimeOffset(),
            _january5Th.ToDateTimeOffset(),
            CalculationType.WholesaleFixing,
            null,
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var fileRequest = new SettlementReportFileRequestDto(
            requestId,
            content,
            new SettlementReportPartialFileInfo(Guid.NewGuid().ToString(), true),
            filter,
            1);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointTimeSeriesViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_METERING_POINT_TIME_SERIES_V1_VIEW_NAME,
            [
                ["'891b7070-b80f-4731-8714-76221e27c366'", "'400000000000000004'", "'exchange'", "'PT15M'", "'404'", "'8442359392712'", "'2022-01-02T02:00:00.000+00:00'", "ARRAY(STRUCT('2022-01-02 12:00:00' AS observation_time, 678.90 AS quantity))"],
                ["'891b7070-b80f-4731-8714-76221e27c366'", "'400000000000000004'", "'exchange'", "'PT1H'", "'404'", "'8442359392712'", "'2022-01-02T02:00:00.000+00:00'", "ARRAY(STRUCT('2022-01-02 12:15:00' AS observation_time, 679.90 AS quantity))"],
            ]);

        // Act
        var actual = await Sut.RequestFileAsync(fileRequest);

        // Assert
        Assert.Equal(requestId, actual.RequestId);

        var container = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        var generatedFileBlob = container.GetBlobClient($"settlement-reports/{requestId.Id}/{actual.StorageFileName}");
        var generatedFile = await generatedFileBlob.DownloadContentAsync();
        var fileContents = generatedFile.Value.Content.ToString();
        var fileLines = fileContents.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        Assert.StartsWith("METERINGPOINTID,TYPEOFMP,STARTDATETIME,", fileLines[0]);
        Assert.Equal(expected, fileLines[1]);
    }

    [Fact]
    public async Task RequestFileAsync_ForMeteringPointMasterData_ReturnsExpectedCsv()
    {
        // Arrange
        var calculationId = Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26de");
        var filter = new SettlementReportRequestFilterDto(
            _gridAreaCodes.ToDictionary(x => x, _ => (CalculationId?)new CalculationId(calculationId)),
            _january1St.ToDateTimeOffset(),
            _january5Th.ToDateTimeOffset(),
            CalculationType.WholesaleFixing,
            null,
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var fileRequest = new SettlementReportFileRequestDto(
            requestId,
            SettlementReportFileContent.MeteringPointMasterData,
            new SettlementReportPartialFileInfo(Guid.NewGuid().ToString(), true),
            filter,
            1);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointMasterDataViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.METERING_POINT_MASTER_DATA_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26de'", "'wholesale_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829e1'", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-03T02:00:00.000+00:00'", "'018'", "'406'", "'407'", "'consumption'", "'flex'", "8397670583196"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26de'", "'wholesale_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829e2'", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-03T02:00:00.000+00:00'", "'018'", "'406'", "'407'", "'consumption'", "'flex'", "8397670583196"],
            ]);

        // Act
        var actual = await Sut.RequestFileAsync(fileRequest);

        // Assert
        Assert.Equal(requestId, actual.RequestId);

        var container = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        var generatedFileBlob = container.GetBlobClient($"settlement-reports/{requestId.Id}/{actual.StorageFileName}");
        var generatedFile = await generatedFileBlob.DownloadContentAsync();
        var fileContents = generatedFile.Value.Content.ToString();
        var fileLines = fileContents.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(3, fileLines.Length);
        Assert.Equal(
            "METERINGPOINTID,VALIDFROM,VALIDTO,GRIDAREAID,TOGRIDAREAID,FROMGRIDAREAID,TYPEOFMP,SETTLEMENTMETHOD,ENERGYSUPPLIERID",
            fileLines[0]);
        Assert.Equal(
            "15cba911-b91e-4782-bed4-f0d2841829e1,2022-01-02T02:00:00Z,2022-01-03T02:00:00Z,018,407,406,E17,D01,8397670583196",
            fileLines[1]);
        Assert.Equal(
            "15cba911-b91e-4782-bed4-f0d2841829e2,2022-01-02T02:00:00Z,2022-01-03T02:00:00Z,018,407,406,E17,D01,8397670583196",
            fileLines[2]);
    }

    [Fact]
    public async Task RequestFileAsync_ForMonthlyAmount_ReturnsExpectedCsv()
    {
        // Arrange
        var calculationId = Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26de");
        var filter = new SettlementReportRequestFilterDto(
            _gridAreaCodes.ToDictionary(x => x, _ => new CalculationId(calculationId)),
            _january1St.ToDateTimeOffset(),
            _january5Th.ToDateTimeOffset(),
            CalculationType.FirstCorrectionSettlement,
            null,
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var fileRequest = new SettlementReportFileRequestDto(
            SettlementReportFileContent.MonthlyAmount,
            new SettlementReportPartialFileInfo(Guid.NewGuid().ToString(), true),
            requestId,
            filter);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMonthlyAmountViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.MONTHLY_AMOUNTS_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26de'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e1'", "'018'", "8397670583196", "'2022-01-02T02:00:00.000+00:00'", "'PT1H'", "'kWh'", "'DKK'", "18.012345", "'tariff'", "'123'", "8397670583197" ],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26de'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e2'", "'018'", "8397670583196", "'2022-01-02T04:00:00.000+00:00'", "'P1D'", "'pcs'", "'DKK'", "18.012346", "'subscription'", "'122'", "8397670583197" ],
            ]);

        // Act
        var actual = await Sut.RequestFileAsync(fileRequest);

        // Assert
        Assert.Equal(requestId, actual.RequestId);

        var container = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        var generatedFileBlob = container.GetBlobClient($"settlement-reports/{requestId.Id}/{actual.StorageFileName}");
        var generatedFile = await generatedFileBlob.DownloadContentAsync();
        var fileContents = generatedFile.Value.Content.ToString();
        var fileLines = fileContents.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(3, fileLines.Length);
        Assert.Equal(
            "ENERGYBUSINESSPROCESS,PROCESSVARIANT,METERINGGRIDAREAID,ENERGYSUPPLIERID,STARTDATETIME,RESOLUTIONDURATION,MEASUREUNIT,ENERGYCURRENCY,AMOUNT,CHARGETYPE,CHARGETYPEID,CHARGETYPEOWNERID",
            fileLines[0]);
        Assert.Equal(
            "D32,1ST,018,8397670583196,2022-01-02T02:00:00Z,PT1H,KWH,DKK,18.012345,D03,123,8397670583197",
            fileLines[1]);
        Assert.Equal(
            "D32,1ST,018,8397670583196,2022-01-02T04:00:00Z,P1D,PCS,DKK,18.012346,D01,122,8397670583197",
            fileLines[2]);
    }
}
