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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.Common.Models;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SettlementReports;

/// <summary>
/// We use an IClassFixture to control the life cycle of the DatabricksSqlStatementApiFixture so:
///   1. It is created and 'InitializeAsync()' is called before the first test in the test class is executed.
///      Use 'InitializeAsync()' to create any schema and seed data.
///   2. 'DisposeAsync()' is called after the last test in the test class has been executed.
///      Use 'DisposeAsync()' to drop any created schema.
/// </summary>
public class SettlementReportResultQueriesTests : IClassFixture<DatabricksSqlStatementApiFixture>, IAsyncLifetime
{
    private readonly DatabricksSqlStatementApiFixture _fixture;

    public SettlementReportResultQueriesTests(DatabricksSqlStatementApiFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.DatabricksSchemaManager.CreateSchemaAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DatabricksSchemaManager.DropSchemaAsync();
    }

    [Fact]
    public async Task GetRowsAsync_ReturnsExpectedReportRow()
    {
        // Arrange
        string[] someGridAreaCodes = { DefaultRow.GridArea };
        const ProcessType someProcessType = ProcessType.BalanceFixing;
        var somePeriodStart = Instant.FromUtc(2022, 5, 16, 1, 0, 0);
        var somePeriodEnd = Instant.FromUtc(2022, 5, 17, 1, 0, 0);
        string? energySupplier = null;

        var tableName = await CreateResultTableWithTwoRowsAsync();

        var sqlStatementClient = new SqlStatementClient(new HttpClient(), _fixture.DatabricksOptionsMock.Object, new DatabricksSqlResponseParser());
        var sut = new SettlementReportResultQueries(sqlStatementClient);

        // Act
        var actual = await sut.GetRowsAsync(someGridAreaCodes, someProcessType, somePeriodStart, somePeriodEnd, energySupplier);

        // Assert
        // actual.
    }

    private async Task<string> CreateResultTableWithRow()
    {
        var tableName = $"TestTable_{DateTime.Now:yyyyMMddHHmmss}";
        var columnDefinition = DeltaTableSchema.Result;
        var values = columnDefinition.Keys.Select(CreateSomeColumnValue).ToList();

        await _fixture.DatabricksSchemaManager.CreateTableAsync(tableName, columnDefinition);
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, values);

        return tableName;
    }

    private static string CreateSomeColumnValue(string columnName)
    {
        return columnName switch
        {
            ResultColumnNames.BatchId => $@"'{DefaultRow.BatchId}'",
            ResultColumnNames.BatchExecutionTimeStart => $@"'{DefaultRow.BatchExecutionTimeStart}'",
            ResultColumnNames.BatchProcessType => $@"'{DefaultRow.BatchProcessType}'",
            ResultColumnNames.TimeSeriesType => $@"'{DefaultRow.TimeSeriesType}'",
            ResultColumnNames.GridArea => $@"'{DefaultRow.GridArea}'",
            ResultColumnNames.FromGridArea => $@"'{DefaultRow.FromGridArea}'",
            ResultColumnNames.BalanceResponsibleId => $@"'{DefaultRow.BalanceResponsibleId}'",
            ResultColumnNames.EnergySupplierId => $@"'{DefaultRow.EnergySupplierId}'",
            ResultColumnNames.Time => $@"'{DefaultRow.Time}'",
            ResultColumnNames.Quantity => $@"{DefaultRow.Quantity}",
            ResultColumnNames.QuantityQuality => $@"'{DefaultRow.QuantityQuality}'",
            ResultColumnNames.AggregationLevel => $@"'{DefaultRow.AggregationLevel}'",
            _ => throw new ArgumentOutOfRangeException($"Unexpected column name: {columnName}."),
        };
    }

    private struct DefaultRow
    {
        public const string BatchId = "ed39dbc5-bdc5-41b9-922a-08d3b12d4538";
        public const string BatchExecutionTimeStart = "2022-03-11T03:00:00.000Z";
        public const string BatchProcessType = DeltaTableProcessType.BalanceFixing;
        public const string TimeSeriesType = DeltaTableTimeSeriesType.Production;
        public const string GridArea = "805";
        public const string FromGridArea = "1236552000028";
        public const string BalanceResponsibleId = "1236552000028";
        public const string EnergySupplierId = "2236552000028";
        public const string Time = "2022-05-16T03:00:00.000Z";
        public const string Quantity = "1.123";
        public const string QuantityQuality = "missing";
        public const string AggregationLevel = "total_ga";
    }
}
