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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.CalculationResults;

/// <summary>
/// We use an IClassFixture to control the life cycle of the DatabricksSqlStatementApiFixture so:
///   1. It is created and 'InitializeAsync()' is called before the first test in the test class is executed.
///      Use 'InitializeAsync()' to create any schema and seed data.
///   2. 'DisposeAsync()' is called after the last test in the test class has been executed.
///      Use 'DisposeAsync()' to drop any created schema.
/// </summary>
public class CalculationResultQueriesTests : IClassFixture<DatabricksSqlStatementApiFixture>, IAsyncLifetime
{
    private readonly DatabricksSqlStatementApiFixture _fixture;

    public CalculationResultQueriesTests(DatabricksSqlStatementApiFixture fixture)
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

    [Theory]
    [InlineAutoMoqData]
    public async Task GetRowsAsync_ReturnsExpectedReportRow(Mock<ILogger<DatabricksSqlResponseParser>> loggerMock)
    {
        // Arrange
        var tableName = await CreateTableWithTwoRowsAsync();
        var sqlStatementClient = new SqlStatementClient(new HttpClient(), _fixture.DatabricksOptionsMock.Object, new DatabricksSqlResponseParser(loggerMock.Object));
        var sut = new CalculationResultQueries(sqlStatementClient, , CreateDeltaTableOptions(_fixture.DatabricksSchemaManager.SchemaName, tableName));

        // Act
        var actual = await sut.GetRowsAsync(_defaultGridAreaCodes, DefaultProcessType, _defaultPeriodStart, _defaultPeriodEnd, null);

        // Assert
        // var actualList = actual.ToList();
        // actualList.Should().HaveCount(1);
        // actualList.First().Should().Be(expectedSettlementReportRow);
    }

    // private async Task<string> CreateTableWithTwoRowsAsync()
    // {
    //     var tableName = await _fixture.DatabricksSchemaManager.DatabricksTableManager..CreateTableAsync();
    //
    //     var row1 = _fixture.DatabricksTableManager.CreateRowValues(gridArea: DefaultGridArea);
    //     await _fixture.DatabricksTableManager.InsertRow(tableName, row1);
    //
    //     var row2 = _fixture.DatabricksTableManager.CreateRowValues(gridArea: SomeOtherGridArea);
    //     await _fixture.DatabricksTableManager.InsertRow(tableName, row2);
    //
    //     return tableName;
    // }

    // private string SchemaName => _fixture.DatabricksSchemaManager.SchemaName;
    //
    // [Theory]
    // [InlineAutoMoqData]
    // public async Task GetRowsAsync_ReturnsExpectedReportRow(Mock<ILogger<DatabricksSqlResponseParser>> loggerMock)
    // {
    //     // Arrange
    //     var tableName = await CreateTableTwoRowsAsync();
    //     var sqlStatementClient = new SqlStatementClient(new HttpClient(), _fixture.DatabricksOptionsMock.Object, new DatabricksSqlResponseParser(loggerMock.Object));
    //     var sut = new SettlementReportResultQueries(sqlStatementClient, CreateDeltaTableOptions(_fixture.DatabricksSchemaManager.SchemaName, tableName));
    //
    //     // Act
    //     var actual = await sut.GetRowsAsync(_defaultGridAreaCodes, DefaultProcessType, _defaultPeriodStart, _defaultPeriodEnd, null);
    //
    //     // Assert
    //     var actualList = actual.ToList();
    //     actualList.Should().HaveCount(1);
    //     actualList.First().Should().Be(expectedSettlementReportRow);
    // }
    //
    // private async Task<string> CreateTableTwoRowsAsync()
    // {
    //     var tableName = $"TestTable_{DateTime.Now:yyyyMMddHHmmss}";
    //     await _fixture.DatabricksSchemaManager.CreateTableAsync(tableName, _tableColumnDefinitions);
    //     await InsertRow(tableName, DefaultGridArea);
    //     await InsertRow(tableName, SomeOtherGridArea);
    //     return tableName;
    // }
    //
    // private async Task InsertRow(string tableName, string gridArea)
    // {
    //     var rowValues = CreateRowValues(_tableColumnDefinitions.Keys, gridArea);
    //     await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, rowValues);
    // }
    //
    // private static IEnumerable<string> CreateRowValues(IEnumerable<string> columnNames, string calculationResultId, string time, string gridArea)
    // {
    //     var values = columnNames.Select(columnName => columnName switch
    //     {
    //         ResultColumnNames.BatchId => "'ed39dbc5-bdc5-41b9-922a-08d3b12d4538'",
    //         ResultColumnNames.BatchExecutionTimeStart => "'2022-03-11T03:00:00.000Z'",
    //         ResultColumnNames.BatchProcessType => $@"'{DeltaTableProcessType.BalanceFixing}'",
    //         ResultColumnNames.CalculationResultId => $@"'{calculationResultId}'",
    //         ResultColumnNames.TimeSeriesType => $@"'{DeltaTableTimeSeriesType.Production}'",
    //         ResultColumnNames.GridArea => $@"'{gridArea}'",
    //         ResultColumnNames.FromGridArea => "'406'",
    //         ResultColumnNames.BalanceResponsibleId => "'1236552000028'",
    //         ResultColumnNames.EnergySupplierId => "'2236552000028'",
    //         ResultColumnNames.Time => $@"'{time}'",
    //         ResultColumnNames.Quantity => "1.123",
    //         ResultColumnNames.QuantityQuality => "'missing'",
    //         ResultColumnNames.AggregationLevel => "'total_ga'",
    //         _ => throw new ArgumentOutOfRangeException($"Unexpected column name: {columnName}."),
    //     });
    //
    //     return values;
    // }
    //
    // private async Task<string> CreateResultTableWithTwoRowsAsync()
    // {
    //     var tableName = $"TestTable_{DateTime.Now:yyyyMMddHHmmss}";
    //     var (someColumnDefinition, values) = GetSomeDeltaTableRow();
    //
    //     await _fixture.DatabricksSchemaManager.CreateTableAsync(tableName, someColumnDefinition);
    //     await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, values);
    //     await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, values);
    //
    //     return tableName;
    // }
    //
    // private static (Dictionary<string, string> ColumnDefintion, List<string> Values) GetSomeDeltaTableRow()
    // {
    //     var dictionary = new Dictionary<string, string>
    //     {
    //         { "someTimeColumn",  "TIMESTAMP" },
    //         { "someStringColumn", "STRING" },
    //         { "someDecimalColumn", "DECIMAL(18,3)" },
    //     };
    //
    //     var values = new List<string>
    //     {
    //         "'2022-03-11T03:00:00.000Z'",
    //         "'measured'",
    //         "1.234",
    //     };
    //
    //     return (dictionary, values);
    // }
}
