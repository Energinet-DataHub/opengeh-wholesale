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
using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Energinet.DataHub.MarketParticipant.Infrastructure.Model.Contracts;
using Energinet.DataHub.Wholesale.Calculations.Application.IntegrationEvents;
using Energinet.DataHub.Wholesale.Calculations.Application.IntegrationEvents.Handlers;
using Energinet.DataHub.Wholesale.Calculations.Application.UseCases;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.GridArea;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.ReceivedIntegrationEvent;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.GridArea;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Energinet.DataHub.Wholesale.Events.Application.Triggers;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Energinet.DataHub.Wholesale.Events.Application.Workers;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.AmountPerChargeResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EnergyResultProducedV2.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EventProviders;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.GridLossResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence.CompletedCalculations;
using Google.Protobuf.Reflection;

namespace Energinet.DataHub.Wholesale.WebApi.Configuration.Modules;

/// <summary>
/// Registration of services required for the Calculations module.
/// </summary>
public static class EventsRegistration
{
    public static void AddEventsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure();

        services.AddIntegrationEventPublisher(configuration);

        RegisterIntegrationEvents(services);

        RegisterHostedServices(services);
    }

    private static void RegisterIntegrationEvents(IServiceCollection services)
    {
        var integrationEventDescriptors = new List<MessageDescriptor>
        {
            GridAreaOwnershipAssigned.Descriptor,
        };
        services.AddSubscriber<ReceivedIntegrationEventHandler>(integrationEventDescriptors);
        services.AddScoped<IIntegrationEventHandler, GridAreaOwnershipAssignedEventHandler>();

        services.AddScoped<IntegrationEventHandlerFactory>();
    }

    private static void AddApplication(this IServiceCollection services)
    {
        services
            .AddScoped<IUnitOfWork, UnitOfWork>();

        services
            .AddScoped<ICompletedCalculationRepository, CompletedCalculationRepository>()
            .AddScoped<ICompletedCalculationFactory, CompletedCalculationFactory>()
            .AddScoped<IRegisterCompletedCalculationsHandler, RegisterCompletedCalculationsHandler>();

        services
            .AddScoped<IEnergyResultEventProvider, EnergyResultEventProvider>()
            .AddScoped<IWholesaleResultEventProvider, WholesaleResultEventProvider>();

        services
            .AddScoped<IGridAreaOwnerRepository, GridAreaOwnerRepository>()
            .AddScoped<IReceivedIntegrationEventRepository, ReceivedIntegrationEventRepository>();
    }

    private static void AddInfrastructure(this IServiceCollection services)
    {
        services
            .AddScoped<IEnergyResultProducedV2Factory, EnergyResultProducedV2Factory>()
            .AddScoped<IGridLossResultProducedV1Factory, GridLossResultProducedV1Factory>()
            .AddScoped<IAmountPerChargeResultProducedV1Factory, AmountPerChargeResultProducedV1Factory>()
            .AddScoped<IMonthlyAmountPerChargeResultProducedV1Factory, MonthlyAmountPerChargeResultProducedV1Factory>()
            .AddScoped<IEventsDatabaseContext, EventsDatabaseContext>();
    }

    private static void AddIntegrationEventPublisher(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceBusOptions = configuration.Get<ServiceBusOptions>()!;

        // Register integration event publisher
        services.Configure<PublisherOptions>(options =>
        {
            options.ServiceBusConnectionString = serviceBusOptions.SERVICE_BUS_SEND_CONNECTION_STRING;
            options.TopicName = serviceBusOptions.INTEGRATIONEVENTS_TOPIC_NAME;
            options.TransportType = Azure.Messaging.ServiceBus.ServiceBusTransportType.AmqpWebSockets;
        });
        services.AddPublisher<IntegrationEventProvider>();

        // Register hosted service for publishing integration events
        services.Configure<PublisherWorkerOptions>(options => options.HostedServiceExecutionDelayMs = 10000);
        services.AddPublisherWorker();
    }

    private static void RegisterHostedServices(IServiceCollection services)
    {
        services
            .AddHostedService<AggregatedTimeSeriesServiceBusWorker>()
            .AddHostedService<RegisterCompletedCalculationsTrigger>()
            .AddHostedService<ReceiveIntegrationEventServiceBusWorker>();

        services
            .AddHealthChecks()
            .AddRepeatingTriggerHealthCheck<RegisterCompletedCalculationsTrigger>(TimeSpan.FromMinutes(1));
    }
}
