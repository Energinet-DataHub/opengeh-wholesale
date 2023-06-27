﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Wholesale.Events.Infrastructure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.EventPublishers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIntegrationEventPublisher(
        this IServiceCollection serviceCollection,
        string serviceBusConnectionString,
        string topicName)
    {
        ServiceBusClientAndAndMessageFactoryRegistry(serviceCollection, serviceBusConnectionString);

        serviceCollection.AddSingleton<IIntegrationEventTopicServiceBusSender>(provider =>
        {
            var client = provider.GetRequiredService<ServiceBusClient>();
            var sender = client.CreateSender(topicName);
            return new IntegrationEventTopicServiceBusSender(sender);
        });

        return serviceCollection;
    }

    private static void ServiceBusClientAndAndMessageFactoryRegistry(
        IServiceCollection serviceCollection,
        string serviceBusConnectionString)
    {
        if (serviceCollection.All(x => x.ServiceType != typeof(ServiceBusClient)))
            serviceCollection.AddSingleton(_ => new ServiceBusClient(serviceBusConnectionString));
    }
}
