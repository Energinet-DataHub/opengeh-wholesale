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

using Azure.Storage.Files.DataLake;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.App.WebApp.Authentication;
using Energinet.DataHub.Core.App.WebApp.Authorization;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Application.Batches.Model;
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Application.ProcessStep;
using Energinet.DataHub.Wholesale.Application.ProcessStep.Model;
using Energinet.DataHub.Wholesale.Application.SettlementReport;
using Energinet.DataHub.Wholesale.Components.DatabricksClient;
using Energinet.DataHub.Wholesale.Domain.ActorAggregate;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.BatchExecutionStateDomainService;
using Energinet.DataHub.Wholesale.Domain.CalculationDomainService;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Energinet.DataHub.Wholesale.Domain.SettlementReportAggregate;
using Energinet.DataHub.Wholesale.Infrastructure;
using Energinet.DataHub.Wholesale.Infrastructure.BatchActor;
using Energinet.DataHub.Wholesale.Infrastructure.Calculations;
using Energinet.DataHub.Wholesale.Infrastructure.EventPublishers;
using Energinet.DataHub.Wholesale.Infrastructure.Integration.DataLake;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Batches;
using Energinet.DataHub.Wholesale.Infrastructure.Processes;
using Energinet.DataHub.Wholesale.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.WebApi.V3.ProcessStepResult;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using ProcessTypeMapper = Energinet.DataHub.Wholesale.Application.Processes.Model.ProcessTypeMapper;

namespace Energinet.DataHub.Wholesale.WebApi.Configuration;

internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds registrations of JwtTokenMiddleware and corresponding dependencies.
    /// </summary>
    public static void AddJwtTokenSecurity(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var externalOpenIdUrl = configuration[ConfigurationSettingNames.ExternalOpenIdUrl]!;
        var internalOpenIdUrl = configuration[ConfigurationSettingNames.InternalOpenIdUrl]!;
        var backendAppId = configuration[ConfigurationSettingNames.BackendBffAppId]!;

        serviceCollection.AddJwtBearerAuthentication(externalOpenIdUrl, internalOpenIdUrl, backendAppId);
        serviceCollection.AddPermissionAuthorization();
    }

    public static void AddCommandStack(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddDbContext<DatabaseContext>(
            options => options.UseSqlServer(
                configuration
                    .GetSection(ConnectionStringsOptions.ConnectionStrings)
                    .Get<ConnectionStringsOptions>()!.DB_CONNECTION_STRING,
                o =>
            {
                o.UseNodaTime();
                o.EnableRetryOnFailure();
            }));
        serviceCollection.AddScoped<IClock>(_ => SystemClock.Instance);
        serviceCollection.AddScoped<IDatabaseContext, DatabaseContext>();
        serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
        serviceCollection.AddScoped<IBatchApplicationService, BatchApplicationService>();
        serviceCollection.AddScoped<ISettlementReportApplicationService, SettlementReportApplicationService>();
        serviceCollection.AddScoped<ISettlementReportRepository, SettlementReportRepository>();
        serviceCollection.AddScoped<IStreamZipper, StreamZipper>();

        var calculationStorageConnectionString = configuration[ConfigurationSettingNames.CalculationStorageConnectionString];
        var calculationStorageContainerName = configuration[ConfigurationSettingNames.CalculationStorageContainerName];
        var dataLakeFileSystemClient = new DataLakeFileSystemClient(calculationStorageConnectionString, calculationStorageContainerName);
        serviceCollection.AddSingleton(dataLakeFileSystemClient);

        serviceCollection.AddScoped<HttpClient>(_ => null!);
        serviceCollection.AddScoped<IBatchFactory, BatchFactory>();
        serviceCollection.AddScoped<IBatchRepository, BatchRepository>();
        serviceCollection.AddScoped<IBatchExecutionStateDomainService, BatchExecutionStateDomainService>();
        serviceCollection.AddScoped<IBatchDtoMapper, BatchDtoMapper>();
        serviceCollection.AddScoped<IProcessTypeMapper, ProcessTypeMapper>();
        serviceCollection.AddScoped<ICalculationDomainService, CalculationDomainService>();
        serviceCollection.AddScoped<ICalculationEngineClient, CalculationEngineClient>();
        serviceCollection.AddScoped<IDatabricksCalculatorJobSelector, DatabricksCalculatorJobSelector>();
        serviceCollection.AddScoped<ICalculationParametersFactory>(_ => null!); // Unused in the use cases of this app
        serviceCollection.AddScoped<IProcessStepApplicationService, ProcessStepApplicationService>();
        serviceCollection.AddScoped<IProcessStepResultMapper, ProcessStepResultMapper>();
        serviceCollection.AddScoped<IProcessStepResultRepository, ProcessStepResultRepository>();
        serviceCollection.AddScoped<IDataLakeClient, DataLakeClient>();
        serviceCollection.AddScoped<IActorRepository, ActorRepository>();
        serviceCollection.AddScoped<IJsonNewlineSerializer, JsonNewlineSerializer>();
        serviceCollection.AddScoped<ICorrelationContext, CorrelationContext>();
        serviceCollection.AddScoped<IJsonSerializer, JsonSerializer>();
        serviceCollection.AddScoped<IProcessStepResultFactory, ProcessStepResultFactory>();
        serviceCollection.AddScoped<IProcessCompletedEventDtoFactory, ProcessCompletedEventDtoFactory>();

        serviceCollection.AddOptions<DatabricksOptions>().Bind(configuration);
        serviceCollection.AddSingleton<IDatabricksWheelClient, DatabricksWheelClient>();

        RegisterDomainEventPublisher(serviceCollection, configuration);

        serviceCollection.ConfigureDateTime(configuration);
    }

    private static void RegisterDomainEventPublisher(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var serviceBusConnectionString =
            configuration[ConfigurationSettingNames.ServiceBusSendConnectionString]!;
        var messageTypes = new Dictionary<Type, string>
        {
            {
                typeof(BatchCreatedDomainEventDto),
                configuration[ConfigurationSettingNames.BatchCreatedEventName]!
            },
        };

        var domainEventTopicName = configuration[ConfigurationSettingNames.DomainEventsTopicName]!;
        serviceCollection.AddDomainEventPublisher(serviceBusConnectionString, domainEventTopicName, new MessageTypeDictionary(messageTypes));
    }

    private static void ConfigureDateTime(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddScoped<IClock>(_ => SystemClock.Instance);
        var dateTimeZoneId = configuration[ConfigurationSettingNames.DateTimeZoneId]!;
        var dateTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(dateTimeZoneId);
        if (dateTimeZone == null)
            throw new ArgumentNullException($"Cannot resolve date time zone object for zone id '{dateTimeZoneId}' from application setting '{ConfigurationSettingNames.DateTimeZoneId}'");
        serviceCollection.AddSingleton(dateTimeZone);
    }
}
