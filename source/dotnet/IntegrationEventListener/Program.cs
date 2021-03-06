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
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers;
using Energinet.DataHub.Wholesale.Infrastructure.Core;
using Energinet.DataHub.Wholesale.IntegrationEventListener.Common;
using Energinet.DataHub.Wholesale.IntegrationEventListener.MarketParticipant;
using Energinet.DataHub.Wholesale.IntegrationEventListener.MeteringPoints;
using Energinet.DataHub.Wholesale.IntegrationEventListener.Monitor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Wholesale.IntegrationEventListener
{
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
                .ConfigureServices(Infrastructure)
                .ConfigureServices(Host)
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

        private static void Infrastructure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddApplicationInsightsTelemetryWorkerService(
                EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.AppInsightsInstrumentationKey));
            serviceCollection.AddSingleton<IJsonSerializer, JsonSerializer>();
            serviceCollection.AddSingleton<ISharedIntegrationEventParser, SharedIntegrationEventParser>();
        }

        private static void Host(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<MeteringPointCreatedDtoFactory>();
            serviceCollection.AddScoped<MeteringPointConnectedDtoFactory>();
            serviceCollection.AddScoped<GridAreaUpdatedDtoFactory>();
        }

        private static void HealthCheck(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>();
            serviceCollection.AddScoped<HealthCheckEndpoint>();

            var integrationEventsEventHubConnectionString = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.IntegrationEventsEventHubConnectionString);
            var integrationEventsEventHubName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.IntegrationEventsEventHubName);

            var serviceBusConnectionString = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.IntegrationEventConnectionManagerString);
            var meteringPointCreatedTopicName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MeteringPointCreatedTopicName);
            var meteringPointCreatedSubscriptionName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MeteringPointCreatedSubscriptionName);
            var meteringPointConnectedTopicName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MeteringPointConnectedTopicName);
            var meteringPointConnectedSubscriptionName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MeteringPointConnectedSubscriptionName);

            var marketParticipantConnectedTopicName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MarketParticipantChangedTopicName);
            var marketParticipantConnectedSubscriptionName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MarketParticipantChangedSubscriptionName);

            serviceCollection
                .AddHealthChecks()
                .AddLiveCheck()
                .AddAzureEventHub(
                    integrationEventsEventHubConnectionString,
                    integrationEventsEventHubName,
                    name: "IntegrationEventsEventHubExists")
                .AddAzureServiceBusSubscription(
                    serviceBusConnectionString,
                    meteringPointCreatedTopicName,
                    meteringPointCreatedSubscriptionName,
                    name: "MeteringPointCreatedSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    serviceBusConnectionString,
                    meteringPointConnectedTopicName,
                    meteringPointConnectedSubscriptionName,
                    name: "MeteringPointConnectedSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    serviceBusConnectionString,
                    marketParticipantConnectedTopicName,
                    marketParticipantConnectedSubscriptionName,
                    name: "MarketParticipantUpdateSubscriptionExists");
        }
    }
}
