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
using Energinet.DataHub.Wholesale.Common.DatabricksClient;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SqlStatements;

public class DatabricksSqlStatementApiFixture : IAsyncLifetime
{
    private const string StatementsEndpointPath = "/api/2.0/sql/statements";

    public DatabricksSqlStatementApiFixture()
    {
        IntegrationTestConfiguration = new IntegrationTestConfiguration();

        DatabricksOptions = new DatabricksOptions
        {
            DATABRICKS_WORKSPACE_URL = $"https://{IntegrationTestConfiguration.Configuration.GetValue("dbw-playground-workspace-url")}", // TODO: set https directly in KV
            DATABRICKS_WORKSPACE_TOKEN = IntegrationTestConfiguration.Configuration.GetValue("dbw-playground-workspace-token"),
            DATABRICKS_WAREHOUSE_ID = IntegrationTestConfiguration.Configuration.GetValue("dbw-sql-endpoint-id"),
        };

        DatabricksSqlSchemaManager = new DatabricksSqlSchemaManager(DatabricksOptions);
    }

    public DatabricksOptions DatabricksOptions { get; }

    public DatabricksSqlSchemaManager DatabricksSqlSchemaManager { get; }

    private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        //// foreach (var schema in _createdSchemas)
        //// {
        ////     await DropSchemaAsync(schema, true).ConfigureAwait(false);
        //// }

        return Task.CompletedTask;
    }
}
