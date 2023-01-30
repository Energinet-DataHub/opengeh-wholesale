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
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Registration;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.Infrastructure.Batches;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBatchCompletedPublisher(
        this IServiceCollection serviceCollection,
        string serviceBusConnectionString,
        string batchCompletedTopicName,
        string messageType)
    {
        serviceCollection.AddScoped<IBatchCompletedPublisher>(provider =>
        {
            var sender = provider
                .GetRequiredService<TargetedSingleton<ServiceBusSender, BatchCompletedPublisher>>()
                .Instance;
            var factory = provider.GetRequiredService<IServiceBusMessageFactory>();
            var serializer = provider.GetRequiredService<IJsonSerializer>();
            return new BatchCompletedPublisher(sender, factory, messageType, serializer);
        });

        if (serviceCollection.All(x => x.ServiceType != typeof(ServiceBusClient)))
           serviceCollection.AddSingleton(_ => new ServiceBusClient(serviceBusConnectionString));

        serviceCollection.AddSingleton(provider =>
        {
            var client = provider.GetRequiredService<ServiceBusClient>();
            return new TargetedSingleton<ServiceBusSender, BatchCompletedPublisher>(
                client.CreateSender(batchCompletedTopicName));
        });

        return serviceCollection;
    }
}
