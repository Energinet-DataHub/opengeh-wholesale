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
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Builder;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks;
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.CalculationCompletedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EventProviders;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.GridLossResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Interfaces;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.Extensions.DependencyInjection;

/// <summary>
/// Registration of services required for the Events module.
/// </summary>
public static class EventsExtensions
{
    /// <summary>
    /// Register handler that handles inbox requests using handlers implementing <see cref="IWholesaleInboxRequestHandler"/>.
    /// </summary>
    public static IServiceCollection AddWholesaleInboxHandler(this IServiceCollection services)
    {
        services.AddScoped<WholesaleInboxHandler>();

        return services;
    }

    public static IServiceCollection AddIntegrationEventsPublishing(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddScoped<IGridLossResultProducedV1Factory, GridLossResultProducedV1Factory>()
            .AddScoped<ICalculationCompletedFactory, CalculationCompletedV1Factory>();

        services
            .AddScoped<IEnergyResultEventProvider, EnergyResultEventProvider>()
            .AddScoped<ICalculationCompletedEventProvider, CalculationCompletedEventProvider>()
            .AddScoped<IServiceBusMessageFactory, ServiceBusMessageFactory>()
            .AddScoped<ICalculationIntegrationEventPublisher, CalculationIntegrationEventPublisher>();

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
                .AddClient<ServiceBusSender, ServiceBusClientOptions>((_, _, provider) =>
                    provider
                        .GetRequiredService<ServiceBusClient>()
                        .CreateSender(integrationEventsOptions.TopicName))
                .WithName(integrationEventsOptions.TopicName);
        });

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
