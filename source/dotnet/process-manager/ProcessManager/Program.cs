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
using Energinet.DataHub.Core.App.Common.Abstractions.IntegrationEventContext;
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.DependencyInjection;
using Energinet.DataHub.Core.App.FunctionApp.FunctionTelemetryScope;
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Components.DatabricksClient.DatabricksWheelClient;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Core;
using Energinet.DataHub.Wholesale.Infrastructure.EventPublishers;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.ProcessManager.Monitor;
using Energinet.DataHub.Wholesale.WebApi.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;

namespace Energinet.DataHub.Wholesale.ProcessManager;

public static class Program
{
    public static async Task Main()
    {
        using var host = CreateHostBuilder().Build();
        await host.RunAsync().ConfigureAwait(false);
    }

    public static IHostBuilder CreateHostBuilder()
    {
        return new HostBuilder()
            .ConfigureFunctionsWorkerDefaults(builder =>
            {
                builder.UseMiddleware<CorrelationIdMiddleware>();
                builder.UseMiddleware<FunctionTelemetryScopeMiddleware>();
                builder.UseMiddleware<IntegrationEventMetadataMiddleware>();
            })
            // .ConfigureServices(Modules)
            // .ConfigureServices(Middlewares)
            // .ConfigureServices(Infrastructure)
            // .ConfigureServices(DateTime)
            .ConfigureServices(HealthCheck);
    }

    private static void Modules(IServiceCollection serviceCollection)
    {
        serviceCollection.AddBatchesModule(
            () => EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DatabaseConnectionString));

        serviceCollection.AddCalculationResultsModule();

        var serviceBusConnectionString =
            EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ServiceBusManageConnectionString);
        var integrationEventTopicName =
            EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.IntegrationEventsTopicName);
        serviceCollection.AddIntegrationEventPublishingModule(serviceBusConnectionString, integrationEventTopicName);
    }

    private static void Middlewares(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ICorrelationContext, CorrelationContext>();
        serviceCollection.AddScoped<CorrelationIdMiddleware>();
        serviceCollection.AddScoped<IIntegrationEventContext, IntegrationEventContext>();
        serviceCollection.AddScoped<IntegrationEventMetadataMiddleware>();
    }

    private static void Infrastructure(IServiceCollection serviceCollection)
    {
        serviceCollection.AddApplicationInsights();

        var dataLakeServiceClient = new DataLakeServiceClient(new Uri(EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.CalculationStorageAccountUri)), new DefaultAzureCredential());
        var dataLakeFileSystemClient = dataLakeServiceClient.GetFileSystemClient(EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.CalculationStorageContainerName));

        serviceCollection.AddSingleton(dataLakeFileSystemClient);

        var connectionString =
            EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DatabaseConnectionString);
        serviceCollection.AddDbContext<IntegrationEventPublishingDatabaseContext>(options =>
            options.UseSqlServer(connectionString, o =>
            {
                o.UseNodaTime();
                o.EnableRetryOnFailure();
            }));

        RegisterEventPublishers(serviceCollection);

        serviceCollection.AddSingleton(_ =>
            {
                var dbwUrl = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DatabricksWorkspaceUrl);
                var dbwToken = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DatabricksWorkspaceToken);
                return DatabricksWheelClient.CreateClient(dbwUrl, dbwToken);
            });
    }

    private static void RegisterEventPublishers(IServiceCollection serviceCollection)
    {
        var serviceBusConnectionString =
            EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ServiceBusManageConnectionString);
        var messageTypes = new Dictionary<Type, string>
        {
            {
                typeof(BatchCompletedEventDto),
                EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.BatchCompletedEventName)
            },
            {
                typeof(ProcessCompletedEventDto),
                EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ProcessCompletedEventName)
            },
        };
        var domainEventTopicName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DomainEventsTopicName);
        serviceCollection.AddDomainEventPublisher(serviceBusConnectionString, domainEventTopicName, new MessageTypeDictionary(messageTypes));
    }

    private static void DateTime(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IClock>(_ => SystemClock.Instance);
        var dateTimeZoneId = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DateTimeZoneId);
        var dateTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(dateTimeZoneId);
        if (dateTimeZone == null)
            throw new ArgumentNullException($"Cannot resolve date time zone object for zone id '{dateTimeZoneId}' from application setting '{EnvironmentSettingNames.DateTimeZoneId}'");
        serviceCollection.AddSingleton(dateTimeZone);
    }

    private static void HealthCheck(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>();
        serviceCollection.AddScoped<HealthCheckEndpoint>();

        // var serviceBusConnectionString =
        //     EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ServiceBusManageConnectionString);
        // var domainEventsTopicName =
        //     EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DomainEventsTopicName);
        // var batchCompletedSubscriptionPublishProcessesCompleted =
        //     EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.PublishProcessesCompletedWhenCompletedBatchSubscriptionName);
        // var batchCompletedSubscriptionZipBasisData =
        //     EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.CreateSettlementReportsWhenCompletedBatchSubscriptionName);
        // var integrationEventsTopicName =
        //     EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DomainEventsTopicName);
        serviceCollection
            .AddHealthChecks()
            .AddLiveCheck();
        // .AddDbContextCheck<IntegrationEventPublishingDatabaseContext>(name: "SqlDatabaseContextCheck")
        // .AddAzureServiceBusTopic(
        //     connectionString: serviceBusConnectionString,
        //     topicName: domainEventsTopicName,
        //     name: "DomainEventsTopicExists")
        // .AddAzureServiceBusSubscription(
        //     connectionString: serviceBusConnectionString,
        //     topicName: domainEventsTopicName,
        //     subscriptionName: batchCompletedSubscriptionPublishProcessesCompleted,
        //     name: "BatchCompletedSubscriptionPublishProcessesCompleted")
        // .AddAzureServiceBusSubscription(
        //     connectionString: serviceBusConnectionString,
        //     topicName: domainEventsTopicName,
        //     subscriptionName: batchCompletedSubscriptionZipBasisData,
        //     name: "BatchCompletedSubscriptionZipBasisData")
        // .AddDatabricksCheck(
        //     EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DatabricksWorkspaceUrl),
        //     EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DatabricksWorkspaceToken))
        // .AddDataLakeContainerCheck(
        //     EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.CalculationStorageAccountUri),
        //     EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.CalculationStorageContainerName))
        // .AddAzureServiceBusTopic(
        //     connectionString: serviceBusConnectionString,
        //     topicName: integrationEventsTopicName,
        //     name: "IntegrationEventsTopicExists");
    }
}
