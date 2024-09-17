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

using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Builder;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Energinet.DataHub.MarketParticipant.Infrastructure.Model.Contracts;
using Energinet.DataHub.Wholesale.Calculations.Application.IntegrationEvents;
using Energinet.DataHub.Wholesale.Calculations.Application.IntegrationEvents.Handlers;
using Energinet.DataHub.Wholesale.Calculations.Application.UseCases;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.GridArea;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.ReceivedIntegrationEvent;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.GridArea;
using Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks;
using Energinet.DataHub.Wholesale.Events.Application.Workers;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.WebApi.Extensions.DependencyInjection;

/// <summary>
/// Registration of services required for the Events module.
/// </summary>
public static class EventsExtensions
{
    public static IServiceCollection AddIntegrationEventsSubscription(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

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
            .AddOptions<IntegrationEventsOptions>()
            .BindConfiguration(IntegrationEventsOptions.SectionName)
            .ValidateDataAnnotations();

        services.AddAzureClients(builder =>
        {
            var integrationEventsOptions =
                configuration
                    .GetRequiredSection(IntegrationEventsOptions.SectionName)
                    .Get<IntegrationEventsOptions>()
                ?? throw new InvalidOperationException("Missing Integration Events configuration.");

            builder
                .AddClient<ServiceBusProcessor, ServiceBusClientOptions>((_, _, provider) =>
                    provider
                        .GetRequiredService<ServiceBusClient>()
                        .CreateProcessor(integrationEventsOptions.TopicName, integrationEventsOptions.SubscriptionName))
                .WithName(integrationEventsOptions.SubscriptionName);
        });

        services
            .AddHostedService<ReceiveIntegrationEventServiceBusWorker>();

        // Health checks
        var defaultAzureCredential = new DefaultAzureCredential();

        services
            .AddHealthChecks()
            .AddAzureServiceBusSubscription(
                sp => sp.GetRequiredService<IOptions<ServiceBusNamespaceOptions>>().Value.FullyQualifiedNamespace,
                sp => sp.GetRequiredService<IOptions<IntegrationEventsOptions>>().Value.TopicName,
                sp => sp.GetRequiredService<IOptions<IntegrationEventsOptions>>().Value.SubscriptionName,
                _ => defaultAzureCredential,
                name: HealthCheckNames.IntegrationEventsTopicSubscription)
            .AddServiceBusTopicSubscriptionDeadLetter(
                sp => sp.GetRequiredService<IOptions<ServiceBusNamespaceOptions>>().Value.FullyQualifiedNamespace,
                sp => sp.GetRequiredService<IOptions<IntegrationEventsOptions>>().Value.TopicName,
                sp => sp.GetRequiredService<IOptions<IntegrationEventsOptions>>().Value.SubscriptionName,
                _ => defaultAzureCredential,
                "Dead-letter (integration events)",
                [HealthChecksConstants.StatusHealthCheckTag]);

        return services;
    }
}
