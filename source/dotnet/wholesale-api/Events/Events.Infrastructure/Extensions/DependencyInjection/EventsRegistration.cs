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
using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.AmountPerChargeResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EnergyResultProducedV2.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EventProviders;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.GridLossResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence.CompletedCalculations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.Extensions.DependencyInjection
{
    /// <summary>
    /// Registration of services required for the Events module.
    /// </summary>
    public static class EventsRegistration
    {
        /// <summary>
        /// Registration if Events database (schema) with key services to support read/write.
        /// </summary>
        public static IServiceCollection AddEventsDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            // We don't add a health check for this context, because it's just another schema
            // in the database we already use for Calculations
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

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICompletedCalculationRepository, CompletedCalculationRepository>();

            return services;
        }

        public static IServiceCollection AddIntegrationEventPublishing(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddScoped<IEnergyResultProducedV2Factory, EnergyResultProducedV2Factory>()
                .AddScoped<IGridLossResultProducedV1Factory, GridLossResultProducedV1Factory>()
                .AddScoped<IAmountPerChargeResultProducedV1Factory, AmountPerChargeResultProducedV1Factory>()
                .AddScoped<IMonthlyAmountPerChargeResultProducedV1Factory, MonthlyAmountPerChargeResultProducedV1Factory>();

            services
                .AddScoped<IEnergyResultEventProvider, EnergyResultEventProvider>()
                .AddScoped<IWholesaleResultEventProvider, WholesaleResultEventProvider>();

            var serviceBusOptions = configuration.Get<ServiceBusOptions>()!;
            services.Configure<PublisherOptions>(options =>
            {
                options.ServiceBusConnectionString = serviceBusOptions.SERVICE_BUS_SEND_CONNECTION_STRING;
                options.TopicName = serviceBusOptions.INTEGRATIONEVENTS_TOPIC_NAME;
                options.TransportType = Azure.Messaging.ServiceBus.ServiceBusTransportType.AmqpWebSockets;
            });
            services.AddPublisher<IntegrationEventProvider>();

            return services;
        }
    }
}
