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

using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ListenerMock;
using Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ResourceProvider;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.Wholesale.Infrastructure.Core;
using Energinet.DataHub.Wholesale.IntegrationEventListener.Common;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Authorization;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Fixture.FunctionApp
{
    public class IntegrationEventListenerFunctionAppFixture : FunctionAppFixture
    {
        public IntegrationEventListenerFunctionAppFixture()
        {
            AzuriteManager = new AzuriteManager();
            IntegrationTestConfiguration = new IntegrationTestConfiguration();
            AuthorizationConfiguration = new AuthorizationConfiguration(
                "u002",
                "integrationtest.local.settings.json",
                "AZURE_SECRETS_KEYVAULT_URL");

            ServiceBusResourceProvider = new ServiceBusResourceProvider(
                IntegrationTestConfiguration.ServiceBusConnectionString,
                TestLogger);

            ServiceBusClient = new ServiceBusClient(
                IntegrationTestConfiguration.ServiceBusConnectionString);

            EventHubResourceProvider = new EventHubResourceProvider(
                IntegrationTestConfiguration.EventHubConnectionString,
                IntegrationTestConfiguration.ResourceManagementSettings,
                TestLogger);
        }

        public EventHubListenerMock EventHubListener { get; private set; } = null!;

        public AuthorizationConfiguration AuthorizationConfiguration { get; }

        public TopicResource IntegrationEventsTopic { get; private set; } = null!;

        public ServiceBusReceiver MeteringPointCreatedDeadLetterReceiver { get; private set; } = null!;

        public ServiceBusReceiver MeteringPointConnectedDeadLetterReceiver { get; private set; } = null!;

        public ServiceBusReceiver MarketParticipantChangedDeadLetterReceiver { get; private set; } = null!;

        public EventHubResourceProvider EventHubResourceProvider { get; }

        private AzuriteManager AzuriteManager { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

        private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

        private ServiceBusClient ServiceBusClient { get; }

        /// <inheritdoc/>
        protected override void OnConfigureHostSettings(FunctionAppHostSettings hostSettings)
        {
            ArgumentNullException.ThrowIfNull(hostSettings);

            var buildConfiguration = GetBuildConfiguration();
            hostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\IntegrationEventListener\\bin\\{buildConfiguration}\\net6.0";
        }

        /// <inheritdoc/>
        protected override void OnConfigureEnvironment()
        {
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.AppInsightsInstrumentationKey, IntegrationTestConfiguration.ApplicationInsightsInstrumentationKey);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.AzureWebJobsStorage, "UseDevelopmentStorage=true");
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.IntegrationEventConnectionListenerString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.IntegrationEventConnectionManagerString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.IntegrationEventsEventHubConnectionString, EventHubResourceProvider.ConnectionString);
        }

        /// <inheritdoc/>
        protected override async Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            AzuriteManager.StartAzurite();

            var eventHubResource = await EventHubResourceProvider
                .BuildEventHub("wholesale-event-hub")
                .SetEnvironmentVariableToEventHubName(EnvironmentSettingNames.IntegrationEventsEventHubName)
                .CreateAsync();

            EventHubListener = new EventHubListenerMock(
                EventHubResourceProvider.ConnectionString,
                eventHubResource.Name,
                "UseDevelopmentStorage=true",
                "container",
                TestLogger);

            await EventHubListener.InitializeAsync().ConfigureAwait(false);

            IntegrationEventsTopic = await ServiceBusResourceProvider
                .BuildTopic("integration-events")
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.IntegrationEventsTopicName)
                .AddSubscription("metering-point-created-to-wholesale")
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.MeteringPointCreatedSubscriptionName)
                .AddSubscription("metering-point-connected-to-wholesale")
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.MeteringPointConnectedSubscriptionName)
                .AddSubscription("market-participant-changed-to-wholesale")
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.MarketParticipantChangedSubscriptionName)
                .CreateAsync();

            MeteringPointCreatedDeadLetterReceiver = ServiceBusClient.CreateReceiver(
                    EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.IntegrationEventsTopicName),
                    EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MeteringPointCreatedSubscriptionName),
                    new ServiceBusReceiverOptions { SubQueue = SubQueue.DeadLetter });

            MeteringPointConnectedDeadLetterReceiver = ServiceBusClient.CreateReceiver(
                    EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.IntegrationEventsTopicName),
                    EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MeteringPointConnectedSubscriptionName),
                    new ServiceBusReceiverOptions { SubQueue = SubQueue.DeadLetter });

            MarketParticipantChangedDeadLetterReceiver = ServiceBusClient.CreateReceiver(
                    EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.IntegrationEventsTopicName),
                    EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.MarketParticipantChangedSubscriptionName),
                    new ServiceBusReceiverOptions { SubQueue = SubQueue.DeadLetter });
        }

        /// <inheritdoc/>
        protected override Task OnFunctionAppHostFailedAsync(IReadOnlyList<string> hostLogSnapshot, Exception exception)
        {
            if (Debugger.IsAttached)
                Debugger.Break();

            return base.OnFunctionAppHostFailedAsync(hostLogSnapshot, exception);
        }

        /// <inheritdoc/>
        protected override async Task OnDisposeFunctionAppDependenciesAsync()
        {
            AzuriteManager.Dispose();

            await ServiceBusClient.DisposeAsync();
            await ServiceBusResourceProvider.DisposeAsync();
            await EventHubResourceProvider.DisposeAsync();
        }

        private static string GetBuildConfiguration()
        {
#if DEBUG
            return "Debug";
#else
            return "Release";
#endif
        }
    }
}
