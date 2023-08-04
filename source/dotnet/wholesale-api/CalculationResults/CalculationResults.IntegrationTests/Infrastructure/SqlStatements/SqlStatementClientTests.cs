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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SqlStatements;

public class SqlStatementClientTests : IClassFixture<DatabricksSqlStatementApiFixture>
{
    private readonly DatabricksSqlStatementApiFixture _fixture;

    public SqlStatementClientTests(DatabricksSqlStatementApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteSqlStatementAsync_WhenQueryFromDatabricks_ReturnsExpectedData(
        Mock<ILogger<DatabricksSqlStatusResponseParser>> loggerMock)
    {
        // Arrange
        await AddDataToResultTableAsync();
        var sut = _fixture.CreateSqlStatementClient(loggerMock);

        var sqlStatement = $@"SELECT * FROM {_fixture.DatabricksSchemaManager.SchemaName}.result";

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
        const int expectedRowCount = 100;
        var sut = _fixture.CreateSqlStatementClient(loggerMock);

        // Arrange: The result of this query spans multiple chunks
        var sqlStatement = $@"select r.id, 'some value' as value from range({expectedRowCount}) as r";

        // Act
        var actual = await sut.ExecuteAsync(sqlStatement).CountAsync();

        // Assert
        actual.Should().Be(expectedRowCount);
    }

    private async Task AddDataToResultTableAsync()
    {
        var values = GetSomeDeltaTableRow();
        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(deltaTableOptions.Value.RESULT_TABLE_NAME, values);
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(deltaTableOptions.Value.RESULT_TABLE_NAME, values);
    }

    private static List<string> GetSomeDeltaTableRow()
    {
        var values = new List<string>
        {
            "'123'",
            "'energy_supplier_id'",
            "'balance_responsible_id'",
            "1.23",
            "'missing'",
            "'2022-03-11T03:00:00.000Z'",
            "'total_ga'",
            "'grid_loss'",
            "'batch_id'",
            "'BalanceFixing'",
            "'2022-03-11T03:00:00.000Z'",
            "'123'",
            "'calculation_result_id'",
        };

        return values;
    }
}
