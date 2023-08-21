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

using Energinet.DataHub.Core.App.WebApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using Energinet.DataHub.Wholesale.Events.Application.Triggers;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence.CompletedBatches;

namespace Energinet.DataHub.Wholesale.WebApi.Configuration.Modules;

/// <summary>
/// Registration of services required for the Batches module.
/// </summary>
public static class EventsRegistration
{
    public static void AddEventsModule(
        this IServiceCollection serviceCollection,
        Func<IServiceProvider, CommunicationSettings> communicationSettingsFactory)
    {
        serviceCollection.AddHostedService<RegisterCompletedBatchesTrigger>();

        serviceCollection.AddScoped<ICompletedBatchRepository, CompletedBatchRepository>();
        serviceCollection.AddScoped<ICompletedBatchFactory, CompletedBatchFactory>();
        serviceCollection.AddScoped<IRegisterCompletedBatchesHandler, RegisterCompletedBatchesHandler>();

        serviceCollection.AddScoped<ICalculationResultIntegrationEventFactory, CalculationResultIntegrationEventFactory>();

        serviceCollection.AddApplications();
        serviceCollection.AddInfrastructure();

        serviceCollection.AddCommunication<IntegrationEventProvider>(communicationSettingsFactory);

        serviceCollection
            .AddHealthChecks()
            .AddRepeatingTriggerHealthCheck<RegisterCompletedBatchesTrigger>(TimeSpan.FromMinutes(1));
    }

    private static void AddApplications(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services
            .AddScoped<ICalculationResultCompletedFactory,
                CalculationResultCompletedFactory>();
    }

    private static void AddInfrastructure(
        this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IEventsDatabaseContext, EventsDatabaseContext>();
        serviceCollection.AddSingleton<IJsonSerializer, JsonSerializer>();
    }
}
