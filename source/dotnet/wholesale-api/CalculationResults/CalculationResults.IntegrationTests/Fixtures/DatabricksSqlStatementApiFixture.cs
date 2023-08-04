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

using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;

/// <summary>
/// The interface 'IAsyncLifetime' forces the implementation of two methods.
///   1. 'InitializeAsync()' which is called before the first test in the test class is executed.
///   2. 'DisposeAsync()' which is called after the last test in the test class has been executed.
/// </summary>
public class DatabricksSqlStatementApiFixture : IAsyncLifetime
{
    public DatabricksSqlStatementApiFixture()
    {
        var integrationTestConfiguration = new IntegrationTestConfiguration();
        DatabricksSchemaManager = new DatabricksSchemaManager(integrationTestConfiguration.DatabricksSettings, "wholesale");
        DatabricksOptionsMock = CreateDatabricksOptionsMock(DatabricksSchemaManager);
    }

    public DatabricksSchemaManager DatabricksSchemaManager { get; }

    private Mock<IOptions<DatabricksOptions>> DatabricksOptionsMock { get; }

    public async Task InitializeAsync()
    {
        await DatabricksSchemaManager.CreateSchemaAsync();
    }

    public async Task DisposeAsync()
    {
        await DatabricksSchemaManager.DropSchemaAsync();
    }

    public SqlStatementClient CreateSqlStatementClient(Mock<ILogger<DatabricksSqlStatusResponseParser>> loggerMock)
    {
        var databricksSqlChunkResponseParser = new DatabricksSqlChunkResponseParser();
        var sqlStatementClient = new SqlStatementClient(
            new HttpClient(),
            DatabricksOptionsMock.Object,
            new DatabricksSqlResponseParser(
                new DatabricksSqlStatusResponseParser(loggerMock.Object, databricksSqlChunkResponseParser),
                databricksSqlChunkResponseParser,
                new DatabricksSqlChunkDataResponseParser()));
        return sqlStatementClient;
    }

    private static Mock<IOptions<DatabricksOptions>> CreateDatabricksOptionsMock(DatabricksSchemaManager databricksSchemaManager)
    {
        var databricksOptionsMock = new Mock<IOptions<DatabricksOptions>>();
        databricksOptionsMock
            .Setup(o => o.Value)
            .Returns(new DatabricksOptions
            {
                DATABRICKS_WORKSPACE_URL = databricksSchemaManager.Settings.WorkspaceUrl,
                DATABRICKS_WORKSPACE_TOKEN = databricksSchemaManager.Settings.WorkspaceAccessToken,
                DATABRICKS_WAREHOUSE_ID = databricksSchemaManager.Settings.WarehouseId,
            });

        return databricksOptionsMock;
    }
}
