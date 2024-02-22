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

using Energinet.DataHub.Core.App.WebApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.MarketParticipant.Infrastructure.Model.Contracts;
using Energinet.DataHub.Wholesale.Calculations.Application.IntegrationEvents;
using Energinet.DataHub.Wholesale.Calculations.Application.IntegrationEvents.Handlers;
using Energinet.DataHub.Wholesale.Calculations.Application.UseCases;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.GridArea;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.ReceivedIntegrationEvent;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.GridArea;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Energinet.DataHub.Wholesale.Events.Application.Triggers;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Energinet.DataHub.Wholesale.Events.Application.Workers;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Extensions.DependencyInjection;
using Google.Protobuf.Reflection;

namespace Energinet.DataHub.Wholesale.WebApi.Configuration.Modules;

/// <summary>
/// Registration of services required for the Events module.
/// </summary>
public static class EventsRegistration
{
    public static IServiceCollection AddEventsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEventsDatabase(configuration);
        services.AddIntegrationEventsSubscription();
        services.AddInboxHandling();
        services.AddCompletedCalculationsHandling();

        return services;
    }

    private static IServiceCollection AddIntegrationEventsSubscription(this IServiceCollection services)
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
            new List<MessageDescriptor>
            {
                GridAreaOwnershipAssigned.Descriptor,
            });

        services
            .AddHostedService<ReceiveIntegrationEventServiceBusWorker>();

        return services;
    }

    private static IServiceCollection AddInboxHandling(this IServiceCollection services)
    {
        services
            .AddHostedService<AggregatedTimeSeriesServiceBusWorker>();

        return services;
    }

    private static IServiceCollection AddCompletedCalculationsHandling(this IServiceCollection services)
    {
        services
            .AddScoped<ICompletedCalculationFactory, CompletedCalculationFactory>()
            .AddScoped<IRegisterCompletedCalculationsHandler, RegisterCompletedCalculationsHandler>(); // This depends on services within Calculations sub-area

        services
            .AddHostedService<RegisterCompletedCalculationsTrigger>()
            .AddHealthChecks()
                .AddRepeatingTriggerHealthCheck<RegisterCompletedCalculationsTrigger>(TimeSpan.FromMinutes(1));

        return services;
    }
}
