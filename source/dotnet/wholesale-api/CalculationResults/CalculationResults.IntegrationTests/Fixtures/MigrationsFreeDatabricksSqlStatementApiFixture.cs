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

using System.Diagnostics;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;

public class MigrationsFreeDatabricksSqlStatementApiFixture : IAsyncLifetime
{
    public MigrationsFreeDatabricksSqlStatementApiFixture()
    {
        var integrationTestConfiguration = new IntegrationTestConfiguration();
        DatabricksSchemaManager = new MigrationsFreeDatabricksSchemaManager(integrationTestConfiguration.DatabricksSettings, "wholesale");
    }

    public MigrationsFreeDatabricksSchemaManager DatabricksSchemaManager { get; }

    public bool DataIsInitialized { get; set; } = false;

    public async Task InitializeAsync()
    {
        await DatabricksSchemaManager.CreateSchemaAsync();
    }

    public async Task DisposeAsync()
    {
        await DatabricksSchemaManager.DropSchemaAsync();
    }

    public DatabricksSqlWarehouseQueryExecutor GetDatabricksExecutor()
    {
        var builder = new ConfigurationBuilder();
        builder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["WorkspaceUrl"] = DatabricksSchemaManager.Settings.WorkspaceUrl,
            ["WarehouseId"] = DatabricksSchemaManager.Settings.WarehouseId,
            ["WorkspaceToken"] = DatabricksSchemaManager.Settings.WorkspaceAccessToken,
        });
        var services = new ServiceCollection();
        var configuration = builder.Build();
        services.AddDatabricksSqlStatementExecution(configuration);
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetService<DatabricksSqlWarehouseQueryExecutor>()!;
    }
}
