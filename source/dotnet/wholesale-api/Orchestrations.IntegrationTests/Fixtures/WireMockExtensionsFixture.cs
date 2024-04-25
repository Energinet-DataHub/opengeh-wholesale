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

using Energinet.DataHub.Core.Databricks.Jobs.Abstractions;
using Energinet.DataHub.Core.Databricks.Jobs.Configuration;
using Energinet.DataHub.Core.Databricks.Jobs.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WireMock.Server;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;

/// <summary>
/// Fixture to support the testing of <see cref="DatabricksApiWireMockExtensions"/>.
/// </summary>
public sealed class WireMockExtensionsFixture : IDisposable
{
    private readonly ServiceCollection _services;

    public WireMockExtensionsFixture()
    {
        MockServer = WireMockServer.Start();

        _services = ConfigureServices(workspaceUrl: MockServer.Url!);
        var serviceProvider = _services.BuildServiceProvider();
        JobApiClient = serviceProvider.GetRequiredService<IJobsApiClient>();
        DatabricksExecutor = serviceProvider.GetRequiredService<DatabricksSqlWarehouseQueryExecutor>();
    }

    public WireMockServer MockServer { get; }

    public IJobsApiClient JobApiClient { get; }

    public DatabricksSqlWarehouseQueryExecutor DatabricksExecutor { get; }

    public void Dispose()
    {
        MockServer.Dispose();
    }

    private static ServiceCollection ConfigureServices(string workspaceUrl)
    {
        var services = new ServiceCollection();

        var configuration = CreateInMemoryConfigurations(new Dictionary<string, string?>()
        {
            [$"{nameof(DatabricksJobsOptions.WorkspaceUrl)}"] = workspaceUrl,
            [$"{nameof(DatabricksJobsOptions.WorkspaceToken)}"] = "notEmpty",
            [$"{nameof(DatabricksJobsOptions.WarehouseId)}"] = "notEmpty",

            [$"{nameof(DatabricksSqlStatementOptions.WorkspaceUrl)}"] = workspaceUrl,
            [$"{nameof(DatabricksSqlStatementOptions.WorkspaceToken)}"] = "notEmpty",
            [$"{nameof(DatabricksSqlStatementOptions.WarehouseId)}"] = "notEmpty",
        });
        services
            .AddDatabricksJobs(configuration)
            .AddDatabricksSqlStatementExecution(configuration);

        return services;
    }

    private static IConfiguration CreateInMemoryConfigurations(Dictionary<string, string?> configurations)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(configurations)
            .Build();
    }
}
