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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SqlStatements;

public class SqlStatementClientTests : IAsyncLifetime
{
    private readonly DatabricksSqlStatementApiFixture _fixture;

    public SqlStatementClientTests(DatabricksSqlStatementApiFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        // Called once per test. This is important to avoid the tests to interfere with each other.
        await _fixture.DatabricksSchemaManager.CreateSchemaAsync();
    }

    public async Task DisposeAsync()
    {
        // Called once per test. This is important to avoid the tests to interfere with each other.
        await _fixture.DatabricksSchemaManager.DropSchemaAsync();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteSqlStatementAsync_WhenQueryFromDatabricks_ReturnsExpectedData(
        Mock<ILogger<DatabricksSqlStatusResponseParser>> loggerMock)
    {
        // Arrange
        await AddDataToEnergyResultTableAsync();
        var sut = _fixture.CreateSqlStatementClient(loggerMock, new Mock<ILogger<SqlStatementClient>>());

        var sqlStatement = $@"SELECT * FROM {_fixture.DatabricksSchemaManager.SchemaName}.{_fixture.DatabricksSchemaManager.EnergyResultTableName}";

        // Act
        var actual = await sut.ExecuteAsync(sqlStatement).ToListAsync();

        // Assert
        actual.Count.Should().Be(2);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteAsync_WhenMultipleChunks_ReturnsAllRows(Mock<ILogger<DatabricksSqlStatusResponseParser>> loggerMock)
    {
        // Arrange
        var expectedRowCount = 100;
        var sut = _fixture.CreateSqlStatementClient(loggerMock, new Mock<ILogger<SqlStatementClient>>());

        // Arrange: The result of this query spans multiple chunks
        var sqlStatement = $@"select r.id, 'some value' as value from range({expectedRowCount}) as r";

        // Act
        var actual = await sut.ExecuteAsync(sqlStatement).CountAsync();

        // Assert
        actual.Should().Be(expectedRowCount);
    }

    private async Task AddDataToEnergyResultTableAsync()
    {
        var values = GetSomeEnergyResultDeltaTableRow();
        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await _fixture.DatabricksSchemaManager.InsertAsync<EnergyResultColumnNames>(deltaTableOptions.Value.ENERGY_RESULTS_TABLE_NAME, values);
        await _fixture.DatabricksSchemaManager.InsertAsync<EnergyResultColumnNames>(deltaTableOptions.Value.ENERGY_RESULTS_TABLE_NAME, values);
    }

    private static IList<string> GetSomeEnergyResultDeltaTableRow()
    {
        var time = "2022-03-11T03:00:00.000Z";
        var batchExecutionTimeStart = "2022-03-11T03:00:00.000Z";
        var gridAreaB = "123";
        var quantity21 = "1.23";
        var row = EnergyResultDeltaTableHelper.CreateRowValues(
            batchExecutionTimeStart: batchExecutionTimeStart,
            time: time,
            batchProcessType: DeltaTableProcessType.BalanceFixing,
            gridArea: gridAreaB,
            quantity: quantity21);

        return row.ToList();
    }
}
