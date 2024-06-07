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
    IClassFixture<DatabricksSqlStatementApiFixture>
{
    private const string GridAreaA = "018";
    private readonly string[] _gridAreaCodes = [GridAreaA];
    private readonly Instant _january1St = Instant.FromUtc(2022, 1, 1, 0, 0, 0);
    private readonly Instant _january5Th = Instant.FromUtc(2022, 1, 31, 0, 0, 0);

    private readonly DatabricksSqlStatementApiFixture _databricksSqlStatementApiFixture;
    private readonly SettlementReportFileBlobStorageFixture _settlementReportFileBlobStorageFixture;

    public SettlementReportFileRequestHandlerIntegrationTests(
        DatabricksSqlStatementApiFixture databricksSqlStatementApiFixture,
        SettlementReportFileBlobStorageFixture settlementReportFileBlobStorageFixture)
    {
        _databricksSqlStatementApiFixture = databricksSqlStatementApiFixture;
        _settlementReportFileBlobStorageFixture = settlementReportFileBlobStorageFixture;

        var mockedOptions = new Mock<IOptions<DeltaTableOptions>>();
        mockedOptions.Setup(x => x.Value).Returns(new DeltaTableOptions
        {
            SettlementReportSchemaName = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.SCHEMA_NAME,
            WHOLESALE_RESULTS_V1_VIEW_NAME = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_V1_VIEW_NAME,
            BasisDataSchemaName = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.BasisDataSchemaName,
            SCHEMA_NAME = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.SCHEMA_NAME,
            EdiResultsSchemaName = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.EdiResultsSchemaName,
            ENERGY_RESULTS_TABLE_NAME = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_TABLE_NAME,
            WHOLESALE_RESULTS_TABLE_NAME = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_TABLE_NAME,
            TOTAL_MONTHLY_AMOUNTS_TABLE_NAME = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.TOTAL_MONTHLY_AMOUNTS_TABLE_NAME,
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

        Fixture.Inject<ISettlementReportFileGeneratorFactory>(new SettlementReportFileGeneratorFactory(settlementReportDataRepository, settlementReportWholesaleRepository));

        var blobContainerClient = settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        Fixture.Inject<ISettlementReportFileRepository>(new SettlementReportFileBlobStorage(blobContainerClient));
    }

    [Fact]
    public async Task RequestFileAsync_ForWholesaleFixing_ReturnsExpectedCsv()
    {
        // Arrange
        var calculationId = Guid.Parse("51d60f89-bbc5-4f7a-be98-6139aab1c1b2");
        var filter = new SettlementReportRequestFilterDto(
            _gridAreaCodes.ToDictionary(x => x, _ => new CalculationId(calculationId)),
            _january1St.ToDateTimeOffset(),
            _january5Th.ToDateTimeOffset(),
            null,
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var fileRequest = new SettlementReportFileRequestDto(
            SettlementReportFileContent.EnergyResultForCalculationId,
            new SettlementReportPartialFileInfo(Guid.NewGuid().ToString()),
            requestId,
            filter);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME,
            [
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'WholesaleFixing'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'WholesaleFixing'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-11T18:30:00.000+00:00'", "21.011"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'WholesaleFixing'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-12T23:15:00.000+00:00'", "22.458"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'WholesaleFixing'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-13T12:00:00.000+00:00'", "151.24"],
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
        Assert.Equal("805,D04,2022-01-01T01:00:00Z,PT15M,E18,,1.100", fileLines[1]);
        Assert.Equal("111,D04,2022-01-01T01:00:00Z,PT15M,E18,,2.100", fileLines[2]);
        Assert.Equal("111,D04,2022-01-02T01:00:00Z,PT15M,E18,,2.200", fileLines[3]);
        Assert.Equal("805,D04,2022-01-02T01:00:00Z,PT15M,E18,,1.200", fileLines[4]);
        Assert.Equal("805,D04,2022-01-03T01:00:00Z,PT15M,E18,,3.200", fileLines[5]);
    }
}
