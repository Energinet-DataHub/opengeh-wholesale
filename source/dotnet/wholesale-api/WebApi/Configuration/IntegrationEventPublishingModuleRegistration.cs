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

using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Application.IntegrationEventsManagement;
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Application.Workers;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Core;
using Energinet.DataHub.Wholesale.Infrastructure.EventPublishers;
using Energinet.DataHub.Wholesale.Infrastructure.IntegrationEventDispatching;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;

namespace Energinet.DataHub.Wholesale.WebApi.Configuration;

/// <summary>
/// Registration of services required for the IntegrationEventPublishing module.
/// </summary>
public static class Registration
{
    public static void AddIntegrationEventPublishingModule(
        this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IIntegrationEventService, IntegrationEventService>();
        serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
        serviceCollection.AddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();
        serviceCollection.AddScoped<IIntegrationEventCleanUpService, IntegrationEventCleanUpService>();

        serviceCollection.AddScoped<IIntegrationEventTopicServiceBusSender, IntegrationEventTopicServiceBusSender>();
        serviceCollection.AddScoped<IIntegrationEventPublishingDatabaseContext, IntegrationEventPublishingDatabaseContext>();
        serviceCollection.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        serviceCollection.AddScoped<IServiceBusMessageFactory, ServiceBusMessageFactory>();

        serviceCollection
            .AddOptions<IntegrationEventRetentionOptions>()
            .Configure(options => options.RetentionDays = 14);
        serviceCollection.AddHostedService<IntegrationEventsRetentionWorker>();

        RegisterEventPublishers(serviceCollection);
    }

    private static void RegisterEventPublishers(IServiceCollection serviceCollection)
    {
        // var serviceBusConnectionString =
        //     EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ServiceBusManageConnectionString);
        // var messageTypes = new Dictionary<Type, string>
        // {
        //     {
        //         typeof(BatchCompletedEventDto),
        //         EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.BatchCompletedEventName)
        //     },
        //     {
        //         typeof(ProcessCompletedEventDto),
        //         EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ProcessCompletedEventName)
        //     },
        // };
        // var domainEventTopicName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DomainEventsTopicName);
        // serviceCollection.AddDomainEventPublisher(serviceBusConnectionString, domainEventTopicName, new MessageTypeDictionary(messageTypes));
        //
        // var integrationEventTopicName =
        //     EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.IntegrationEventsTopicName);
        // serviceCollection.AddIntegrationEventPublisher(serviceBusConnectionString, integrationEventTopicName);
    }
}
