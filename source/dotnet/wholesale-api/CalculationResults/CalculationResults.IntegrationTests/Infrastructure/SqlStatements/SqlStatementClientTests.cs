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

using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.Common.DatabricksClient;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SqlStatements;

/// <summary>
/// We use a IClassFixture to control the life cycle of the DatabricksSqlStatementApiFixture so:
///   1. It is created and 'InitializeAsync()' is called before the first test in the test class is executed.
///      Use 'InitializeAsync()' to create any schema and seed data.
///   2. 'DisposeAsync()' is called after the last test in the test class has been executed.
///      Use 'DisposeAsync()' to drop any created schema.
/// </summary>
public class SqlStatementClientTests : IClassFixture<DatabricksSqlStatementApiFixture>
{
    private readonly string _schemaName = $"TestSchema{Guid.NewGuid().ToString("N")[..8]}"; // TODO: use PR NUMBER
    private readonly string _sometableName = $"TestTable{Guid.NewGuid().ToString("N")[..8]}"; // TODO: use commit ID?

    private readonly DatabricksSqlStatementApiFixture _fixture;

    public SqlStatementClientTests(DatabricksSqlStatementApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Test()
    {
       _fixture.DatabricksWarehouseManager.Settings.WarehouseId.Should().NotBeNull();
    }

    [Fact]
    public async Task Test2()
    {
        await _fixture.DatabricksWarehouseManager.CreateSchemaAsync("testSchema");
    }

    [Fact]
    public async Task ExecuteSqlStatementAsync_WhenQueryFromDatabricks_ReturnsExpectedData()
    {
        // Arrange
        var databricksSqlResponseParser = new DatabricksSqlResponseParser();
        var httpClient = new HttpClient();
        var sut = new SqlStatementClient(httpClient, _fixture.DatabricksOptionsMock.Object, databricksSqlResponseParser);
        const string sqlStatement = "SELECT * FROM myTable";

        // Act
        var response = await sut.ExecuteSqlStatementAsync(sqlStatement);

        // Assert
        response.RowCount.Should().Be(1);
    }
}
