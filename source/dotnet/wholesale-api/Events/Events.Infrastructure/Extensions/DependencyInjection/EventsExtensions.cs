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

using Energinet.DataHub.Core.App.Common.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.Options;
using Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.AmountPerChargeResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.CalculationCompletedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EnergyResultProducedV2.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EventProviders;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.GridLossResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.TotalMonthlyAmountResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence.CompletedCalculations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.Extensions.DependencyInjection;

/// <summary>
/// Registration of services required for the Events module.
/// </summary>
public static class EventsExtensions
{
    /// <summary>
    /// Registration if Events database (schema) with key services to support read/write.
    /// </summary>
    public static IServiceCollection AddEventsDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddScoped<IEventsDatabaseContext, EventsDatabaseContext>();
        services.AddDbContext<EventsDatabaseContext>(
            options => options.UseSqlServer(
                configuration
                    .GetSection(ConnectionStringsOptions.ConnectionStrings)
                    .Get<ConnectionStringsOptions>()!.DB_CONNECTION_STRING,
                o =>
                {
                    o.UseNodaTime();
                    o.EnableRetryOnFailure();
                }));
        // Database Health check
        services.TryAddHealthChecks(
            registrationKey: HealthCheckNames.WholesaleDatabase,
            (key, builder) =>
        {
            builder.AddDbContextCheck<EventsDatabaseContext>(name: key);
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICompletedCalculationRepository, CompletedCalculationRepository>();

        return services;
    }

    public static void AddWholesaleInboxHandler(this IServiceCollection services)
    {
        // Wholesale inbox events handler
        services.AddScoped<WholesaleInboxHandler>();
    }

    public static IServiceCollection AddIntegrationEventsPublishing(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddScoped<IEnergyResultProducedV2Factory, EnergyResultProducedV2Factory>()
            .AddScoped<IGridLossResultProducedV1Factory, GridLossResultProducedV1Factory>()
            .AddScoped<IAmountPerChargeResultProducedV1Factory, AmountPerChargeResultProducedV1Factory>()
            .AddScoped<IMonthlyAmountPerChargeResultProducedV1Factory, MonthlyAmountPerChargeResultProducedV1Factory>()
            .AddScoped<ITotalMonthlyAmountResultProducedV1Factory, TotalMonthlyAmountResultProducedV1Factory>()
            .AddScoped<ICalculationCompletedFactory, CalculationCompletedV1Factory>();

        services
            .AddScoped<IEnergyResultEventProvider, EnergyResultEventProvider>()
            .AddScoped<IWholesaleResultEventProvider, WholesaleResultEventProvider>()
            .AddScoped<ICalculationCompletedEventProvider, CalculationCompletedEventProvider>();

        var serviceBusNamespaceOptions = configuration
            .GetRequiredSection(ServiceBusNamespaceOptions.SectionName)
            .Get<ServiceBusNamespaceOptions>();
        var integrationEventsOptions = configuration
            .GetRequiredSection(IntegrationEventsOptions.SectionName)
            .Get<IntegrationEventsOptions>();

        services.Configure<PublisherOptions>(options =>
        {
            options.ServiceBusConnectionString = serviceBusNamespaceOptions!.ConnectionString;
            options.TopicName = integrationEventsOptions!.TopicName;
        });
        services.AddPublisher<IntegrationEventProvider>();

        // Health checks
        services.AddHealthChecks()
            // Must use a listener connection string
            .AddAzureServiceBusSubscription(
                serviceBusNamespaceOptions!.ConnectionString,
                integrationEventsOptions!.TopicName,
                integrationEventsOptions.SubscriptionName,
                name: HealthCheckNames.IntegrationEventsTopicSubscription);

        return services;
    }

    public static IServiceCollection AddCompletedCalculationsHandling(this IServiceCollection services)
    {
        services
            .AddScoped<ICompletedCalculationFactory, CompletedCalculationFactory>()
            .AddScoped<IRegisterCompletedCalculationsHandler, RegisterCompletedCalculationsHandler>(); // This depends on services within Calculations sub-area

        return services;
    }
}
