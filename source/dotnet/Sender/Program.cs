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

using Energinet.DataHub.Core.App.Common.Abstractions.IntegrationEventContext;
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.FunctionTelemetryScope;
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.MessageHub.Client;
using Energinet.DataHub.Wholesale.Infrastructure.Core;
using Energinet.DataHub.Wholesale.Sender.Configuration;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Persistence.Processes;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Services;
using Energinet.DataHub.Wholesale.Sender.Monitor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Wholesale.Sender;

public static class Program
{
    public static async Task Main()
    {
        using var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults(builder =>
            {
                builder.UseMiddleware<CorrelationIdMiddleware>();
                builder.UseMiddleware<FunctionTelemetryScopeMiddleware>();
                builder.UseMiddleware<IntegrationEventMetadataMiddleware>();
            })
            .ConfigureServices(ApplicationServices)
            .ConfigureServices(DomainServices)
            .ConfigureServices(MiddlewareServices)
            .ConfigureServices(Infrastructure)
            .ConfigureServices(MessageHub)
            .ConfigureServices(HealthCheck)
            .Build();

        await host.RunAsync().ConfigureAwait(false);
    }

    private static void ApplicationServices(IServiceCollection services)
    {
        services.AddScoped<IDataAvailableNotifier, DataAvailableNotifier>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDataAvailableNotificationFactory, DataAvailableNotificationFactory>();
    }

    private static void DomainServices(IServiceCollection services)
    {
        services.AddScoped<IProcessRepository, ProcessRepository>();
    }

    private static void MiddlewareServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ICorrelationContext, CorrelationContext>();
        serviceCollection.AddScoped<CorrelationIdMiddleware>();
        serviceCollection.AddScoped<FunctionTelemetryScopeMiddleware>();
        serviceCollection.AddScoped<IIntegrationEventContext, IntegrationEventContext>();
        serviceCollection.AddScoped<IntegrationEventMetadataMiddleware>();
    }

    private static void Infrastructure(IServiceCollection serviceCollection)
    {
        serviceCollection.AddApplicationInsightsTelemetryWorkerService(
            EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.AppInsightsInstrumentationKey));
        serviceCollection.AddSingleton<IJsonSerializer, JsonSerializer>();

        serviceCollection.AddScoped<IDatabaseContext, DatabaseContext>();
        var connectionString =
            EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DatabaseConnectionString);
        serviceCollection.AddDbContext<DatabaseContext>(options =>
            options.UseSqlServer(connectionString, o => o.UseNodaTime()));
    }

    private static void MessageHub(IServiceCollection services)
    {
        var serviceBusConnectionString = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MessageHubServiceBusConnectionString);
        var dataAvailableQueue = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MessageHubDataAvailableQueueName);
        var domainReplyQueue = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MessageHubReplyQueueName);
        var storageServiceConnectionString = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MessageHubStorageConnectionString);
        var azureBlobStorageContainerName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MessageHubStorageContainerName);

        services.AddMessageHub(
            serviceBusConnectionString,
            new MessageHubConfig(dataAvailableQueue, domainReplyQueue),
            storageServiceConnectionString,
            new StorageConfig(azureBlobStorageContainerName));
    }

    private static void HealthCheck(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>();
        serviceCollection.AddScoped<HealthCheckEndpoint>();

        serviceCollection
            .AddHealthChecks()
            .AddLiveCheck()
            .AddDbContextCheck<DatabaseContext>()
            .AddAzureServiceBusTopic(
                EnvironmentSettingNames.ServiceBusManageConnectionString.Val(),
                EnvironmentSettingNames.ProcessCompletedTopicName.Val())
            .AddAzureServiceBusSubscription(
                EnvironmentSettingNames.ServiceBusManageConnectionString.Val(),
                EnvironmentSettingNames.ProcessCompletedTopicName.Val(),
                EnvironmentSettingNames.ProcessCompletedSubscriptionName.Val())
            .AddAzureServiceBusQueue(
                EnvironmentSettingNames.DataHubServiceBusManageConnectionString.Val(),
                EnvironmentSettingNames.MessageHubDataAvailableQueueName.Val())
            .AddAzureServiceBusQueue(
                connectionString: EnvironmentSettingNames.DataHubServiceBusManageConnectionString.Val(),
                queueName: EnvironmentSettingNames.MessageHubRequestQueue.Val())
            .AddAzureServiceBusQueue(
                connectionString: EnvironmentSettingNames.DataHubServiceBusManageConnectionString.Val(),
                queueName: EnvironmentSettingNames.MessageHubReplyQueueName.Val());
    }
}
