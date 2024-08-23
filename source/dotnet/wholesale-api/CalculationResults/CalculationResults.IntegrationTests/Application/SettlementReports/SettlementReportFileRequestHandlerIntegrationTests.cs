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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Persistence.Databricks;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Microsoft.Extensions.Options;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Application.SettlementReports;

[Collection(nameof(SettlementReportCollectionFixture))]
public sealed class SettlementReportFileRequestHandlerIntegrationTests : TestBase<SettlementReportFileRequestHandler>
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

        Fixture.Inject(mockedOptions);

        var sqlWarehouseQueryExecutor = _databricksSqlStatementApiFixture.GetDatabricksExecutor();
        var databricksContext = new SettlementReportDatabricksContext(mockedOptions.Object, sqlWarehouseQueryExecutor);

        var settlementReportDataRepository = new SettlementReportEnergyResultRepository(databricksContext);
        var settlementReportWholesaleRepository = new SettlementReportWholesaleRepository(databricksContext);
        var settlementReportChargeLinkPeriodsRepository = new SettlementReportChargeLinkPeriodsRepository(databricksContext);

        var settlementReportMeteringPointMasterDataRepository = new SettlementReportMeteringPointMasterDataRepository(new SettlementReportDatabricksContext(
            mockedOptions.Object,
            sqlWarehouseQueryExecutor));

        var settlementReportMeteringPointTimeSeriesResultRepository = new SettlementReportMeteringPointTimeSeriesResultRepository(new SettlementReportDatabricksContext(
            mockedOptions.Object,
            sqlWarehouseQueryExecutor));

        var settlementReportMonthlyAmountRepository = new SettlementReportMonthlyAmountRepository(new SettlementReportDatabricksContext(
            mockedOptions.Object,
            sqlWarehouseQueryExecutor));

        var settlementReportChargePriceRepository = new SettlementReportChargePriceRepository(new SettlementReportDatabricksContext(
            mockedOptions.Object,
            sqlWarehouseQueryExecutor));

        var settlementReportMonthlyAmountTotalRepository = new SettlementReportMonthlyAmountTotalRepository(new SettlementReportDatabricksContext(
            mockedOptions.Object,
            sqlWarehouseQueryExecutor));

        Fixture.Inject<ISettlementReportFileGeneratorFactory>(new SettlementReportFileGeneratorFactory(
            settlementReportDataRepository,
            settlementReportWholesaleRepository,
            settlementReportChargeLinkPeriodsRepository,
            settlementReportMeteringPointMasterDataRepository,
            settlementReportMeteringPointTimeSeriesResultRepository,
            settlementReportMonthlyAmountRepository,
            settlementReportChargePriceRepository,
            settlementReportMonthlyAmountTotalRepository));

        var blobContainerClient = settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        Fixture.Inject<ISettlementReportFileRepository>(new SettlementReportFileBlobStorage(blobContainerClient));
    }

    [Fact]
    public async Task RequestFileAsync_ForWholesaleFixing_ReturnsExpectedCsv()
    {
        // Arrange
        var calculationId = Guid.Parse("61d60f89-bbc5-4f7a-be98-6139aab1c1b2");
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
        var actorInfo = new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME,
            [
                ["'61d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'0'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "1.100"],
                ["'61d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'0'", "'47433af6-03c1-46bd-ab9b-dd0497035306'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-11T18:30:00.000+00:00'", "2.100"],
                ["'61d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'0'", "'47433af6-03c1-46bd-ab9b-dd0497035307'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-12T23:15:00.000+00:00'", "2.200"],
                ["'61d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'0'", "'47433af6-03c1-46bd-ab9b-dd0497035308'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-13T12:00:00.000+00:00'", "1.200"],
                ["'61d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'0'", "'47433af6-03c1-46bd-ab9b-dd0497035309'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-14T12:15:00.000+00:00'", "3.200"],
            ]);

        // Act
        var actual = await Sut.RequestFileAsync(fileRequest, actorInfo);

        // Assert
        Assert.Equal(requestId, actual.RequestId);

        var container = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        var generatedFileBlob = container.GetBlobClient($"settlement-reports/{requestId.Id}/{actual.StorageFileName}");
        var generatedFile = await generatedFileBlob.DownloadContentAsync();
        var fileContents = generatedFile.Value.Content.ToString();
        var fileLines = fileContents.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("METERINGGRIDAREAID,ENERGYBUSINESSPROCESS,STARTDATETIME,RESOLUTIONDURATION,TYPEOFMP,SETTLEMENTMETHOD,ENERGYQUANTITY", fileLines[0]);
        Assert.Equal("\"018\",D05,2022-01-10T03:15:00Z,PT15M,E17,E02,1.100", fileLines[1]);
        Assert.Equal("\"018\",D05,2022-01-11T18:30:00Z,PT15M,E17,E02,2.100", fileLines[2]);
        Assert.Equal("\"018\",D05,2022-01-12T23:15:00Z,PT15M,E17,E02,2.200", fileLines[3]);
        Assert.Equal("\"018\",D05,2022-01-13T12:00:00Z,PT15M,E17,E02,1.200", fileLines[4]);
        Assert.Equal("\"018\",D05,2022-01-14T12:15:00Z,PT15M,E17,E02,3.200", fileLines[5]);
    }

    [Fact]
    public async Task RequestFileAsync_ForChargeLinkPeriods_ReturnsExpectedCsv()
    {
        // Arrange
        var calculationId = Guid.Parse("71d60f89-bbc5-4f7a-be98-6139aab1c1b3");
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
        var actorInfo = new SettlementReportRequestedByActor(MarketRole.EnergySupplier, null);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager
            .InsertAsync<SettlementReportChargeLinkPeriodsViewColumns>(
                _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value
                    .CHARGE_LINK_PERIODS_V1_VIEW_NAME,
                [
                    [
                        "'71d60f89-bbc5-4f7a-be98-6139aab1c1b3'", "'wholesale_fixing'",
                        "'15cba911-b91e-4786-bed4-f0d28418a9eb'", "'consumption'", "'tariff'", "'40000'",
                        "'6392825108998'", "46", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-03T02:00:00.000+00:00'",
                        "'018'", "'8397670583196'", "0"
                    ],
                    [
                        "'71d60f89-bbc5-4f7a-be98-6139aab1c1b3'", "'wholesale_fixing'",
                        "'15cba911-b91e-4786-bed4-f0d28418a9e2'", "'consumption'", "'tariff'", "'40000'",
                        "'6392825108998'", "46", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-03T02:00:00.000+00:00'",
                        "'018'", "'8397670583191'", "0"
                    ],
                ]);

        var actual = await Sut.RequestFileAsync(fileRequest, actorInfo);

        // Assert
        Assert.Equal(requestId, actual.RequestId);

        var container = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        var generatedFileBlob = container.GetBlobClient($"settlement-reports/{requestId.Id}/{actual.StorageFileName}");
        var generatedFile = await generatedFileBlob.DownloadContentAsync();
        var fileContents = generatedFile.Value.Content.ToString();
        var fileLines = fileContents.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(
            "METERINGPOINTID,TYPEOFMP,CHARGETYPE,CHARGEOWNER,CHARGEID,CHARGEOCCURRENCES,PERIODSTART,PERIODEND",
            fileLines[0]);
        Assert.Equal(
            "\"15cba911-b91e-4786-bed4-f0d28418a9e2\",E17,D03,\"6392825108998\",40000,46,2022-01-02T02:00:00Z,2022-01-03T02:00:00Z",
            fileLines[1]);
        Assert.Equal(
            "\"15cba911-b91e-4786-bed4-f0d28418a9eb\",E17,D03,\"6392825108998\",40000,46,2022-01-02T02:00:00Z,2022-01-03T02:00:00Z",
            fileLines[2]);
    }

    [Theory]
    [InlineData("444", SettlementReportFileContent.Pt15M, "\"400000000000000004\",E20,2022-01-01T23:00:00Z,678.900,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,")]
    [InlineData("454", SettlementReportFileContent.Pt1H, "\"400000000000000004\",E20,2022-01-01T23:00:00Z,679.900,,,,,,,,,,,,,,,,,,,,,,,,")]
    public async Task RequestFileAsync_ForWholesaleFixingMeteringPointTimeSeries_ReturnsExpectedCsv(string gridAreaCode, SettlementReportFileContent content, string expected)
    {
        // Arrange
        var calculationId = Guid.Parse("891b7070-b80f-4731-8714-76221e27c366");
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
        var actorInfo = new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointTimeSeriesViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_METERING_POINT_TIME_SERIES_V1_VIEW_NAME,
            [
                ["'891b7070-b80f-4731-8714-76221e27c366'", "'wholesale_fixing'", "'1'", "'400000000000000004'", "'exchange'", "'PT15M'", $"'{gridAreaCode}'", "'8442359392712'", "'2022-01-02 12:00:00'", "678.90"],
                ["'891b7070-b80f-4731-8714-76221e27c366'", "'wholesale_fixing'", "'1'", "'400000000000000004'", "'exchange'", "'PT1H'", $"'{gridAreaCode}'", "'8442359392712'", "'2022-01-02 12:15:00'", "679.90"],
            ]);

        // Act
        var actual = await Sut.RequestFileAsync(fileRequest, actorInfo);

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
        var actorInfo = new SettlementReportRequestedByActor(MarketRole.DataHubAdministrator, null);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointMasterDataViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.METERING_POINT_MASTER_DATA_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26de'", "'wholesale_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829e1'", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-03T02:00:00.000+00:00'", "'018'", "'406'", "'407'", "'consumption'", "'flex'", "8397670583196"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26de'", "'wholesale_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829e2'", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-03T02:00:00.000+00:00'", "'018'", "'406'", "'407'", "'consumption'", "'flex'", "8397670583196"],
            ]);

        // Act
        var actual = await Sut.RequestFileAsync(fileRequest, actorInfo);

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
            "\"15cba911-b91e-4782-bed4-f0d2841829e1\",2022-01-02T02:00:00Z,2022-01-03T02:00:00Z,\"018\",\"407\",\"406\",E17,D01,\"8397670583196\"",
            fileLines[1]);
        Assert.Equal(
            "\"15cba911-b91e-4782-bed4-f0d2841829e2\",2022-01-02T02:00:00Z,2022-01-03T02:00:00Z,\"018\",\"407\",\"406\",E17,D01,\"8397670583196\"",
            fileLines[2]);
    }

    [Fact]
    public async Task RequestFileAsync_ForMonthlyAmount_ReturnsExpectedCsv()
    {
        // Arrange
        var calculationId = Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26df");
        var filter = new SettlementReportRequestFilterDto(
            new Dictionary<string, CalculationId?>
            {
                {
                    "4", new CalculationId(calculationId)
                },
            },
            _january1St.ToDateTimeOffset(),
            _january5Th.ToDateTimeOffset(),
            CalculationType.FirstCorrectionSettlement,
            null,
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var fileRequest = new SettlementReportFileRequestDto(
            requestId,
            SettlementReportFileContent.MonthlyAmount,
            new SettlementReportPartialFileInfo(Guid.NewGuid().ToString(), true),
            filter,
            1);
        var actorInfo = new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, "8397670583197");

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMonthlyAmountViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.MONTHLY_AMOUNTS_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26df'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e1'", "'4'", "8397670583196", "'2022-01-02T02:00:00.000+00:00'", "'kWh'", "18.012345", "'tariff'", "'123'", "8397670583197", "1" ],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26df'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e2'", "'4'", "8397670583196", "'2022-01-02T04:00:00.000+00:00'", "'pcs'", "18.012346", "'subscription'", "'122'", "8397670583197", "1" ],
            ]);

        // Act
        var actual = await Sut.RequestFileAsync(fileRequest, actorInfo);

        // Assert
        Assert.Equal(requestId, actual.RequestId);

        var container = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        var generatedFileBlob = container.GetBlobClient($"settlement-reports/{requestId.Id}/{actual.StorageFileName}");
        var generatedFile = await generatedFileBlob.DownloadContentAsync();
        var fileContents = generatedFile.Value.Content.ToString();
        var fileLines = fileContents.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(3, fileLines.Length);
        Assert.Equal(
            "ENERGYBUSINESSPROCESS,PROCESSVARIANT,METERINGGRIDAREAID,ENERGYSUPPLIERID,STARTDATETIME,RESOLUTIONDURATION,MEASUREUNIT,ENERGYCURRENCY,AMOUNT,CHARGETYPE,CHARGEID,CHARGEOWNER",
            fileLines[0]);
        Assert.Equal(
            "D32,1ST,\"004\",\"8397670583196\",2022-01-02T02:00:00Z,P1M,KWH,DKK,18.012345,D03,123,\"8397670583197\"",
            fileLines[1]);
        Assert.Equal(
            "D32,1ST,\"004\",\"8397670583196\",2022-01-02T04:00:00Z,P1M,PCS,DKK,18.012346,D01,122,\"8397670583197\"",
            fileLines[2]);
    }
}
