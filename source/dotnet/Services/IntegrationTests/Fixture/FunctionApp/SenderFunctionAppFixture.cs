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
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.MessageHub.IntegrationTesting;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixture.Database;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Authorization;
using Energinet.DataHub.Wholesale.Sender;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Fixture.FunctionApp
{
    public class SenderFunctionAppFixture : FunctionAppFixture
    {
        public SenderFunctionAppFixture()
        {
            AzuriteManager = new AzuriteManager();
            DatabaseManager = new WholesaleDatabaseManager();
            IntegrationTestConfiguration = new IntegrationTestConfiguration();
            AuthorizationConfiguration = new AuthorizationConfiguration(
                "u002",
                "integrationtest.local.settings.json",
                "AZURE_SECRETS_KEYVAULT_URL");

            ServiceBusResourceProvider = new ServiceBusResourceProvider(
                IntegrationTestConfiguration.ServiceBusConnectionString,
                TestLogger);
        }

        public AuthorizationConfiguration AuthorizationConfiguration { get; }

        public WholesaleDatabaseManager DatabaseManager { get; }

        public TopicResource CompletedProcessTopic { get; set; } = null!;

        public QueueResource DataAvailableQueue { get; set; } = null!;

        public QueueResource MessageHubRequestQueue { get; set; } = null!;

        public QueueResource MessageHubReplyQueue { get; set; } = null!;

        public MessageHubSimulation MessageHubMock { get; set; } = null!;

        private AzuriteManager AzuriteManager { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

        private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

        /// <inheritdoc/>
        protected override void OnConfigureHostSettings(FunctionAppHostSettings hostSettings)
        {
            ArgumentNullException.ThrowIfNull(hostSettings);

            var buildConfiguration = GetBuildConfiguration();
            hostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\Sender\\bin\\{buildConfiguration}\\net6.0";
        }

        /// <inheritdoc/>
        protected override void OnConfigureEnvironment()
        {
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.AppInsightsInstrumentationKey, IntegrationTestConfiguration.ApplicationInsightsInstrumentationKey);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DatabaseConnectionString, DatabaseManager.ConnectionString);
            Environment.SetEnvironmentVariable("AzureWebJobsStorage", "UseDevelopmentStorage=true");

            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DataHubServiceBusManageConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ServiceBusManageConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ServiceBusListenConnectionString, ServiceBusResourceProvider.ConnectionString);

            Environment.SetEnvironmentVariable(EnvironmentSettingNames.MessageHubServiceBusSendConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.MessageHubServiceBusListenConnectionString, ServiceBusResourceProvider.ConnectionString);

            Environment.SetEnvironmentVariable(EnvironmentSettingNames.CalculatorResultsConnectionString, "UseDevelopmentStorage=true");
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.CalculatorResultsFileSystemName, "processes");
        }

        /// <inheritdoc/>
        protected override async Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            AzuriteManager.StartAzurite();

            await DatabaseManager.CreateDatabaseAsync();

            CompletedProcessTopic = await ServiceBusResourceProvider
                .BuildTopic("completed-process")
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.ProcessCompletedTopicName)
                .AddSubscription("completed-process-sub-send-data-available")
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ProcessCompletedSubscriptionName)
                .CreateAsync();

            DataAvailableQueue = await ServiceBusResourceProvider
                .BuildQueue("data-available")
                .SetEnvironmentVariableToQueueName(EnvironmentSettingNames.MessageHubDataAvailableQueueName)
                .CreateAsync();

            MessageHubRequestQueue = await ServiceBusResourceProvider
                .BuildQueue("messagehub-request", requiresSession: true)
                .SetEnvironmentVariableToQueueName(EnvironmentSettingNames.MessageHubRequestQueueName)
                .CreateAsync();

            MessageHubReplyQueue = await ServiceBusResourceProvider
                .BuildQueue("messagehub-reply", requiresSession: true)
                .SetEnvironmentVariableToQueueName(EnvironmentSettingNames.MessageHubReplyQueueName)
                .CreateAsync();

            const string messageHubStorageConnectionString = "UseDevelopmentStorage=true";
            const string messageHubStorageContainerName = "messagehub-container";
            Environment.SetEnvironmentVariable(
                EnvironmentSettingNames.MessageHubStorageConnectionString,
                messageHubStorageConnectionString);
            Environment.SetEnvironmentVariable(
                EnvironmentSettingNames.MessageHubStorageContainerName,
                messageHubStorageContainerName);

            var messageHubSimulationConfig = new MessageHubSimulationConfig(
                serviceBusReadWriteConnectionString: ServiceBusResourceProvider.ConnectionString,
                dataAvailableQueueName: DataAvailableQueue.Name,
                domainQueueName: MessageHubRequestQueue.Name,
                domainReplyQueueName: MessageHubReplyQueue.Name,
                blobStorageConnectionString: messageHubStorageConnectionString,
                blobStorageContainerName: messageHubStorageContainerName);

            MessageHubMock = new MessageHubSimulation(messageHubSimulationConfig);
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
            await MessageHubMock.DisposeAsync();
            await DatabaseManager.DeleteDatabaseAsync();
            await ServiceBusResourceProvider.DisposeAsync();
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
