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

using DurableTask.SqlServer.AzureFunctions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.DurableTask;

/// <summary>
/// A manager that can be used to manage orchestrations in Durable Functions,
/// typically from integration tests.
/// See https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-instance-management?tabs=csharp
///
/// IMPORTANT: This class is purely intended to be used from tests.
///
/// For production scenarious it is recommended to manage instances using
/// the Durable Functions orchestration client binding.
/// See https://github.com/Azure/azure-functions-durable-extension/issues/1600#issuecomment-742176091.
/// </summary>
public class DurableTaskManager : IDisposable
{
    // TODO:
    // We should move this class to TestCommon, maybe to a new project named DurableFunctionApp.TestCommon.
    // It should recide in the namespace "DurableTask" like it does here.
    // When it has been moved we should remove our current dependency to the NuGet package "Microsoft.Azure.WebJobs.Extensions.DurableTask".
    public DurableTaskManager(
        string storageProviderConnectionStringName,
        string storageProviderConnectionString,
        bool useSqlProvider = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageProviderConnectionStringName);
        ArgumentException.ThrowIfNullOrWhiteSpace(storageProviderConnectionString);

        ConnectionStringName = storageProviderConnectionStringName;
        ConnectionString = storageProviderConnectionString;
        var services = ConfigureServices(ConnectionStringName, ConnectionString, useSqlProvider);
        ServiceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// The storage provider connection string name in configuration.
    /// </summary>
    public string ConnectionStringName { get; }

    /// <summary>
    /// The storage provider connection string.
    /// This value should be configured as the value of the <see cref="ConnectionStringName"/> setting.
    /// </summary>
    public string ConnectionString { get; }

    private ServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Create a durable client that can be used to manage the Task Hub given by <paramref name="taskHubName"/>.
    /// </summary>
    public IDurableClient CreateClient(string taskHubName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskHubName);

        var clientFactory = ServiceProvider.GetRequiredService<IDurableClientFactory>();
        return clientFactory.CreateClient(new DurableClientOptions
        {
            ConnectionName = ConnectionStringName,
            TaskHub = taskHubName,
        });
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        ServiceProvider.Dispose();
    }

    /// <summary>
    /// Ensure we register services and configuration necessary for
    /// later requesting the creation of the type <see cref="IDurableClientFactory"/>.
    /// </summary>
    private static ServiceCollection ConfigureServices(string connectionStringName, string connectionString, bool useSqlProvider)
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                [connectionStringName] = connectionString,
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        if (useSqlProvider)
        {
            // If we use Durable Task SQL Provider then add this factory
            services.AddDurableTaskSqlProvider();
        }

        services.AddDurableClientFactory();

        return services;
    }
}
