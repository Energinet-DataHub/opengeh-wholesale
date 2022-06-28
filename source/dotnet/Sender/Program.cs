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
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.MessageHub.Client;
using Energinet.DataHub.Wholesale.Infrastructure.Core;
using Energinet.DataHub.Wholesale.Sender.Configuration;
using Energinet.DataHub.Wholesale.Sender.Monitor;
using Energinet.DataHub.Wholesale.Sender.Persistence;
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
            .ConfigureServices(Middlewares)
            .ConfigureServices(Infrastructure)
            .ConfigureServices(MessageHub)
            .ConfigureServices(HealthCheck)
            .Build();

        await host.RunAsync().ConfigureAwait(false);
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

    private static void Middlewares(IServiceCollection serviceCollection)
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

        var connectionString =
            EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DatabaseConnectionString);
        serviceCollection.AddDbContext<DatabaseContext>(options =>
            options.UseSqlServer(connectionString, o => o.UseNodaTime()));
    }

    private static void HealthCheck(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>();
        serviceCollection.AddScoped<HealthCheckEndpoint>();

        var completedProcessServiceBusConnectionString = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ServiceBusListenConnectionString);
        var completedProcessTopicName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ProcessCompletedTopicName);
        var completedProcessSubscriptionName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ProcessCompletedSubscriptionName);

        var dataAvailableServiceBusConnectionString = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MessageHubServiceBusConnectionString);
        var dataAvailableQueueName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MessageHubDataAvailableQueueName);

        serviceCollection
            .AddHealthChecks()
            .AddLiveCheck()
            .AddAzureServiceBusSubscription(
                completedProcessServiceBusConnectionString,
                completedProcessTopicName,
                completedProcessSubscriptionName)
            .AddAzureServiceBusQueue(
                dataAvailableServiceBusConnectionString,
                dataAvailableQueueName);
    }
}
