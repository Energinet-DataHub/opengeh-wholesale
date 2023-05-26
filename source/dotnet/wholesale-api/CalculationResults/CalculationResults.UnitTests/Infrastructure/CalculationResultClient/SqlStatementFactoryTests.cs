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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;
using Energinet.DataHub.Wholesale.Common.Models;
using FluentAssertions;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.CalculationResultClient;

[UnitTest]
public class SqlStatementFactoryTests
{
    private readonly string[] _defaultGridAreasCodes = { "123", "234", "345" };
    private readonly Instant _defaultPeriodStart = Instant.FromUtc(2022, 10, 12, 1, 0);
    private readonly Instant _defaultPeriodEnd = Instant.FromUtc(2022, 10, 12, 3, 0);

    [Fact]
    public void CreateForSettlementReport_WhenEnergySupplierIsNull_ReturnsExpectedSqlStatement()
    {
        // Arrange
        var expectedSql = GetExpectedSqlWhenNoEnergySupplier();

        // Act
        var actual = SqlStatementFactory.CreateForSettlementReport(_defaultGridAreasCodes, ProcessType.BalanceFixing, _defaultPeriodStart, _defaultPeriodEnd, null);

        // Assert
        actual.Should().Be(expectedSql);
    }

    [Fact]
    public void CreateForSettlementReport_WhenEnergySupplierIsNotNull_ReturnsExpectedSqlStatement()
    {
        // Arrange
        const string someEnergySupplier = "1234567890123";
        var expectedSql = GetExpectedSqlWhenWithEnergySupplier(someEnergySupplier);

        // Act
        var actual = SqlStatementFactory.CreateForSettlementReport(_defaultGridAreasCodes, ProcessType.BalanceFixing, _defaultPeriodStart, _defaultPeriodEnd, someEnergySupplier);

        // Assert
        actual.Should().Be(expectedSql);
    }

    private static string GetExpectedSqlWhenNoEnergySupplier()
    {
        // This string must match the values of the private members that defines grid area codes, period start and period end
        return $@"
SELECT grid_area, batch_process_type, time, time_series_type, quantity
FROM wholesale_output.result
WHERE
    {ResultColumnNames.GridArea} IN (123,234,345)
    AND {ResultColumnNames.TimeSeriesType} IN ('production','flex_consumption','non_profiled_consumption','net_exchange_per_ga')
    AND {ResultColumnNames.BatchProcessType} = 'BalanceFixing'
    AND {ResultColumnNames.Time} BETWEEN '2022-10-12T01:00:00Z' AND '2022-10-12T03:00:00Z'
    AND {ResultColumnNames.AggregationLevel} = 'total_ga'
ORDER by time
";
    }

    private static string GetExpectedSqlWhenWithEnergySupplier(string energySupplier)
    {
        // This string must match the values of the private members that defines grid area codes, period start and period end
        return $@"
SELECT grid_area, batch_process_type, time, time_series_type, quantity
FROM wholesale_output.result
WHERE
    {ResultColumnNames.GridArea} IN (123,234,345)
    AND {ResultColumnNames.TimeSeriesType} IN ('production','flex_consumption','non_profiled_consumption')
    AND {ResultColumnNames.BatchProcessType} = 'BalanceFixing'
    AND {ResultColumnNames.Time} BETWEEN '2022-10-12T01:00:00Z' AND '2022-10-12T03:00:00Z'
    AND {ResultColumnNames.AggregationLevel} = 'es_ga'
    AND {ResultColumnNames.EnergySupplierId} = '{energySupplier}'
ORDER by time
";
    }
}
