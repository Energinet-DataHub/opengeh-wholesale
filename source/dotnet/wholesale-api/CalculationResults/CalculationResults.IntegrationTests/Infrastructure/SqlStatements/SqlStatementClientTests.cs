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
using Energinet.DataHub.Wholesale.Common.DatabricksClient;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SqlStatements;

public class SqlStatementClientTests
{
    private readonly string _someSchemaName = $"TestSchema{Guid.NewGuid().ToString("N")[..8]}"; // TODO: use PR NUMBER
    private readonly string _sometableName = $"TestTable{Guid.NewGuid().ToString("N")[..8]}"; // TODO: use commit ID?

    private readonly DatabricksSqlStatementApiFixture _databricksSqlStatementApiFixture;

    public SqlStatementClientTests(DatabricksSqlStatementApiFixture databricksSqlStatementApiFixture)
    {
        _databricksSqlStatementApiFixture = databricksSqlStatementApiFixture;
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteSqlStatementAsync_WhenQueryFromDatabricks_ReturnsExpectedData(
        [Frozen] Mock<IOptions<DatabricksOptions>> databricksOptionsMock)
    {
        // Arrange
        databricksOptionsMock.Setup(o => o.Value).Returns(_databricksSqlStatementApiFixture.DatabricksOptions);
        var databricksSqlResponseParser = new DatabricksSqlResponseParser();
        var httpClient = new HttpClient();
        var sut = new SqlStatementClient(httpClient, databricksOptionsMock.Object, databricksSqlResponseParser);
        var sqlStatement = "SELECT * FROM myTable";

        // Act
        var response = await sut.ExecuteSqlStatementAsync(sqlStatement);

        // Assert
        response.RowCount.Should().Be(1);
    }
}
