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

using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.MarketParticipant.Infrastructure.Model.Contracts;
using Energinet.DataHub.Wholesale.Calculations.Application.IntegrationEvents;
using Energinet.DataHub.Wholesale.Calculations.Application.IntegrationEvents.Handlers;
using Energinet.DataHub.Wholesale.Calculations.Application.UseCases;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.GridArea;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.ReceivedIntegrationEvent;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.GridArea;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.Options;
using Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks;
using Energinet.DataHub.Wholesale.Events.Application.Workers;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.WebApi.Extensions.DependencyInjection;

/// <summary>
/// Registration of services required for the Events module.
/// </summary>
public static class EventsExtensions
{
    public static IServiceCollection AddIntegrationEventsSubscription(this IServiceCollection services)
    {
        // These are located within Calculations sub-area
        services
            .AddScoped<IGridAreaOwnerRepository, GridAreaOwnerRepository>()
            .AddScoped<IIntegrationEventHandler, GridAreaOwnershipAssignedEventHandler>();

        // These are located within Calculations sub-area
        services
            .AddScoped<IReceivedIntegrationEventRepository, ReceivedIntegrationEventRepository>()
            .AddScoped<IntegrationEventHandlerFactory>();

        // These are located within Calculations sub-area
        services
            .AddSubscriber<ReceivedIntegrationEventHandler>(
            [
                GridAreaOwnershipAssigned.Descriptor,
            ]);

        services
            .AddHostedService<ReceiveIntegrationEventServiceBusWorker>();

        services
            .AddOptions<IntegrationEventsOptions>()
            .BindConfiguration(IntegrationEventsOptions.SectionName)
            .ValidateDataAnnotations();

        // Health checks
        services.AddHealthChecks()
            .AddAzureServiceBusSubscription(
                sp => sp.GetRequiredService<IOptions<ServiceBusNamespaceOptions>>().Value.ConnectionString,
                sp => sp.GetRequiredService<IOptions<IntegrationEventsOptions>>().Value.TopicName,
                sp => sp.GetRequiredService<IOptions<IntegrationEventsOptions>>().Value.SubscriptionName,
                name: HealthCheckNames.IntegrationEventsTopicSubscription);

        return services;
    }

    public static IServiceCollection AddInboxHandling(this IServiceCollection services)
    {
        services
            .AddOptions<WholesaleInboxQueueOptions>()
            .BindConfiguration(WholesaleInboxQueueOptions.SectionName)
            .ValidateDataAnnotations();

        // Health checks
        services.AddHealthChecks()
            // Must use a listener connection string
            .AddAzureServiceBusQueue(
                sp => sp.GetRequiredService<IOptions<ServiceBusNamespaceOptions>>().Value.ConnectionString,
                sp => sp.GetRequiredService<IOptions<WholesaleInboxQueueOptions>>().Value.QueueName,
                name: "WholesaleInboxQueue");

        services
            .AddHostedService<WholesaleInboxServiceBusWorker>();

        return services;
    }
}
