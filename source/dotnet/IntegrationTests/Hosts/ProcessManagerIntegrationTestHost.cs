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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Wholesale.IntegrationTests.Mock;
using Energinet.DataHub.Wholesale.ProcessManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Hosts;

public sealed class ProcessManagerIntegrationTestHost : IDisposable
{
    private readonly IHost _processManagerHost;

    private ProcessManagerIntegrationTestHost(IHost processManagerHost)
    {
        _processManagerHost = processManagerHost;
    }

    public static Task<ProcessManagerIntegrationTestHost> InitializeAsync(
        Action<IServiceCollection>? serviceConfiguration = default)
    {
        ConfigureEnvironmentVars();

        var host = Program.BuildAppHost(serviceCollection =>
        {
            ConfigureServices(serviceCollection);
            serviceConfiguration?.Invoke(serviceCollection);
        });

        return Task.FromResult(new ProcessManagerIntegrationTestHost(host));
    }

    public AsyncServiceScope BeginScope()
    {
        var host = _processManagerHost;
        if (host == null)
            throw new InvalidOperationException("Cannot create scope on uninitialized host.");

        return host.Services.CreateAsyncScope();
    }

    public void Dispose()
    {
        _processManagerHost.Dispose();
    }

    private static void ConfigureEnvironmentVars()
    {
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.AppInsightsInstrumentationKey, "fake_value");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ServiceBusSendConnectionString, "fake_value");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ServiceBusManageConnectionString, "fake_value");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ProcessCompletedTopicName, "fake_value");
    }

    private static void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.Replace(ServiceDescriptor.Singleton<ServiceBusClient, MockedServiceBusClient>());
        serviceCollection.Replace(ServiceDescriptor.Scoped<ICorrelationContext, MockedCorrelationContext>());
    }
}
