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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Energinet.DataHub.Wholesale.Common.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SettlementReport;

public class SettlementReportResultQueriesTests
{
    private readonly Instant _somePeriodStart = Instant.FromUtc(2021, 3, 1, 10, 15);
    private readonly Instant _somePeriodEnd = Instant.FromUtc(2021, 3, 31, 10, 15);
    private readonly string[] _someGridAreas = { "123", "456", };
    private readonly TableChunk _someTableChunk = TableTestHelper.CreateTableForSettlementReport(3);
    private readonly string[] _columnNames = { EnergyResultColumnNames.GridArea, EnergyResultColumnNames.BatchProcessType, EnergyResultColumnNames.Time, EnergyResultColumnNames.TimeSeriesType, EnergyResultColumnNames.Quantity, };
    private readonly IOptions<DeltaTableOptions> _someDeltaTableOptions = Options.Create(new DeltaTableOptions { SCHEMA_NAME = "someSchema", ENERGY_RESULTS_TABLE_NAME = "someTable", });

    [Theory]
    [AutoMoqData]
    public async Task GetRowsAsync_ReturnsExpectedNumberOfRows(
        Mock<DatabricksSqlWarehouseQueryExecutor> databricksSqlWarehouseQueryExecutorMock)
    {
        // Arrange
        var asyncResult = ToAsyncEnumerable(_someTableChunk);
        databricksSqlWarehouseQueryExecutorMock.Setup(s => s.ExecuteStatementAsync(It.IsAny<DatabricksStatement>())).Returns(asyncResult);
        var sut = new SettlementReportResultQueries(databricksSqlWarehouseQueryExecutorMock.Object, _someDeltaTableOptions);

        // Act
        var actual = await sut.GetRowsAsync(_someGridAreas, ProcessType.BalanceFixing, _somePeriodStart, _somePeriodEnd, null);

        // Assert
        actual.Count().Should().Be(_someTableChunk.RowCount);
    }

    [Theory]
    [AutoMoqData]
    public async Task GetRowsAsync_ReturnsExpectedData(
        Mock<DatabricksSqlWarehouseQueryExecutor> databricksSqlWarehouseQueryExecutorMock)
    {
        // Arrange
        var row = new[] { "123", "BalanceFixing", "2022-05-16T01:00:00.000Z", "non_profiled_consumption", "1.234" };
        var expected = new SettlementReportResultRow(
            "123",
            ProcessType.BalanceFixing,
            Instant.FromUtc(2022, 5, 16, 1, 0, 0),
            "PT15M",
            MeteringPointType.Consumption,
            SettlementMethod.NonProfiled,
            1.234m);
        var table = new TableChunk(_columnNames,  new List<string[]> { row });
        var asyncResult = ToAsyncEnumerable(table);
        databricksSqlWarehouseQueryExecutorMock.Setup(s => s.ExecuteStatementAsync(It.IsAny<DatabricksStatement>())).Returns(asyncResult);
        var sut = new SettlementReportResultQueries(databricksSqlWarehouseQueryExecutorMock.Object, _someDeltaTableOptions);

        // Act
        var actual = await sut.GetRowsAsync(_someGridAreas, ProcessType.BalanceFixing, _somePeriodStart, _somePeriodEnd, null);

        // Assert
        actual.First().Should().Be(expected);
    }

    private static async IAsyncEnumerable<SqlResultRow> ToAsyncEnumerable(TableChunk tableChunk)
    {
        for (var index = 0; index < tableChunk.RowCount; index++)
            yield return new SqlResultRow(tableChunk, index);

        await Task.Delay(0);
    }
}
