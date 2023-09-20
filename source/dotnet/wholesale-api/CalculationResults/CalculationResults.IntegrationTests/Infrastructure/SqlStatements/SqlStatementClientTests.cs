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

public class SqlStatementClientTests
{
    private readonly DatabricksSqlStatementApiFixture _fixture;
    private readonly ILogger<DatabricksSqlStatusResponseParser> _loggerResponseParserStub;
    private readonly ILogger<SqlStatementClient> _loggerSqlClientStub;

    public SqlStatementClientTests(Mock<ILogger<DatabricksSqlStatusResponseParser>> loggerResponseParserStub, Mock<ILogger<SqlStatementClient>> loggerSqlClientStub)
    {
        _fixture = new DatabricksSqlStatementApiFixture();
        _loggerResponseParserStub = loggerResponseParserStub.Object;
        _loggerSqlClientStub = loggerSqlClientStub.Object;
    }

    [Fact]
    public async Task ExecuteSqlStatementAsync_WhenQueryFromDatabricks_ReturnsExpectedData()
    {
        // Arrange
        await AddDataToEnergyResultTableAsync(_fixture.DatabricksSchemaManager);
        var sut = _fixture.CreateSqlStatementClient(_loggerResponseParserStub, _loggerSqlClientStub);

        var sqlStatement = $@"SELECT * FROM {_fixture.DatabricksSchemaManager.SchemaName}.{_fixture.DatabricksSchemaManager.EnergyResultTableName}";

        // Act
        var actual = await sut.ExecuteAsync(sqlStatement).ToListAsync();

        // Assert
        actual.Count.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMultipleChunks_ReturnsAllRows()
    {
        // Arrange
        const int expectedRowCount = 100;
        var sut = _fixture.CreateSqlStatementClient(_loggerResponseParserStub, _loggerSqlClientStub);

        // Arrange: The result of this query spans multiple chunks
        var sqlStatement = $@"select r.id, 'some value' as value from range({expectedRowCount}) as r";

        // Act
        var actual = await sut.ExecuteAsync(sqlStatement).CountAsync();

        // Assert
        actual.Should().Be(expectedRowCount);
    }

    private static async Task AddDataToEnergyResultTableAsync(DatabricksSchemaManager databricksSchemaManager)
    {
        var values = GetSomeEnergyResultDeltaTableRow();
        var deltaTableOptions = databricksSchemaManager.DeltaTableOptions;
        await databricksSchemaManager.InsertAsync<EnergyResultColumnNames>(deltaTableOptions.Value.ENERGY_RESULTS_TABLE_NAME, values);
        await databricksSchemaManager.InsertAsync<EnergyResultColumnNames>(deltaTableOptions.Value.ENERGY_RESULTS_TABLE_NAME, values);
    }

    private static IList<string> GetSomeEnergyResultDeltaTableRow()
    {
        const string time = "2022-03-11T03:00:00.000Z";
        const string batchExecutionTimeStart = "2022-03-11T03:00:00.000Z";
        const string gridAreaB = "123";
        const string quantity21 = "1.23";
        var row = EnergyResultDeltaTableHelper.CreateRowValues(
            batchExecutionTimeStart: batchExecutionTimeStart,
            time: time,
            batchProcessType: DeltaTableProcessType.BalanceFixing,
            gridArea: gridAreaB,
            quantity: quantity21);

        return row.ToList();
    }
}
