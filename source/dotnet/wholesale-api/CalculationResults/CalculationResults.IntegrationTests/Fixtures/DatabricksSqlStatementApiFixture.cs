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

        DatabricksSchemaManager = new DatabricksSchemaManager(databricksWarehouseSettings, "wholesale");
        DatabricksOptionsMock = CreateDatabricksOptionsMock(DatabricksSchemaManager);
    }

    public DatabricksSchemaManager DatabricksSchemaManager { get; }

    public Mock<IOptions<DatabricksOptions>> DatabricksOptionsMock { get; }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
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
