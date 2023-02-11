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
using Energinet.DataHub.Wholesale.Infrastructure.Core;
using Energinet.DataHub.Wholesale.Infrastructure.EventPublishers;
using Energinet.DataHub.Wholesale.Infrastructure.Integration.DataLake;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Batches;
using Energinet.DataHub.Wholesale.Infrastructure.Processes;
using Energinet.DataHub.Wholesale.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.WebApi.Controllers.V2;
using Energinet.DataHub.Wholesale.WebApi.V3.ProcessStepResult;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Energinet.DataHub.Wholesale.WebApi.Configuration;

internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds registrations of JwtTokenMiddleware and corresponding dependencies.
    /// </summary>
    /// <param name="serviceCollection">ServiceCollection container</param>
    public static void AddJwtTokenSecurity(this IServiceCollection serviceCollection)
    {
        var externalOpenIdUrl = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ExternalOpenIdUrl);
        var internalOpenIdUrl = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.InternalOpenIdUrl);
        var backendAppId = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.BackendAppId);

        serviceCollection.AddJwtBearerAuthentication(externalOpenIdUrl, internalOpenIdUrl, backendAppId);
        serviceCollection.AddPermissionAuthorization();
    }

    public static void AddWebApiHostRegistrations(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IProcessStepResultFactory, ProcessStepResultFactory>();
    }

    public static void AddCommandStack(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(EnvironmentSettingNames.DbConnectionString);
        if (connectionString == null)
            throw new ArgumentNullException(EnvironmentSettingNames.DbConnectionString, "does not exist in configuration settings");

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
        var calculationStorageConnectionString = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.CalculationStorageConnectionString);
        var calculationStorageContainerName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.CalculationStorageContainerName);
        var dataLakeFileSystemClient = new DataLakeFileSystemClient(calculationStorageConnectionString, calculationStorageContainerName);
        services.AddSingleton(dataLakeFileSystemClient);
        services.AddScoped<HttpClient>(_ => null!);
        services.AddScoped<IBatchFactory, BatchFactory>();
        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<IBatchExecutionStateDomainService, BatchExecutionStateDomainService>();
        services.AddScoped<IBatchDtoMapper, BatchDtoMapper>();
        services.AddScoped<IBatchDtoV2Mapper, BatchDtoV2Mapper>();
        services.AddScoped<IProcessTypeMapper, ProcessTypeMapper>();
        services.AddScoped<ICalculationDomainService, CalculationDomainService>();
        services.AddScoped<ICalculationEngineClient, CalculationEngineClient>();

        services.AddSingleton(_ =>
        {
            var dbwUrl = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DatabricksWorkspaceUrl);
            var dbwToken = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DatabricksWorkspaceToken);

            return DatabricksWheelClient.CreateClient(dbwUrl, dbwToken);
        });
        services.AddScoped<IDatabricksCalculatorJobSelector, DatabricksCalculatorJobSelector>();
        services.AddScoped<ICalculationParametersFactory>(_ => null!); // Unused in the use cases of this app
        services.AddScoped<IProcessStepApplicationService, ProcessStepApplicationService>();
        services.AddScoped<IProcessStepResultMapper, ProcessStepResultMapper>();
        services.AddScoped<IProcessStepResultRepository, ProcessStepResultRepository>();
        services.AddScoped<IBatchRequestDtoValidator, BatchRequestDtoValidator>();
        services.AddScoped<IDataLakeClient, DataLakeClient>();
        services.AddScoped<IActorRepository, ActorRepository>();
        services.AddScoped<IJsonNewlineSerializer, JsonNewlineSerializer>();
        services.AddScoped<ICorrelationContext, CorrelationContext>();
        services.AddScoped<IJsonSerializer, JsonSerializer>();

        RegisterDomainEventPublisher(services);

        services.ConfigureDateTime();
    }

    private static void RegisterDomainEventPublisher(IServiceCollection services)
    {
        var serviceBusConnectionString =
            EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ServiceBusSendConnectionString);
        var messageTypes = new Dictionary<Type, string>
        {
            {
                typeof(BatchCreatedDomainEventDto),
                EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.BatchCreatedEventName)
            },
        };

        var domainEventTopicName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DomainEventsTopicName);
        services.AddDomainEventPublisher(serviceBusConnectionString, domainEventTopicName, new MessageTypeDictionary(messageTypes));
    }

    private static void ConfigureDateTime(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IClock>(_ => SystemClock.Instance);
        var dateTimeZoneId = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DateTimeZoneId);
        var dateTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(dateTimeZoneId);
        if (dateTimeZone == null)
            throw new ArgumentNullException($"Cannot resolve date time zone object for zone id '{dateTimeZoneId}' from application setting '{EnvironmentSettingNames.DateTimeZoneId}'");
        serviceCollection.AddSingleton(dateTimeZone);
    }
}
