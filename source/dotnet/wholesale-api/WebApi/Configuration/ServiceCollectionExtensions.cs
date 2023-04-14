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

    public static void AddCommandStack(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConfigurationSettingNames.DbConnectionString);
        if (connectionString == null)
            throw new ArgumentNullException(ConfigurationSettingNames.DbConnectionString, "does not exist in configuration settings");

        services.AddDbContext<DatabaseContext>(
            options => options.UseSqlServer(connectionString, o =>
            {
                o.UseNodaTime();
                o.EnableRetryOnFailure();
            }));
        services.AddScoped<IClock>(_ => SystemClock.Instance);
        services.AddScoped<IDatabaseContext, DatabaseContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IBatchApplicationService, BatchApplicationService>();
        services.AddScoped<ISettlementReportApplicationService, SettlementReportApplicationService>();
        services.AddScoped<ISettlementReportRepository, SettlementReportRepository>();
        services.AddScoped<IStreamZipper, StreamZipper>();
        var calculationStorageConnectionString = configuration[ConfigurationSettingNames.CalculationStorageConnectionString];
        var calculationStorageContainerName = configuration[ConfigurationSettingNames.CalculationStorageContainerName];
        var dataLakeFileSystemClient = new DataLakeFileSystemClient(calculationStorageConnectionString, calculationStorageContainerName);
        services.AddSingleton(dataLakeFileSystemClient);
        services.AddScoped<HttpClient>(_ => null!);
        services.AddScoped<IBatchFactory, BatchFactory>();
        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<IBatchExecutionStateDomainService, BatchExecutionStateDomainService>();
        services.AddScoped<IBatchDtoMapper, BatchDtoMapper>();
        services.AddScoped<IProcessTypeMapper, ProcessTypeMapper>();
        services.AddScoped<ICalculationDomainService, CalculationDomainService>();
        services.AddScoped<ICalculationEngineClient, CalculationEngineClient>();

        services.AddOptions<DatabricksOptions>().Bind(configuration);
        services.AddSingleton<IDatabricksWheelClient, DatabricksWheelClient>();

        services.AddScoped<IDatabricksCalculatorJobSelector, DatabricksCalculatorJobSelector>();
        services.AddScoped<ICalculationParametersFactory>(_ => null!); // Unused in the use cases of this app
        services.AddScoped<IProcessStepApplicationService, ProcessStepApplicationService>();
        services.AddScoped<IProcessStepResultMapper, ProcessStepResultMapper>();
        services.AddScoped<IProcessStepResultRepository, ProcessStepResultRepository>();
        services.AddScoped<IDataLakeClient, DataLakeClient>();
        services.AddScoped<IActorRepository, ActorRepository>();
        services.AddScoped<IJsonNewlineSerializer, JsonNewlineSerializer>();
        services.AddScoped<ICorrelationContext, CorrelationContext>();
        services.AddScoped<IJsonSerializer, JsonSerializer>();
        services.AddScoped<IProcessStepResultFactory, ProcessStepResultFactory>();
        services.AddScoped<IProcessCompletedEventDtoFactory, ProcessCompletedEventDtoFactory>();

        RegisterDomainEventPublisher(services, configuration);

        services.ConfigureDateTime(configuration);
    }

    private static void RegisterDomainEventPublisher(IServiceCollection services, IConfiguration configuration)
    {
        var serviceBusConnectionString =
            configuration[ConfigurationSettingNames.ServiceBusSendConnectionString]!;
        var messageTypes = new Dictionary<Type, string>
        {
            {
                typeof(BatchCreatedDomainEvent),
                configuration[ConfigurationSettingNames.BatchCreatedEventName]!
            },
        };

        var domainEventTopicName = configuration[ConfigurationSettingNames.DomainEventsTopicName]!;
        services.AddDomainEventPublisher(serviceBusConnectionString, domainEventTopicName, new MessageTypeDictionary(messageTypes));
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
