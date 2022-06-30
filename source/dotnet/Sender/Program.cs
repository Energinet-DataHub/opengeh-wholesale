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
using NodaTime;

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
            .ConfigureServices(MiddlewareServices)
            .ConfigureServices(Infrastructure)
            .ConfigureServices(MessageHub)
            .ConfigureServices(HealthCheck)
            .Build();

        await host.RunAsync().ConfigureAwait(false);
    }

    private static void ApplicationServices(IServiceCollection services)
    {
        services.AddScoped<IDocumentSender, DocumentSender>();
    }

    private static void MiddlewareServices(IServiceCollection services)
    {
        services.AddScoped(typeof(IClock), _ => SystemClock.Instance);
        services.AddScoped<IDataAvailableNotifier, DataAvailableNotifier>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDocumentFactory, DocumentFactory>();
        services.AddScoped<IDataAvailableNotificationFactory, DataAvailableNotificationFactory>();
        services.AddScoped<IProcessRepository, ProcessRepository>();
        services.AddScoped<ICorrelationContext, CorrelationContext>();
        services.AddScoped<CorrelationIdMiddleware>();
        services.AddScoped<FunctionTelemetryScopeMiddleware>();
        services.AddScoped<IIntegrationEventContext, IntegrationEventContext>();
        services.AddScoped<IntegrationEventMetadataMiddleware>();
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
        var messageHubSendConnectionString = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MessageHubServiceBusSendConnectionString);
        var dataAvailableQueue = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MessageHubDataAvailableQueueName);
        var domainReplyQueue = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MessageHubReplyQueueName);
        var storageServiceConnectionString = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MessageHubStorageConnectionString);
        var azureBlobStorageContainerName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MessageHubStorageContainerName);

        services.AddMessageHub(
            messageHubSendConnectionString,
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
            .AddDbContextCheck<DatabaseContext>("DatabaseContext")
            .AddAzureServiceBusTopic(
                EnvironmentSettingNames.ServiceBusManageConnectionString.Val(),
                EnvironmentSettingNames.ProcessCompletedTopicName.Val(),
                "ProcessCompletedTopic")
            .AddAzureServiceBusSubscription(
                EnvironmentSettingNames.ServiceBusManageConnectionString.Val(),
                EnvironmentSettingNames.ProcessCompletedTopicName.Val(),
                EnvironmentSettingNames.ProcessCompletedSubscriptionName.Val(),
                "ProcessCompletedSubscription")
            .AddAzureServiceBusQueue(
                EnvironmentSettingNames.DataHubServiceBusManageConnectionString.Val(),
                EnvironmentSettingNames.MessageHubDataAvailableQueueName.Val(),
                "MessageHubDataAvailableQueue")
            .AddAzureServiceBusQueue(
                EnvironmentSettingNames.DataHubServiceBusManageConnectionString.Val(),
                EnvironmentSettingNames.MessageHubRequestQueueName.Val(),
                "MessageHubRequestQueue")
            .AddAzureServiceBusQueue(
                EnvironmentSettingNames.DataHubServiceBusManageConnectionString.Val(),
                EnvironmentSettingNames.MessageHubReplyQueueName.Val(),
                "MessageHubReplyQueue");
    }
}
