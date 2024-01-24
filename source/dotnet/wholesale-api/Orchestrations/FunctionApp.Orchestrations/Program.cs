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

using Energinet.DataHub.Core.Databricks.Jobs.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Energinet.DataHub.Wholesale.Batches.Application;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Batches.Application.UseCases;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Calculations;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence.Calculations;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.AmountPerChargeResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.CalculationResultCompleted.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EnergyResultProducedV2.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EventProviders;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence.CompletedCalculations;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Register dependencies for calculation activities
        // => NodaTIme
        services.AddSingleton<IClock>(_ => SystemClock.Instance);
        services.AddSingleton<DateTimeZone>(_ =>
        {
            return DateTimeZoneProviders.Tzdb.GetZoneOrNull(Environment.GetEnvironmentVariable("TIME_ZONE")!)!;
        });

        // => SQL Database - Calculations
        services.AddScoped<IDatabaseContext, DatabaseContext>();
        services.AddDbContext<DatabaseContext>(
            options => options.UseSqlServer(
                Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"),
                o =>
                {
                    o.UseNodaTime();
                    o.EnableRetryOnFailure();
                }));
        services.AddScoped<ICalculationRepository, CalculationRepository>();
        services.AddScoped<Energinet.DataHub.Wholesale.Batches.Application.IUnitOfWork, Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence.UnitOfWork>();

        // => SQL Database - Events
        services.AddScoped<IEventsDatabaseContext, EventsDatabaseContext>();
        services.AddDbContext<EventsDatabaseContext>(
            options => options.UseSqlServer(
                Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"),
                o =>
                {
                    o.UseNodaTime();
                    o.EnableRetryOnFailure();
                }));
        services.AddScoped<ICompletedCalculationRepository, CompletedCalculationRepository>();
        services.AddScoped<Energinet.DataHub.Wholesale.Events.Application.UseCases.IUnitOfWork, Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence.UnitOfWork>();

        // => Databricks
        services.AddDatabricksSqlStatementExecution(context.Configuration);
        services.AddDatabricksJobs(context.Configuration);
        services.AddScoped<IDatabricksCalculatorJobSelector, DatabricksCalculatorJobSelector>();
        services.AddScoped<ICalculationParametersFactory, DatabricksCalculationParametersFactory>();
        services.AddScoped<ICalculationEngineClient, CalculationEngineClient>();

        services.AddOptions<DeltaTableOptions>();

        // => ServiceBus (integration events)
        services.Configure<PublisherOptions>(options =>
        {
            options.ServiceBusConnectionString = Environment.GetEnvironmentVariable("SERVICE_BUS_SEND_CONNECTION_STRING")!;
            options.TopicName = Environment.GetEnvironmentVariable("INTEGRATIONEVENTS_TOPIC_NAME")!;
            options.TransportType = Azure.Messaging.ServiceBus.ServiceBusTransportType.AmqpWebSockets;
        });
        services.AddPublisher<IntegrationEventProvider>()
            .AddScoped<IEnergyResultEventProvider, EnergyResultEventProvider>()
                .AddScoped<IEnergyResultQueries, EnergyResultQueries>()
                .AddScoped<IEnergyResultProducedV2Factory, EnergyResultProducedV2Factory>()
            .AddScoped<IWholesaleResultEventProvider, WholesaleResultEventProvider>()
                .AddScoped<IWholesaleResultQueries, WholesaleResultQueries>()
                .AddScoped<IAmountPerChargeResultProducedV1Factory, AmountPerChargeResultProducedV1Factory>()
                .AddScoped<IMonthlyAmountPerChargeResultProducedV1Factory, MonthlyAmountPerChargeResultProducedV1Factory>();

        // => Clients
        services.AddScoped<ICalculationsClient, CalculationsClient>();

        // => Factories / mappers
        services.AddScoped<ICalculationFactory, CalculationFactory>();
        services.AddScoped<ICalculationResultCompletedFactory, CalculationResultCompletedFactory>();
        services.AddScoped<ICompletedCalculationFactory, CompletedCalculationFactory>();
        services.AddScoped<ICalculationDtoMapper, CalculationDtoMapper>();

        // => Handlers
        services.AddScoped<ICreateCalculationHandler, CreateCalculationHandler>();
    })
    .Build();

host.Run();
