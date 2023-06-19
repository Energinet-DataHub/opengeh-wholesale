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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SqlStatements;

/// <summary>
/// We use an IClassFixture to control the life cycle of the DatabricksSqlStatementApiFixture so:
///   1. It is created and 'InitializeAsync()' is called before the first test in the test class is executed.
///      Use 'InitializeAsync()' to create any schema and seed data.
///   2. 'DisposeAsync()' is called after the last test in the test class has been executed.
///      Use 'DisposeAsync()' to drop any created schema.
/// </summary>
public class SqlStatementClientTests : IClassFixture<DatabricksSqlStatementApiFixture>, IAsyncLifetime
{
    private readonly DatabricksSqlStatementApiFixture _fixture;

    public SqlStatementClientTests(DatabricksSqlStatementApiFixture fixture)
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

    private string SchemaName => _fixture.DatabricksSchemaManager.SchemaName;

    [Fact]
    public async Task ExecuteSqlStatementAsync_WhenQueryFromDatabricks_ReturnsExpectedData()
    {
        // Arrange
        var tableName = await CreateResultTableWithTwoRowsAsync();

        var sut = new SqlStatementClient(new HttpClient(), _fixture.DatabricksOptionsMock.Object, new DatabricksSqlResponseParser());
        var sqlStatement = $@"SELECT * FROM {SchemaName}.{tableName}";

        // Act
        var actual = await sut.ExecuteAsync(sqlStatement).ToListAsync();

        // Assert
        actual.Count.Should().Be(2);
    }

    private async Task<string> CreateResultTableWithTwoRowsAsync()
    {
        var tableName = $"TestTable_{Guid.NewGuid().ToString("N")[..8]}";
        var columnDefinition = DeltaTableSchema.Result;

        var values = CreateSomeRow(DeltaTableSchema.Result.Keys);

        await _fixture.DatabricksSchemaManager.CreateTableAsync(tableName, columnDefinition);
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, values);
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, values);

        return tableName;
    }

    private static string CreateSomeRow(IEnumerable<string> columnNames)
    {
        var valueCollection = columnNames.Select(CreateSomeColumnValue).ToList();
        return @$"({string.Join(",", valueCollection)})"; // Example: ('805', 1.0, 2022-05-16T03:00:00.000Z)
    }

    private static string CreateSomeColumnValue(string columnName)
    {
        return columnName switch
        {
            ResultColumnNames.BatchId => "'ed39dbc5-bdc5-41b9-922a-08d3b12d4538'",
            ResultColumnNames.BatchExecutionTimeStart => "'2022-03-11T03:00:00.000Z'",
            ResultColumnNames.BatchProcessType => $@"'{DeltaTableProcessType.BalanceFixing}'",
            ResultColumnNames.TimeSeriesType => $@"'{DeltaTableTimeSeriesType.Production}'",
            ResultColumnNames.GridArea => "'805'",
            ResultColumnNames.FromGridArea => "'806'",
            ResultColumnNames.BalanceResponsibleId => "'1236552000028'",
            ResultColumnNames.EnergySupplierId => "'1236552000027'",
            ResultColumnNames.Time => "'2022-05-16T03:00:00.000Z'",
            ResultColumnNames.Quantity => "1.234",
            ResultColumnNames.QuantityQuality => "'measured'",
            ResultColumnNames.AggregationLevel => $@"'{DeltaTableAggregationLevel.GridArea}'",
            _ => throw new ArgumentOutOfRangeException($"Unexpected column name: {columnName}."),
        };
    }
}
