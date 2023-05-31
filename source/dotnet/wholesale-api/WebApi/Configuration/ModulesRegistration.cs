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

using Azure.Identity;
using Azure.Storage.Files.DataLake;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Common.DatabricksClient;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.WebApi.Configuration.Modules;
using Energinet.DataHub.Wholesale.WebApi.Configuration.Options;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Energinet.DataHub.Wholesale.WebApi.Configuration;

internal static class ServiceCollectionExtensions
{
    public static void AddModules(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        // Add modules
        var connectionStringOptions = configuration.GetSection(ConnectionStringsOptions.ConnectionStrings)
            .Get<ConnectionStringsOptions>();
        serviceCollection.AddBatchesModule(() => connectionStringOptions!.DB_CONNECTION_STRING);

        serviceCollection.AddCalculationResultsModule();

        var serviceBusOptions = configuration.Get<ServiceBusOptions>()!;
        serviceCollection.AddIntegrationEventPublishingModule(
            serviceBusOptions.SERVICE_BUS_SEND_CONNECTION_STRING,
            serviceBusOptions.INTEGRATIONEVENTS_TOPIC_NAME);

        // Add registration that are used by more than one module
        serviceCollection.AddShared(configuration);
    }

    private static void AddShared(this IServiceCollection serviceCollection, IConfiguration configuration)
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

        serviceCollection.AddSingleton<IJobsApiClient, JobsApiClient>();

        serviceCollection.AddDateTimeConfiguration(configuration);
        serviceCollection.AddDataLakeFileSystemClient(configuration);
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
