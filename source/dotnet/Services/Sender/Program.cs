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
using Energinet.DataHub.Core.App.Common.Abstractions.IntegrationEventContext;
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.DependencyInjection;
using Energinet.DataHub.Core.App.FunctionApp.FunctionTelemetryScope;
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.MessageHub.Client;
using Energinet.DataHub.MessageHub.Client.SimpleInjector;
using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Application.Infrastructure;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.BasisData;
using Energinet.DataHub.Wholesale.Infrastructure.Core;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Batches;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Persistence.Processes;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Services;
using Energinet.DataHub.Wholesale.Sender.Monitor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Sender;

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
            .ConfigureServices(ApplicationServices)
            .ConfigureServices(MiddlewareServices)
            .ConfigureServices(Domains)
            .ConfigureServices(Infrastructure)
            .ConfigureServices(MessageHub)
            .ConfigureServices(HealthCheck);
    }

    private static void ApplicationServices(IServiceCollection services)
    {
        services.AddScoped<IDocumentSender, DocumentSender>();
    }

    private static void MiddlewareServices(IServiceCollection services)
    {
        services.AddScoped<IClock>(_ => SystemClock.Instance);
        services.AddScoped<IDataAvailableNotifier, DataAvailableNotifier>();
        services.AddScoped<ISenderUnitOfWork, SenderUnitOfWork>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDocumentFactory, DocumentFactory>();
        services.AddScoped<IDocumentIdGenerator, DocumentIdGenerator>();
        services.AddScoped<ISeriesIdGenerator, SeriesIdGenerator>();
        services.AddScoped<IDataAvailableNotificationFactory, DataAvailableNotificationFactory>();
        services.AddScoped<IProcessRepository, ProcessRepository>();
        services.AddScoped<ICorrelationContext, CorrelationContext>();
        services.AddScoped<CorrelationIdMiddleware>();
        services.AddScoped<IIntegrationEventContext, IntegrationEventContext>();
        services.AddScoped<IntegrationEventMetadataMiddleware>();
        services.AddScoped<IntegrationEventMetadataMiddleware>();
    }

    private static void Domains(IServiceCollection services)
    {
        services.AddScoped<IBatchRepository, BatchRepository>();
    }

    private static void Infrastructure(IServiceCollection serviceCollection)
    {
        serviceCollection.AddApplicationInsights();
        serviceCollection.AddSingleton<IJsonSerializer, JsonSerializer>();

        var calculatorResultConnection = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.CalculationStorageConnectionString);
        var calculatorResultFileSystem = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.CalculationStorageContainerName);
        serviceCollection.AddSingleton(new DataLakeFileSystemClient(calculatorResultConnection, calculatorResultFileSystem));

        serviceCollection.AddScoped<IDatabaseContext, DatabaseContext>();
        serviceCollection.AddScoped<ISenderDatabaseContext, SenderDatabaseContext>();
        var connectionString =
            EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DatabaseConnectionString);
        serviceCollection.AddDbContext<SenderDatabaseContext>(options =>
            options.UseSqlServer(connectionString, o =>
            {
                o.UseNodaTime();
                o.EnableRetryOnFailure();
            }));
        serviceCollection.AddDbContext<DatabaseContext>(options =>
            options.UseSqlServer(connectionString, o =>
            {
                o.UseNodaTime();
                o.EnableRetryOnFailure();
            }));

        serviceCollection.AddScoped<ICalculatedResultReader, CalculatedResultsReader>();
        var calculationStorageConnectionString = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.CalculationStorageConnectionString);
        var calculationStorageContainerName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.CalculationStorageContainerName);
        var dataLakeFileSystemClient = new DataLakeFileSystemClient(calculationStorageConnectionString, calculationStorageContainerName);
        serviceCollection.AddScoped<IWebFilesZipper>(_ => null!);
        serviceCollection.AddScoped<IBatchFileManager>(
            provider => new BatchFileManager(
                dataLakeFileSystemClient,
                provider.GetService<IWebFilesZipper>()!,
                provider.GetRequiredService<ILogger<IBatchFileManager>>()));
        serviceCollection.AddHttpClient();
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
            .AddDbContextCheck<SenderDatabaseContext>("DatabaseContext")
            .AddAzureServiceBusTopic(
                EnvironmentSettingNames.ServiceBusManageConnectionString.Val(),
                EnvironmentSettingNames.DomainEventsTopicName.Val(),
                "ProcessCompletedTopic")
            .AddAzureServiceBusSubscription(
                EnvironmentSettingNames.ServiceBusManageConnectionString.Val(),
                EnvironmentSettingNames.DomainEventsTopicName.Val(),
                EnvironmentSettingNames.SendDataAvailableWhenCompletedProcessSubscriptionName.Val(),
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
                "MessageHubReplyQueue")
            .AddDataLakeCheck(
                EnvironmentSettingNames.CalculationStorageConnectionString.Val(),
                EnvironmentSettingNames.CalculationStorageContainerName.Val());
    }
}
