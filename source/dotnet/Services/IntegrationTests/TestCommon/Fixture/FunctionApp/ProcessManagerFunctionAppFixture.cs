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
using Azure.Storage.Blobs;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.Wholesale.IntegrationTests.Components;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Authorization;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.Database;
using Energinet.DataHub.Wholesale.ProcessManager;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.FunctionApp
{
    public class ProcessManagerFunctionAppFixture : FunctionAppFixture
    {
        public ProcessManagerFunctionAppFixture()
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

            DatabricksTestManager = new DatabricksTestManager();
        }

        public DatabricksTestManager DatabricksTestManager { get; }

        public WholesaleDatabaseManager DatabaseManager { get; }

        public AuthorizationConfiguration AuthorizationConfiguration { get; }

        public TopicResource DomainEventsTopic { get; private set; } = null!;

        public ServiceBusTestListener BatchCompletedListener { get; private set; } = null!;

        public ServiceBusTestListener SendDataAvailableWhenProcessCompletedListener { get; private set; } = null!;

        private AzuriteManager AzuriteManager { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

        private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

        /// <inheritdoc/>
        protected override void OnConfigureHostSettings(FunctionAppHostSettings hostSettings)
        {
            ArgumentNullException.ThrowIfNull(hostSettings);

            var buildConfiguration = GetBuildConfiguration();
            hostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\ProcessManager\\bin\\{buildConfiguration}\\net6.0";
        }

        /// <inheritdoc/>
        protected override void OnConfigureEnvironment()
        {
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.AppInsightsInstrumentationKey, IntegrationTestConfiguration.ApplicationInsightsInstrumentationKey);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.AzureWebJobsStorage, "UseDevelopmentStorage=true");
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ServiceBusSendConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ServiceBusListenConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ServiceBusManageConnectionString, ServiceBusResourceProvider.ConnectionString);

            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DatabaseConnectionString, DatabaseManager.ConnectionString);

            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DatabricksWorkspaceUrl, DatabricksTestManager.DatabricksUrl);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DatabricksWorkspaceToken, DatabricksTestManager.DatabricksToken);

            Environment.SetEnvironmentVariable(EnvironmentSettingNames.CalculationStorageConnectionString, "UseDevelopmentStorage=true");
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.CalculationStorageContainerName, "wholesale");
        }

        /// <inheritdoc/>
        protected override async Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            AzuriteManager.StartAzurite();

            await DatabaseManager.CreateDatabaseAsync();
            DatabricksTestManager.BeginListen();

            var batchCompletedEventName = "batch-completed";
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.BatchCompletedEventName, batchCompletedEventName);

            var processCompletedEventName = "process-completed";
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ProcessCompletedEventName, processCompletedEventName);

            var batchCompletedSubscriptionName = "batch-completed";
            var sendDataAvailableWhenProcessCompletedSubscriptionName = "process-completed";

            DomainEventsTopic = await ServiceBusResourceProvider
                .BuildTopic("domain-events")
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.DomainEventsTopicName)
                .AddSubscription("zip-basis-data")
                .AddSubjectFilter(batchCompletedEventName)
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ZipBasisDataWhenCompletedBatchSubscriptionName)
                .AddSubscription("publish-process-completed")
                .AddSubjectFilter(batchCompletedEventName)
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.PublishProcessesCompletedWhenCompletedBatchSubscriptionName)
                // Subscriptions to observe side effects of the process manager
                .AddSubscription(batchCompletedSubscriptionName)
                .AddSubjectFilter(batchCompletedEventName)
                .AddSubscription(sendDataAvailableWhenProcessCompletedSubscriptionName)
                .AddSubjectFilter(processCompletedEventName)
                .CreateAsync();

            var batchCompletedListener = new ServiceBusListenerMock(ServiceBusResourceProvider.ConnectionString, TestLogger);
            await batchCompletedListener.AddTopicSubscriptionListenerAsync(DomainEventsTopic.Name, batchCompletedSubscriptionName);
            BatchCompletedListener = new ServiceBusTestListener(batchCompletedListener);

            var sendDataAvailableWhenProcessCompletedListener = new ServiceBusListenerMock(ServiceBusResourceProvider.ConnectionString, TestLogger);
            await sendDataAvailableWhenProcessCompletedListener.AddTopicSubscriptionListenerAsync(DomainEventsTopic.Name, sendDataAvailableWhenProcessCompletedSubscriptionName);
            SendDataAvailableWhenProcessCompletedListener = new ServiceBusTestListener(sendDataAvailableWhenProcessCompletedListener);

            // Create storage container - ought to be a Data Lake file system
            var blobContainerClient = new BlobContainerClient(
                Environment.GetEnvironmentVariable(EnvironmentSettingNames.CalculationStorageConnectionString),
                Environment.GetEnvironmentVariable(EnvironmentSettingNames.CalculationStorageContainerName));

            if (!await blobContainerClient.ExistsAsync())
                await blobContainerClient.CreateAsync();
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

            await ServiceBusResourceProvider.DisposeAsync();
            await DatabaseManager.DeleteDatabaseAsync();
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
