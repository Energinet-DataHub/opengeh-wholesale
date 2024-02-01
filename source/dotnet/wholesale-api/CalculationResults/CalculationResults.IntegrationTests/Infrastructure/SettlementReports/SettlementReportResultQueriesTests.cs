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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SettlementReports;

public class SettlementReportResultQueriesTests : TestBase<SettlementReportResultQueries>, IClassFixture<DatabricksSqlStatementApiFixture>
{
    private const CalculationType DefaultCalculationType = CalculationType.BalanceFixing;
    private const string GridAreaA = "805";
    private const string GridAreaB = "111";
    private readonly string[] _gridAreaCodes = { GridAreaA, GridAreaB };
    private readonly DatabricksSqlStatementApiFixture _fixture;
    private readonly Instant _january1St = Instant.FromUtc(2022, 1, 1, 0, 0, 0);
    private readonly Instant _january5Th = Instant.FromUtc(2022, 1, 5, 0, 0, 0);

    public SettlementReportResultQueriesTests(DatabricksSqlStatementApiFixture fixture)
    {
        _fixture = fixture;
        Fixture.Inject(_fixture.DatabricksSchemaManager.DeltaTableOptions);
        Fixture.Inject(_fixture.GetDatabricksExecutor());
    }

    [Fact]
    public async Task GetRowsAsync_ReturnsExpectedReportRows()
    {
        // Arrange
        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        var expectedSettlementReportRow = await InsertRowsFromMultipleCalculationsAsync(deltaTableOptions);

        // Act
        var actual = await Sut.GetRowsAsync(_gridAreaCodes, DefaultCalculationType, _january1St, _january5Th, null);

        // Assert
        actual.Should().BeEquivalentTo(expectedSettlementReportRow);
    }

    private async Task<List<SettlementReportResultRow>> InsertRowsFromMultipleCalculationsAsync(IOptions<DeltaTableOptions> options)
    {
        const string january1st = "2022-01-01T01:00:00.000Z";
        const string january2nd = "2022-01-02T01:00:00.000Z";
        const string january3rd = "2022-01-03T01:00:00.000Z";
        const string april1st = "2022-04-01T01:00:00.000Z";
        const string may1st = "2022-05-01T01:00:00.000Z";
        const string june1st = "2022-06-01T01:00:00.000Z";

        // Calculation 1: Balance fixing, ExecutionTime=june1st, Period: 01/01 to 02/01 (include)
        const string quantity11 = "1.100"; // include
        const string quantity12 = "1.200"; // include
        var calculation1Row1 = EnergyResultDeltaTableHelper.CreateRowValues(calculationExecutionTimeStart: may1st, time: january1st, calculationType: DeltaTableCalculationType.BalanceFixing, gridArea: GridAreaA, quantity: quantity11);
        var calculation1Row2 = EnergyResultDeltaTableHelper.CreateRowValues(calculationExecutionTimeStart: may1st, time: january2nd, calculationType: DeltaTableCalculationType.BalanceFixing, gridArea: GridAreaA, quantity: quantity12);

        // Calculation 2: Same as calculation 1, but for other grid area (include)
        const string quantity21 = "2.100"; // include
        const string quantity22 = "2.200"; // include
        var calculation2Row1 = EnergyResultDeltaTableHelper.CreateRowValues(calculationExecutionTimeStart: may1st, time: january1st, calculationType: DeltaTableCalculationType.BalanceFixing, gridArea: GridAreaB, quantity: quantity21);
        var calculation2Row2 = EnergyResultDeltaTableHelper.CreateRowValues(calculationExecutionTimeStart: may1st, time: january2nd, calculationType: DeltaTableCalculationType.BalanceFixing, gridArea: GridAreaB, quantity: quantity22);

        // Calculation 3: Same as calculation 1, but only partly covering the same period (include the uncovered part)
        const string quantity31 = "3.100"; // exclude because it's an older calculation
        const string quantity32 = "3.200"; // include because other calculations don't cover this date
        var calculation3Row1 = EnergyResultDeltaTableHelper.CreateRowValues(calculationExecutionTimeStart: april1st, time: january2nd, calculationType: DeltaTableCalculationType.BalanceFixing, gridArea: GridAreaA, quantity: quantity31);
        var calculation3Row2 = EnergyResultDeltaTableHelper.CreateRowValues(calculationExecutionTimeStart: april1st, time: january3rd, calculationType: DeltaTableCalculationType.BalanceFixing, gridArea: GridAreaA, quantity: quantity32);

        // Calculation 4: Same as calculation 1, but newer and for Aggregation (exclude)
        const string quantity41 = "4.100"; // exclude because it's aggregation
        const string quantity42 = "4.200";  // exclude because it's aggregation
        var calculation4Row1 = EnergyResultDeltaTableHelper.CreateRowValues(calculationExecutionTimeStart: june1st, time: january1st, calculationType: DeltaTableCalculationType.Aggregation, gridArea: GridAreaA, quantity: quantity41);
        var calculation4Row2 = EnergyResultDeltaTableHelper.CreateRowValues(calculationExecutionTimeStart: june1st, time: january2nd, calculationType: DeltaTableCalculationType.Aggregation, gridArea: GridAreaA, quantity: quantity42);

        var rows = new List<IReadOnlyCollection<string>> { calculation1Row1, calculation1Row2, calculation2Row1, calculation2Row2, calculation3Row1, calculation3Row2, calculation4Row1, calculation4Row2, };
        await _fixture.DatabricksSchemaManager.InsertAsync<EnergyResultColumnNames>(options.Value.ENERGY_RESULTS_TABLE_NAME, rows);

        var expectedSettlementReportRows = new List<SettlementReportResultRow>
        {
            GetSettlementReportRow(GridAreaA, january1st, quantity11),
            GetSettlementReportRow(GridAreaA, january2nd, quantity12),
            GetSettlementReportRow(GridAreaA, january3rd, quantity32),
            GetSettlementReportRow(GridAreaB, january1st, quantity21),
            GetSettlementReportRow(GridAreaB, january2nd, quantity22),
        };

        return expectedSettlementReportRows;
    }

    private static SettlementReportResultRow GetSettlementReportRow(string gridArea, string time, string quantity)
    {
        return new SettlementReportResultRow(
            gridArea,
            CalculationType.BalanceFixing,
            SqlResultValueConverters.ToInstant(time) ?? throw new Exception("Could not parse time"),
            "PT15M",
            MeteringPointType.Production,
            null,
            SqlResultValueConverters.ToDecimal(quantity) ?? throw new Exception("Could not parse time"));
    }
}
