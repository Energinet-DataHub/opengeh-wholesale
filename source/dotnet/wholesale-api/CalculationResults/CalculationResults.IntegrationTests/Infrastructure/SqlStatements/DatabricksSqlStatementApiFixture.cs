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

using System.Net.Http.Headers;
using System.Net.Http.Json;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.Common.DatabricksClient;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SqlStatements;

public class DatabricksSqlStatementApiFixture : IAsyncLifetime
{
    private const string StatementsEndpointPath = "/api/2.0/sql/statements";
    public DatabricksOptions DatabricksOptions { get; }

    public DatabricksSqlStatementApiFixture()
    {
        IntegrationTestConfiguration = new IntegrationTestConfiguration();
        DatabricksOptions = new DatabricksOptions
        {
            // TODO: use key vault
            DATABRICKS_WAREHOUSE_ID = "anyDatabricksId",
            DATABRICKS_WORKSPACE_URL = "https://anyDatabricksUrl",
            DATABRICKS_WORKSPACE_TOKEN = "myToken",
        };
    }


    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        // foreach (var schema in _createdSchemas)
        // {
        //     await DropSchemaAsync(schema, true).ConfigureAwait(false);
        // }
    }

    private IntegrationTestConfiguration IntegrationTestConfiguration { get; }
}
