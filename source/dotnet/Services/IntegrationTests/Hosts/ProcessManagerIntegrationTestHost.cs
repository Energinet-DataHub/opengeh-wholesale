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
    public const string CalculationStorageConnectionString = "UseDevelopmentStorage=true";
    public static readonly string _calculationStorageContainerName = $"calculation-{Guid.NewGuid()}";

    private readonly IHost _processManagerHost;

    private ProcessManagerIntegrationTestHost(IHost processManagerHost)
    {
        _processManagerHost = processManagerHost;
    }

    public static Task<ProcessManagerIntegrationTestHost> CreateAsync(
        Action<IServiceCollection>? serviceConfiguration = default)
    {
        ConfigureEnvironmentVars();

        var hostBuilder = Program
            .CreateHostBuilder()
            .ConfigureServices(ConfigureServices);

        if (serviceConfiguration != null)
        {
            hostBuilder = hostBuilder.ConfigureServices(serviceConfiguration);
        }

        return Task.FromResult(new ProcessManagerIntegrationTestHost(hostBuilder.Build()));
    }

    public AsyncServiceScope BeginScope()
    {
        return _processManagerHost.Services.CreateAsyncScope();
    }

    public void Dispose()
    {
        _processManagerHost.Dispose();
    }

    private static void ConfigureEnvironmentVars()
    {
        var anyValue = "fake_value";
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.AppInsightsInstrumentationKey, anyValue);

        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ServiceBusSendConnectionString, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ServiceBusManageConnectionString, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DomainEventsTopicName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.PublishProcessesCompletedWhenCompletedBatchSubscriptionName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ZipBasisDataWhenCompletedBatchSubscriptionName, anyValue);

        Environment.SetEnvironmentVariable(EnvironmentSettingNames.CalculationStorageConnectionString, CalculationStorageConnectionString);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.CalculationStorageContainerName, _calculationStorageContainerName);

        Environment.SetEnvironmentVariable(EnvironmentSettingNames.BatchCompletedEventName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ProcessCompletedEventName, anyValue);
    }

    private static void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.Replace(ServiceDescriptor.Singleton<ServiceBusClient, MockedServiceBusClient>());
        serviceCollection.Replace(ServiceDescriptor.Scoped<ICorrelationContext, MockedCorrelationContext>());
    }
}
