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
using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Application.JobRunner;
using Energinet.DataHub.Wholesale.Components.DatabricksClient;
using Energinet.DataHub.Wholesale.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Batches;
using Energinet.DataHub.Wholesale.Infrastructure.Core;
using Energinet.DataHub.Wholesale.Infrastructure.JobRunner;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Batches;
using Energinet.DataHub.Wholesale.Infrastructure.Processes;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using Energinet.DataHub.Wholesale.ProcessManager.Monitor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;

namespace Energinet.DataHub.Wholesale.ProcessManager;

public static class Program
{
    public static async Task Main()
    {
        using var host = BuildAppHost().Build();
        await host.RunAsync().ConfigureAwait(false);
    }

    public static IHostBuilder BuildAppHost()
    {
        return new HostBuilder()
            .ConfigureFunctionsWorkerDefaults(builder =>
            {
                builder.UseMiddleware<CorrelationIdMiddleware>();
                builder.UseMiddleware<FunctionTelemetryScopeMiddleware>();
                builder.UseMiddleware<IntegrationEventMetadataMiddleware>();
            })
            .ConfigureServices(Middlewares)
            .ConfigureServices(Applications)
            .ConfigureServices(Domains)
            .ConfigureServices(Infrastructure)
            .ConfigureServices(HealthCheck);
    }

    private static void Middlewares(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IClock>(_ => SystemClock.Instance);
        serviceCollection.AddScoped<ICorrelationContext, CorrelationContext>();
        serviceCollection.AddScoped<CorrelationIdMiddleware>();
        serviceCollection.AddScoped<IIntegrationEventContext, IntegrationEventContext>();
        serviceCollection.AddScoped<IntegrationEventMetadataMiddleware>();
    }

    private static void Applications(IServiceCollection services)
    {
        services.AddScoped<IBatchApplicationService, BatchApplicationService>();
        services.AddScoped<IBatchExecutionStateHandler, BatchExecutionStateHandler>();
        services.AddScoped<IBatchDtoMapper, BatchDtoMapper>();
        services.AddScoped<ICalculatorJobRunner, DatabricksCalculatorJobRunner>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void Domains(IServiceCollection services)
    {
        services.AddScoped<IBatchFactory, BatchFactory>();
        services.AddScoped<IBatchRepository, BatchRepository>();
    }

    private static void Infrastructure(IServiceCollection serviceCollection)
    {
        serviceCollection.AddApplicationInsights();

        serviceCollection.AddScoped<IDatabaseContext, DatabaseContext>();
        serviceCollection.AddSingleton<IJsonSerializer, JsonSerializer>();

        var batchCompletedMessageType = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.BatchCompletedEventName);
        var processCompletedMessageType = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ProcessCompletedEventName);
        var messageTypes = new Dictionary<Type, string>
        {
            { typeof(BatchCompletedEventDto), batchCompletedMessageType },
            { typeof(ProcessCompletedEventDto), processCompletedMessageType },
        };
        serviceCollection.AddScoped<IServiceBusMessageFactory>(provider =>
        {
            var correlationContext = provider.GetRequiredService<ICorrelationContext>();
            return new ServiceBusMessageFactory(correlationContext, messageTypes);
        });

        var calculationStorageConnectionString = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.CalculationStorageConnectionString);
        var calculationStorageContainerName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.CalculationStorageContainerName);
        serviceCollection.AddSingleton(new DataLakeFileSystemClient(calculationStorageConnectionString, calculationStorageContainerName));

        var connectionString =
            EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DatabaseConnectionString);
        serviceCollection.AddDbContext<DatabaseContext>(options =>
            options.UseSqlServer(connectionString, o =>
            {
                o.UseNodaTime();
                o.EnableRetryOnFailure();
            }));

        var serviceBusConnectionString =
            EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ServiceBusSendConnectionString);

        var domainEventsTopicName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DomainEventsTopicName);
        serviceCollection.AddBatchCompletedPublisher(serviceBusConnectionString, domainEventsTopicName);
        serviceCollection.AddProcessCompletedPublisher(serviceBusConnectionString, domainEventsTopicName);

        serviceCollection.AddScoped<IDatabricksCalculatorJobSelector, DatabricksCalculatorJobSelector>();
        serviceCollection
            .AddScoped<ICalculatorJobParametersFactory, DatabricksCalculatorJobParametersFactory>();

        serviceCollection.AddSingleton(_ =>
        {
            var dbwUrl = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DatabricksWorkspaceUrl);
            var dbwToken = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DatabricksWorkspaceToken);

            return DatabricksWheelClient.CreateClient(dbwUrl, dbwToken);
        });
    }

    private static void HealthCheck(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>();
        serviceCollection.AddScoped<HealthCheckEndpoint>();

        var serviceBusConnectionString =
            EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ServiceBusManageConnectionString);
        var domainEventsTopicName =
            EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DomainEventsTopicName);
        var batchCompletedSubscriptionPublishProcessesCompleted =
            EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.PublishProcessesCompletedWhenCompletedBatchSubscriptionName);
        var batchCompletedSubscriptionZipBasisData =
            EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ZipBasisDataWhenCompletedBatchSubscriptionName);

        serviceCollection
            .AddHealthChecks()
            .AddLiveCheck()
            .AddDbContextCheck<DatabaseContext>(name: "SqlDatabaseContextCheck")
            .AddAzureServiceBusTopic(
                connectionString: serviceBusConnectionString,
                topicName: domainEventsTopicName,
                name: "DomainEventsTopicExists")
            .AddAzureServiceBusSubscription(
                connectionString: serviceBusConnectionString,
                topicName: domainEventsTopicName,
                subscriptionName: batchCompletedSubscriptionPublishProcessesCompleted,
                name: "BatchCompletedSubscriptionPublishProcessesCompleted")
            .AddAzureServiceBusSubscription(
                connectionString: serviceBusConnectionString,
                topicName: domainEventsTopicName,
                subscriptionName: batchCompletedSubscriptionZipBasisData,
                name: "BatchCompletedSubscriptionZipBasisData")
            .AddDatabricksCheck(
                EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DatabricksWorkspaceUrl),
                EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DatabricksWorkspaceToken));
    }
}
