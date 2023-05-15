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

using Energinet.DataHub.Wholesale.Application.IntegrationEventsManagement;
using Energinet.DataHub.Wholesale.Application.Workers;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Infrastructure.EventPublishers;
using Energinet.DataHub.Wholesale.Infrastructure.IntegrationEventDispatching;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.WebApi.Configuration;

/// <summary>
/// Registration of services required for the Batches module.
/// </summary>
public static class IntegrationEventPublishingRegistration
{
    public static void AddIntegrationEventPublishingModule(
        this IServiceCollection serviceCollection,
        string serviceBusConnectionString,
        string integrationEventTopicName)
    {
        serviceCollection.AddHostedService<DispatchIntegrationEventsWorker>();
        serviceCollection.AddHostedService<IntegrationEventsRetentionWorker>();

        serviceCollection.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        serviceCollection.AddScoped<IIntegrationEventCleanUpService, IntegrationEventCleanUpService>();
        serviceCollection.AddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();
        serviceCollection.AddScoped<IIntegrationEventTypeMapper>(_ => new IntegrationEventTypeMapper(new Dictionary<Type, string>
        {
            { typeof(CalculationResultCompleted), CalculationResultCompleted.BalanceFixingEventName },
        }));
        serviceCollection.AddScoped<IIntegrationEventService, IntegrationEventService>();
        serviceCollection.AddScoped<IIntegrationEventPublisher, IntegrationEventPublisher>();
        serviceCollection.AddIntegrationEventPublisher(serviceBusConnectionString, integrationEventTopicName);
    }
}
