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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.App.Common.Abstractions.IntegrationEventContext;
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.FunctionTelemetryScope;
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Application.JobRunner;
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Components.DatabricksClient;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Infrastructure;
using Energinet.DataHub.Wholesale.Infrastructure.Core;
using Energinet.DataHub.Wholesale.Infrastructure.JobRunner;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Batches;
using Energinet.DataHub.Wholesale.Infrastructure.Registration;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using Energinet.DataHub.Wholesale.ProcessManager.Monitor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Wholesale.ProcessManager;

public class Program
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
            .ConfigureServices(Applications)
            .ConfigureServices(Domains)
            .ConfigureServices(Infrastructure)
            .ConfigureServices(HealthCheck)
            .Build();

        await host.RunAsync().ConfigureAwait(false);
    }

    private static void Middlewares(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ICorrelationContext, CorrelationContext>();
        serviceCollection.AddScoped<CorrelationIdMiddleware>();
        serviceCollection.AddScoped<FunctionTelemetryScopeMiddleware>();
        serviceCollection.AddScoped<IIntegrationEventContext, IntegrationEventContext>();
        serviceCollection.AddScoped<IntegrationEventMetadataMiddleware>();
    }

    private static void Applications(IServiceCollection services)
    {
        services.AddScoped<IBatchApplicationService, BatchApplicationService>();
        services.AddScoped<IJobRunner, DatabricksJobRunner>();
        services.AddScoped<IProcessCompletedPublisher>(provider =>
        {
            var sender = provider
                .GetRequiredService<TargetedSingleton<ServiceBusSender, ProcessCompletedPublisher>>()
                .Instance;
            var factory = provider.GetRequiredService<IServiceBusMessageFactory>();
            return new ProcessCompletedPublisher(sender, factory);
        });
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void Domains(IServiceCollection services)
    {
        services.AddScoped<IBatchRepository, BatchRepository>();
    }

    private static void Infrastructure(IServiceCollection serviceCollection)
    {
        serviceCollection.AddApplicationInsightsTelemetryWorkerService(
            EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.AppInsightsInstrumentationKey));

        serviceCollection.AddScoped<IDatabaseContext, DatabaseContext>();
        serviceCollection.AddSingleton<IJsonSerializer, JsonSerializer>();
        serviceCollection.AddScoped<IServiceBusMessageFactory, ServiceBusMessageFactory>();

        var connectionString =
            EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DatabaseConnectionString);
        serviceCollection.AddDbContext<DatabaseContext>(options =>
            options.UseSqlServer(connectionString, o => o.UseNodaTime()));

        var serviceBusConnectionString =
            EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ServiceBusSendConnectionString);
        var processCompletedTopicName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ProcessCompletedTopicName);
        serviceCollection.AddSingleton(_ => new ServiceBusClient(serviceBusConnectionString));
        serviceCollection.AddSingleton(provider =>
        {
            var client = provider.GetRequiredService<ServiceBusClient>();
            return new TargetedSingleton<ServiceBusSender, ProcessCompletedPublisher>(
                client.CreateSender(processCompletedTopicName));
        });

        serviceCollection.AddScoped<DatabricksJobSelector>();

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
        var processCompletedTopicName =
            EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ProcessCompletedTopicName);

        serviceCollection
            .AddHealthChecks()
            .AddLiveCheck()
            .AddDbContextCheck<DatabaseContext>(name: "SqlDatabaseContextCheck")
            .AddAzureServiceBusTopic(
                serviceBusConnectionString,
                processCompletedTopicName,
                name: "ProcessCompletedTopicExists")
            .AddDatabricksCheck(
                EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DatabricksWorkspaceUrl),
                EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DatabricksWorkspaceToken));
    }
}
