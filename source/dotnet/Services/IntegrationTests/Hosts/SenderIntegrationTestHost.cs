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
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.Wholesale.IntegrationTests.Mock;
using Energinet.DataHub.Wholesale.Sender;
using Energinet.DataHub.Wholesale.Sender.Endpoints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Hosts;

public sealed class SenderIntegrationTestHost : IDisposable
{
    private readonly IHost _processManagerHost;

    private SenderIntegrationTestHost(IHost processManagerHost)
    {
        _processManagerHost = processManagerHost;
    }

    public static Task<SenderIntegrationTestHost> CreateAsync(
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

        return Task.FromResult(new SenderIntegrationTestHost(hostBuilder.Build()));
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
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DatabaseConnectionString,  "UseDevelopmentStorage=true");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DataHubServiceBusManageConnectionString, "foo=bar");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ServiceBusManageConnectionString, "foo=bar");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ServiceBusListenConnectionString, "foo=bar");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DomainEventsTopicName, anyValue);
        Environment.SetEnvironmentVariable(
            EnvironmentSettingNames.SendDataAvailableWhenCompletedProcessSubscriptionName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.MessageHubServiceBusSendConnectionString, "foo=bar");
        Environment.SetEnvironmentVariable(
            EnvironmentSettingNames.MessageHubServiceBusListenConnectionString,
            anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.MessageHubDataAvailableQueueName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.MessageHubRequestQueueName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.MessageHubReplyQueueName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.MessageHubStorageConnectionString, "foo=bar");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.MessageHubStorageContainerName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.CalculationStorageConnectionString, "UseDevelopmentStorage=true");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.CalculationStorageContainerName, anyValue);
    }

    private static void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.Replace(ServiceDescriptor.Singleton<ServiceBusClient, MockedServiceBusClient>());
        serviceCollection.Replace(ServiceDescriptor.Singleton(provider =>
        {
            var mock = new Moq.Mock<IServiceBusClientFactory>();
            mock.Setup(x => x.Create()).Returns(provider.GetRequiredService<ServiceBusClient>());
            return mock.Object;
        }));
    }
}
