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
using Energinet.DataHub.Wholesale.Batches.IntegrationTests.Fixture.Database;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.Common.DatabricksClient;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SqlStatements;

public class SqlStatementClientTests //: IClassFixture<WholesaleDatabaseFixture<DatabaseContext>>
{
    private readonly DatabricksOptions _databricksOptions = new()
    {
        DATABRICKS_WAREHOUSE_ID = "anyDatabricksId",
        DATABRICKS_WORKSPACE_URL = "https://anyDatabricksUrl",
        DATABRICKS_WORKSPACE_TOKEN = "myToken",
    };

    public SqlStatementClientTests()
    {
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ___(
        [Frozen] Mock<IOptions<DatabricksOptions>> databricksOptionsMock)
    {
        // Arrange
        databricksOptionsMock.Setup(o => o.Value).Returns(_databricksOptions);
        var databricksSqlResponseParser = new DatabricksSqlResponseParser();
        var httpClient = new HttpClient();
        var sqlQueryClient = new SqlStatementClient(httpClient, databricksOptionsMock.Object, databricksSqlResponseParser);
        var sut = new SettlementReportResultQueries(sqlQueryClient);

        // Act
        sut.GetRowsAsync();

        // Assert

    }
}
