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
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Subscriber;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using Energinet.DataHub.Wholesale.Events.Application.GridArea;
using Energinet.DataHub.Wholesale.Events.Application.IntegrationEvents;
using Energinet.DataHub.Wholesale.Events.Application.IntegrationEvents.Handlers;
using Energinet.DataHub.Wholesale.Events.Application.Options;
using Energinet.DataHub.Wholesale.Events.Application.Triggers;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Energinet.DataHub.Wholesale.Events.Application.Workers;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence.CompletedBatches;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence.GridArea;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence.ReceivedIntegrationEvent;
using Google.Protobuf.Reflection;

namespace Energinet.DataHub.Wholesale.WebApi.Configuration.Modules;

/// <summary>
/// Registration of services required for the Batches module.
/// </summary>
public static class EventsRegistration
{
    public static void AddEventsModule(
        this IServiceCollection serviceCollection)
    {
        RegisterCompletedBatch(serviceCollection);

        serviceCollection.AddScoped<IGridAreaRepository, GridAreaRepository>();
        serviceCollection.AddScoped<IReceivedIntegrationEventRepository, ReceivedIntegrationEventRepository>();

        serviceCollection.AddScoped<ICalculationResultIntegrationEventFactory, CalculationResultIntegrationEventFactory>();

        serviceCollection.AddInfrastructure();

        serviceCollection.AddPublisher<IntegrationEventProvider>();

        RegisterHostedServices(serviceCollection);

        RegisterIntegrationEvents(serviceCollection);
    }

    private static void RegisterIntegrationEvents(IServiceCollection serviceCollection)
    {
        var integrationEventDescriptors = new List<MessageDescriptor>
        {
            GridAreaOwnershipAssigned.Descriptor,
        };
        serviceCollection.AddSubscriber<ReceivedIntegrationEventHandler>(integrationEventDescriptors);

        serviceCollection.AddScoped<IntegrationEventHandlerFactory>();
    }

    private static void RegisterCompletedBatch(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ICompletedBatchRepository, CompletedBatchRepository>();
        serviceCollection.AddScoped<ICompletedBatchFactory, CompletedBatchFactory>();
        serviceCollection.AddScoped<IRegisterCompletedBatchesHandler, RegisterCompletedBatchesHandler>();
    }

    private static void AddInfrastructure(
        this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
        serviceCollection
            .AddScoped<ICalculationResultCompletedFactory,
                CalculationResultCompletedFactory>();
        serviceCollection.AddScoped<IEnergyResultProducedV1Factory,
            EnergyResultProducedV1Factory>();
        serviceCollection.AddScoped<IAmountPerChargeResultProducedV1Factory,
            AmountPerChargeResultProducedV1Factory>();
        serviceCollection.AddScoped<IMonthlyAmountPerChargeResultProducedV1Factory,
            MonthlyAmountPerChargeResultProducedV1Factory>();
        serviceCollection.AddScoped<IEventsDatabaseContext, EventsDatabaseContext>();
        serviceCollection.AddSingleton<IJsonSerializer, JsonSerializer>();
    }

    private static void RegisterHostedServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddHostedService<AggregatedTimeSeriesServiceBusWorker>();
        serviceCollection.AddHostedService<ReceiveIntegrationEventServiceBusWorker>();
        serviceCollection.AddHostedService<RegisterCompletedBatchesTrigger>();
        serviceCollection
            .AddHealthChecks()
            .AddRepeatingTriggerHealthCheck<RegisterCompletedBatchesTrigger>(TimeSpan.FromMinutes(1));
    }
}
