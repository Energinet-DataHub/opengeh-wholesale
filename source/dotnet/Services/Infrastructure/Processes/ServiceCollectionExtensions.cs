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
using Energinet.DataHub.Wholesale.Application.Infrastructure;
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Infrastructure.Integration;
using Energinet.DataHub.Wholesale.Infrastructure.Registration;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.Infrastructure.Processes;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProcessCompletedPublisher(
        this IServiceCollection serviceCollection,
        string serviceBusConnectionString,
        string processCompletedTopicName)
    {
        serviceCollection.AddScoped<IProcessCompletedPublisher>(provider =>
        {
            var sender = provider
                .GetRequiredService<TargetedSingleton<ServiceBusSender, ProcessCompletedPublisher>>()
                .Instance;
            var factory = provider.GetRequiredService<IServiceBusMessageFactory>();
            return new ProcessCompletedPublisher(sender, factory);
        });

        if (serviceCollection.All(x => x.ServiceType != typeof(ServiceBusClient)))
            serviceCollection.AddSingleton(_ => new ServiceBusClient(serviceBusConnectionString));

        serviceCollection.AddSingleton(provider =>
        {
            var client = provider.GetRequiredService<ServiceBusClient>();
            return new TargetedSingleton<ServiceBusSender, ProcessCompletedPublisher>(
                client.CreateSender(processCompletedTopicName));
        });

        return serviceCollection;
    }

    public static IServiceCollection AddProcessCompletedIntegrationEventPublisher(
        this IServiceCollection serviceCollection,
        string serviceBusConnectionString,
        string integrationEventsTopicName)
    {
        serviceCollection.AddScoped<IProcessCompletedIntegrationEventPublisher>(provider =>
        {
            var sender = provider
                .GetRequiredService<TargetedSingleton<ServiceBusSender, ProcessCompletedIntegrationEventPublisher>>()
                .Instance;
            var factory = provider.GetRequiredService<IServiceBusMessageFactory>();
            var mapper = provider.GetRequiredService<IProcessCompletedIntegrationEventMapper>();
            return new ProcessCompletedIntegrationEventPublisher(sender, factory, mapper);
        });

        if (serviceCollection.All(x => x.ServiceType != typeof(ServiceBusClient)))
            serviceCollection.AddSingleton(_ => new ServiceBusClient(serviceBusConnectionString));

        serviceCollection.AddSingleton(provider =>
        {
            var client = provider.GetRequiredService<ServiceBusClient>();
            return new TargetedSingleton<ServiceBusSender, ProcessCompletedIntegrationEventPublisher>(
                client.CreateSender(integrationEventsTopicName));
        });

        return serviceCollection;
    }
}
