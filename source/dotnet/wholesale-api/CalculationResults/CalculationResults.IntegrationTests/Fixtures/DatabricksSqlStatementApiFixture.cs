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

using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures.TestCommon;
using Energinet.DataHub.Wholesale.Common.DatabricksClient;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;

public class DatabricksSqlStatementApiFixture : IAsyncLifetime
{
    public DatabricksSqlStatementApiFixture()
    {
        var integrationTestConfiguration = new IntegrationTestConfiguration();
        var databricksWarehouseSettings = new DatabricksWarehouseSettings
        {
            // We have to build the URL here in code as currently this is also what happens in the infrastructure code (terraform).
            WorkspaceUrl = $"https://{integrationTestConfiguration.Configuration.GetValue("dbw-playground-workspace-url")}",
            WorkspaceAccessToken = integrationTestConfiguration.Configuration.GetValue("dbw-playground-workspace-token"),
            WarehouseId = integrationTestConfiguration.Configuration.GetValue("dbw-sql-endpoint-id"),
        };

        DatabricksWarehouseManager = new DatabricksWarehouseManager(databricksWarehouseSettings);
        DatabricksOptionsMock = CreateDatabricksOptionsMock(DatabricksWarehouseManager);
    }

    public DatabricksWarehouseManager DatabricksWarehouseManager { get; }

    public Mock<IOptions<DatabricksOptions>> DatabricksOptionsMock { get;}

    public Task InitializeAsync()
    {
        // TODO: Create schema if we want to reuse it between multiple tests
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        // TODO: Drop any schema we created in 'InitializeAsync'
        return Task.CompletedTask;
    }

    private static Mock<IOptions<DatabricksOptions>> CreateDatabricksOptionsMock(DatabricksWarehouseManager databricksWarehouseManager)
    {
        var databricksOptionsMock = new Mock<IOptions<DatabricksOptions>>();
        databricksOptionsMock
            .Setup(o => o.Value)
            .Returns(new DatabricksOptions
            {
                DATABRICKS_WORKSPACE_URL = databricksWarehouseManager.Settings.WorkspaceUrl,
                DATABRICKS_WORKSPACE_TOKEN = databricksWarehouseManager.Settings.WorkspaceAccessToken,
                DATABRICKS_WAREHOUSE_ID = databricksWarehouseManager.Settings.WarehouseId,
            });

        return databricksOptionsMock;
    }
}
