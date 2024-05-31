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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Application.SettlementReports;

[Collection(nameof(SettlementReportFileCollectionFixture))]
public sealed class SettlementReportFileRequestHandlerIntegrationTests : TestBase<SettlementReportFileRequestHandler>,
    IClassFixture<DatabricksSqlStatementApiFixture>
{
    private const string GridAreaA = "805";
    private const string GridAreaB = "111";
    private readonly string[] _gridAreaCodes = [GridAreaA, GridAreaB];
    private readonly Instant _january1St = Instant.FromUtc(2022, 1, 1, 0, 0, 0);
    private readonly Instant _january5Th = Instant.FromUtc(2022, 1, 5, 0, 0, 0);

    private readonly DatabricksSqlStatementApiFixture _databricksSqlStatementApiFixture;
    private readonly SettlementReportFileBlobStorageFixture _settlementReportFileBlobStorageFixture;

    public SettlementReportFileRequestHandlerIntegrationTests(
        DatabricksSqlStatementApiFixture databricksSqlStatementApiFixture,
        SettlementReportFileBlobStorageFixture settlementReportFileBlobStorageFixture)
    {
        _databricksSqlStatementApiFixture = databricksSqlStatementApiFixture;
        _settlementReportFileBlobStorageFixture = settlementReportFileBlobStorageFixture;

        var dataRepository = new LegacySettlementReportDataRepository(new SettlementReportResultQueries(
            databricksSqlStatementApiFixture.GetDatabricksExecutor(),
            databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions));

        Fixture.Inject<ISettlementReportFileGeneratorFactory>(new SettlementReportFileGeneratorFactory(dataRepository));

        var blobContainerClient = settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        Fixture.Inject<ISettlementReportFileRepository>(new SettlementReportFileBlobStorage(blobContainerClient));
    }

    [Fact]
    public async Task RequestFileAsync_ForBalanceFixing_ReturnsExpectedCsv()
    {
        // Arrange
        var filter = new SettlementReportRequestFilterDto(
            _gridAreaCodes.Select(code => new CalculationFilterDto("8E1E8B45-6101-426B-8A0A-B2AB0354A442", code)).ToList(),
            _january1St.ToDateTimeOffset(),
            _january5Th.ToDateTimeOffset(),
            null,
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var fileRequest = new SettlementReportFileRequestDto(
            SettlementReportFileContent.EnergyResultLatestPerDay,
            new SettlementReportPartialFileInfo(Guid.NewGuid().ToString()),
            requestId,
            filter);

        await InsertRowsFromMultipleCalculationsAsync(_databricksSqlStatementApiFixture
            .DatabricksSchemaManager
            .DeltaTableOptions);

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

    // NOTE: This will only work with LegacyRepo. When view arrives, this has to be redone.
    private async Task InsertRowsFromMultipleCalculationsAsync(IOptions<DeltaTableOptions> options)
    {
        const string january1 = "2022-01-01T01:00:00.000Z";
        const string january2 = "2022-01-02T01:00:00.000Z";
        const string january3 = "2022-01-03T01:00:00.000Z";
        const string april1 = "2022-04-01T01:00:00.000Z";
        const string may1 = "2022-05-01T01:00:00.000Z";
        const string june1 = "2022-06-01T01:00:00.000Z";

        // Calculation 1: Balance fixing, ExecutionTime=june1st, Period: 01/01 to 02/01 (include)
        const string quantity11 = "1.100"; // include
        const string quantity12 = "1.200"; // include
        var calculation1Row1 = EnergyResultDeltaTableHelper.CreateRowValues(calculationExecutionTimeStart: may1, time: january1, calculationType: DeltaTableCalculationType.BalanceFixing, gridArea: GridAreaA, quantity: quantity11);
        var calculation1Row2 = EnergyResultDeltaTableHelper.CreateRowValues(calculationExecutionTimeStart: may1, time: january2, calculationType: DeltaTableCalculationType.BalanceFixing, gridArea: GridAreaA, quantity: quantity12);

        // Calculation 2: Same as calculation 1, but for other grid area (include)
        const string quantity21 = "2.100"; // include
        const string quantity22 = "2.200"; // include
        var calculation2Row1 = EnergyResultDeltaTableHelper.CreateRowValues(calculationExecutionTimeStart: may1, time: january1, calculationType: DeltaTableCalculationType.BalanceFixing, gridArea: GridAreaB, quantity: quantity21);
        var calculation2Row2 = EnergyResultDeltaTableHelper.CreateRowValues(calculationExecutionTimeStart: may1, time: january2, calculationType: DeltaTableCalculationType.BalanceFixing, gridArea: GridAreaB, quantity: quantity22);

        // Calculation 3: Same as calculation 1, but only partly covering the same period (include the uncovered part)
        const string quantity31 = "3.100"; // exclude because it's an older calculation
        const string quantity32 = "3.200"; // include because other calculations don't cover this date
        var calculation3Row1 = EnergyResultDeltaTableHelper.CreateRowValues(calculationExecutionTimeStart: april1, time: january2, calculationType: DeltaTableCalculationType.BalanceFixing, gridArea: GridAreaA, quantity: quantity31);
        var calculation3Row2 = EnergyResultDeltaTableHelper.CreateRowValues(calculationExecutionTimeStart: april1, time: january3, calculationType: DeltaTableCalculationType.BalanceFixing, gridArea: GridAreaA, quantity: quantity32);

        // Calculation 4: Same as calculation 1, but newer and for Aggregation (exclude)
        const string quantity41 = "4.100"; // exclude because it's aggregation
        const string quantity42 = "4.200";  // exclude because it's aggregation
        var calculation4Row1 = EnergyResultDeltaTableHelper.CreateRowValues(calculationExecutionTimeStart: june1, time: january1, calculationType: DeltaTableCalculationType.Aggregation, gridArea: GridAreaA, quantity: quantity41);
        var calculation4Row2 = EnergyResultDeltaTableHelper.CreateRowValues(calculationExecutionTimeStart: june1, time: january2, calculationType: DeltaTableCalculationType.Aggregation, gridArea: GridAreaA, quantity: quantity42);

        var rows = new List<IReadOnlyCollection<string>> { calculation1Row1, calculation1Row2, calculation2Row1, calculation2Row2, calculation3Row1, calculation3Row2, calculation4Row1, calculation4Row2 };
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<EnergyResultColumnNames>(options.Value.ENERGY_RESULTS_TABLE_NAME, rows);
    }
}
