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
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.Wholesale.IntegrationEventListener;
using Energinet.DataHub.Wholesale.IntegrationEventListener.Common;
using Energinet.DataHub.Wholesale.IntegrationTests.Mock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Hosts;

public sealed class IntegrationEventListenerIntegrationTestHost : IDisposable
{
    private readonly IHost _processManagerHost;

    private IntegrationEventListenerIntegrationTestHost(IHost processManagerHost)
    {
        _processManagerHost = processManagerHost;
    }

    public static Task<IntegrationEventListenerIntegrationTestHost> CreateAsync(
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

        return Task.FromResult(new IntegrationEventListenerIntegrationTestHost(hostBuilder.Build()));
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
        const string anyValue = "fake_value";
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.AppInsightsInstrumentationKey, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.IntegrationEventConnectionListenerString, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.IntegrationEventConnectionManagerString, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.IntegrationEventsTopicName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.MeteringPointCreatedSubscriptionName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.MeteringPointConnectedSubscriptionName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.MarketParticipantChangedSubscriptionName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.IntegrationEventsEventHubName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.IntegrationEventsEventHubConnectionString, anyValue);
    }

    private static void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.Replace(ServiceDescriptor.Singleton<ServiceBusClient, MockedServiceBusClient>());
    }
}
