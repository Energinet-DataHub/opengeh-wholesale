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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SettlementReport;

public class SettlementReportSqlStatementFactoryTests
{
    private readonly string[] _defaultGridAreasCodes = { "123", "234", "345" };
    private readonly Instant _defaultPeriodStart = Instant.FromUtc(2022, 10, 12, 1, 0);
    private readonly Instant _defaultPeriodEnd = Instant.FromUtc(2022, 10, 12, 3, 0);
    private readonly string _schemaName = new DeltaTableOptions().SCHEMA_NAME;
    private readonly string _tableName = new DeltaTableOptions().ENERGY_RESULTS_TABLE_NAME;

    [Fact]
    public void Create_WhenEnergySupplierIsNull_ReturnsExpectedSqlStatement()
    {
        // Arrange
        var expectedSql = GetExpectedSqlWhenNoEnergySupplier();

        // Act
        var actual = SettlementReportSqlStatementFactory.Create(_schemaName, _tableName, _defaultGridAreasCodes, CalculationType.BalanceFixing, _defaultPeriodStart, _defaultPeriodEnd, null);

        // Assert
        actual.Should().Be(expectedSql);
    }

    [Fact]
    public void Create_WhenEnergySupplierIsNotNull_ReturnsExpectedSqlStatement()
    {
        // Arrange
        const string someEnergySupplier = "1234567890123";
        var expectedSql = GetExpectedSqlWhenWithEnergySupplier(someEnergySupplier);

        // Act
        var actual = SettlementReportSqlStatementFactory.Create(_schemaName, _tableName, _defaultGridAreasCodes, CalculationType.BalanceFixing, _defaultPeriodStart, _defaultPeriodEnd, someEnergySupplier);

        // Assert
        actual.Should().Be(expectedSql);
    }

    private string GetExpectedSqlWhenNoEnergySupplier()
    {
        // This string must match the values of the private members that defines grid area codes, period start and period end
        return $@"
SELECT t1.grid_area, t1.{EnergyResultColumnNames.CalculationType}, t1.time, t1.time_series_type, t1.quantity
FROM {_schemaName}.{_tableName} t1
LEFT JOIN {_schemaName}.{_tableName} t2
    ON t1.time = t2.time
        AND t1.{EnergyResultColumnNames.CalculationExecutionTimeStart} < t2.{EnergyResultColumnNames.CalculationExecutionTimeStart}
        AND t1.grid_area = t2.grid_area
        AND COALESCE(t1.out_grid_area, 'N/A') = COALESCE(t2.out_grid_area, 'N/A')
        AND t1.time_series_type = t2.time_series_type
        AND t1.{EnergyResultColumnNames.CalculationType} = t2.{EnergyResultColumnNames.CalculationType}
        AND t1.aggregation_level = t2.aggregation_level
WHERE t2.time IS NULL
    AND t1.{EnergyResultColumnNames.GridArea} IN (123,234,345)
    AND t1.{EnergyResultColumnNames.TimeSeriesType} IN ('production','flex_consumption','non_profiled_consumption','net_exchange_per_ga','total_consumption')
    AND t1.{EnergyResultColumnNames.CalculationType} = 'BalanceFixing'
    AND t1.{EnergyResultColumnNames.Time} BETWEEN '2022-10-12T01:00:00Z' AND '2022-10-12T03:00:00Z'
    AND t1.{EnergyResultColumnNames.AggregationLevel} = '{DeltaTableAggregationLevel.GridArea}'
ORDER BY t1.time
";
    }

    private string GetExpectedSqlWhenWithEnergySupplier(string energySupplier)
    {
        // This string must match the values of the private members that defines grid area codes, period start and period end
        return $@"
SELECT t1.grid_area, t1.{EnergyResultColumnNames.CalculationType}, t1.time, t1.time_series_type, t1.quantity
FROM {_schemaName}.{_tableName} t1
LEFT JOIN {_schemaName}.{_tableName} t2
    ON t1.time = t2.time
        AND t1.{EnergyResultColumnNames.CalculationExecutionTimeStart} < t2.{EnergyResultColumnNames.CalculationExecutionTimeStart}
        AND t1.grid_area = t2.grid_area
        AND COALESCE(t1.out_grid_area, 'N/A') = COALESCE(t2.out_grid_area, 'N/A')
        AND t1.time_series_type = t2.time_series_type
        AND t1.{EnergyResultColumnNames.CalculationType} = t2.{EnergyResultColumnNames.CalculationType}
        AND t1.aggregation_level = t2.aggregation_level
WHERE t2.time IS NULL
    AND t1.{EnergyResultColumnNames.GridArea} IN (123,234,345)
    AND t1.{EnergyResultColumnNames.TimeSeriesType} IN ('production','flex_consumption','non_profiled_consumption')
    AND t1.{EnergyResultColumnNames.CalculationType} = 'BalanceFixing'
    AND t1.{EnergyResultColumnNames.Time} BETWEEN '2022-10-12T01:00:00Z' AND '2022-10-12T03:00:00Z'
    AND t1.{EnergyResultColumnNames.AggregationLevel} = '{DeltaTableAggregationLevel.EnergySupplierAndGridArea}'
    AND t1.{EnergyResultColumnNames.EnergySupplierId} = '{energySupplier}'
ORDER BY t1.time
";
    }
}
