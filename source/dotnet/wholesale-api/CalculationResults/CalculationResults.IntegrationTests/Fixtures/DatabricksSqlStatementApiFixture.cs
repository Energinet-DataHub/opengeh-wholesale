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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    }

    public DatabricksSchemaManager DatabricksSchemaManager { get; }

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
        var serviceCollection = new ServiceCollection();
        var configuration = builder.Build();
        serviceCollection.AddDatabricksSqlStatementExecution(configuration);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        return serviceProvider.GetService<DatabricksSqlWarehouseQueryExecutor>()!;
    }
}
