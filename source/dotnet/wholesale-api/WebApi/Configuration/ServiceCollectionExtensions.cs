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

using Azure.Identity;
using Azure.Storage.Files.DataLake;
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.App.WebApp.Authentication;
using Energinet.DataHub.Core.App.WebApp.Authorization;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Components.DatabricksClient.DatabricksWheelClient;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.WebApi.Configuration.Options;
using Energinet.DataHub.Wholesale.WebApi.V3.ProcessStepResult;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using ProcessTypeMapper = Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application.Processes.Model.ProcessTypeMapper;

namespace Energinet.DataHub.Wholesale.WebApi.Configuration;

internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds registrations of JwtTokenMiddleware and corresponding dependencies.
    /// </summary>
    public static void AddJwtTokenSecurity(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var options = configuration.Get<JwtOptions>()!;
        serviceCollection.AddJwtBearerAuthentication(options.EXTERNAL_OPEN_ID_URL, options.INTERNAL_OPEN_ID_URL, options.BACKEND_BFF_APP_ID);
        serviceCollection.AddPermissionAuthorization();
    }

    public static void AddCommandStack(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddDbContext<IntegrationEventPublishingDatabaseContext>(
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
        serviceCollection.AddScoped<ICorrelationContext, CorrelationContext>();
        serviceCollection.AddScoped<IJsonSerializer, JsonSerializer>();
        serviceCollection.AddScoped<IProcessStepResultFactory, ProcessStepResultFactory>();

        serviceCollection.AddSingleton<IDatabricksWheelClient, DatabricksWheelClient>();

        serviceCollection.AddDateTimeConfiguration(configuration);
        serviceCollection.AddDataLakeFileSystemClient(configuration);
    }

    public static void AddHealthCheck(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var serviceBusOptions = configuration.Get<ServiceBusOptions>()!;
        var dataLakeOptions = configuration.Get<DataLakeOptions>()!;
        serviceCollection.AddHealthChecks()
            .AddLiveCheck()
            .AddDbContextCheck<IntegrationEventPublishingDatabaseContext>(name: "SqlDatabaseContextCheck")
            .AddDataLakeContainerCheck(dataLakeOptions.STORAGE_ACCOUNT_URI, dataLakeOptions.STORAGE_CONTAINER_NAME)
            .AddAzureServiceBusTopic(
                serviceBusOptions.SERVICE_BUS_MANAGE_CONNECTION_STRING,
                serviceBusOptions.INTEGRATIONEVENTS_TOPIC_NAME,
                name: "IntegrationEventsTopicExists");
    }

    /// <summary>
    /// The middleware to handle properly set a CorrelationContext is only supported for Functions.
    /// This registry will ensure a new CorrelationContext (with a new Id) is set for each session
    /// </summary>
    public static void AddCorrelationContext(this IServiceCollection serviceCollection)
    {
        var serviceDescriptor =
            serviceCollection.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(ICorrelationContext));
        serviceCollection.Remove(serviceDescriptor!);
        serviceCollection.AddScoped<ICorrelationContext>(_ =>
        {
            var correlationContext = new CorrelationContext();
            correlationContext.SetId(Guid.NewGuid().ToString());
            return correlationContext;
        });
    }

    private static void AddDataLakeFileSystemClient(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var options = configuration.Get<DataLakeOptions>()!;
        serviceCollection.AddSingleton<DataLakeFileSystemClient>(_ =>
        {
            var dataLakeServiceClient = new DataLakeServiceClient(new Uri(options.STORAGE_ACCOUNT_URI), new DefaultAzureCredential());
            return dataLakeServiceClient.GetFileSystemClient(options.STORAGE_CONTAINER_NAME);
        });
    }

    private static void AddDateTimeConfiguration(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var options = configuration.Get<DateTimeOptions>()!;
        serviceCollection.AddSingleton<DateTimeZone>(_ =>
        {
            var dateTimeZoneId = options.TIME_ZONE;
            return DateTimeZoneProviders.Tzdb.GetZoneOrNull(dateTimeZoneId)!;
        });
    }
}
