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
using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Azure.Storage.Files.DataLake;
using Energinet.DataHub.Core.Databricks.Jobs.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Calculations.IntegrationTests.Fixture.Database;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.Orchestration.IntegrationTests.Fixtures
{
    /// <summary>
    /// Support testing Orchestration app.
    /// </summary>
    public class OrchestrationAppFixture : IAsyncLifetime
    {
        public OrchestrationAppFixture()
        {
            TestLogger = new TestDiagnosticsLogger();
            IntegrationTestConfiguration = new IntegrationTestConfiguration();

            AzuriteManager = new AzuriteManager(useOAuth: true);
            DatabaseManager = new WholesaleDatabaseManager<DatabaseContext>();

            ServiceBusResourceProvider = new ServiceBusResourceProvider(
                IntegrationTestConfiguration.ServiceBusConnectionString,
                TestLogger);

            HostConfigurationBuilder = new FunctionAppHostConfigurationBuilder();
        }

        public ITestDiagnosticsLogger TestLogger { get; }

        [NotNull]
        public FunctionAppHostManager? AppHostManager { get; private set; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

        private AzuriteManager AzuriteManager { get; }

        private WholesaleDatabaseManager<DatabaseContext> DatabaseManager { get; }

        private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

        private FunctionAppHostConfigurationBuilder HostConfigurationBuilder { get; }

        public async Task InitializeAsync()
        {
            // Storage emulator
            AzuriteManager.StartAzurite();

            // Database
            await DatabaseManager.CreateDatabaseAsync();

            // Prepare host settings
            var port = 8000;
            var appHostSettings = CreateAppHostSettings("Orchestration", ref port);

            // ServiceBus entities
            await ServiceBusResourceProvider
                .BuildTopic("integration-events")
                    .Do(topic => appHostSettings.ProcessEnvironmentVariables
                        .Add(nameof(ServiceBusOptions.INTEGRATIONEVENTS_TOPIC_NAME), topic.Name))
                .AddSubscription("subscription")
                    .Do(subscription => appHostSettings.ProcessEnvironmentVariables
                        .Add(nameof(ServiceBusOptions.INTEGRATIONEVENTS_SUBSCRIPTION_NAME), subscription.SubscriptionName))
                .CreateAsync();

            // DataLake
            await EnsureCalculationStorageContainerExistsAsync();

            // Create and start host
            AppHostManager = new FunctionAppHostManager(appHostSettings, TestLogger);
            StartHost(AppHostManager);
        }

        public async Task DisposeAsync()
        {
            AppHostManager.Dispose();
            AzuriteManager.Dispose();
            await DatabaseManager.DeleteDatabaseAsync();
        }

        /// <summary>
        /// Use this method to attach <paramref name="testOutputHelper"/> to the host logging pipeline.
        /// While attached, any entries written to host log pipeline will also be logged to xUnit test output.
        /// It is important that it is only attached while a test i active. Hence, it should be attached in
        /// the test class constructor; and detached in the test class Dispose method (using 'null').
        /// </summary>
        /// <param name="testOutputHelper">If a xUnit test is active, this should be the instance of xUnit's <see cref="ITestOutputHelper"/>; otherwise it should be 'null'.</param>
        public void SetTestOutputHelper(ITestOutputHelper testOutputHelper)
        {
            TestLogger.TestOutputHelper = testOutputHelper;
        }

        private FunctionAppHostSettings CreateAppHostSettings(string csprojName, ref int port)
        {
            var buildConfiguration = GetBuildConfiguration();

            var appHostSettings = HostConfigurationBuilder.CreateFunctionAppHostSettings();
            appHostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\{csprojName}\\bin\\{buildConfiguration}\\net8.0";
            appHostSettings.Port = ++port;

            appHostSettings.ProcessEnvironmentVariables.Add("FUNCTIONS_WORKER_RUNTIME", "dotnet-isolated");
            appHostSettings.ProcessEnvironmentVariables.Add("AzureWebJobsStorage", AzuriteManager.FullConnectionString);
            appHostSettings.ProcessEnvironmentVariables.Add("APPLICATIONINSIGHTS_CONNECTION_STRING", IntegrationTestConfiguration.ApplicationInsightsConnectionString);

            // Time zone
            appHostSettings.ProcessEnvironmentVariables.Add(nameof(DateTimeOptions.TIME_ZONE), "Europe/Copenhagen");

            // Database
            appHostSettings.ProcessEnvironmentVariables.Add(
                $"{nameof(ConnectionStringsOptions.ConnectionStrings)}__{nameof(ConnectionStringsOptions.DB_CONNECTION_STRING)}",
                DatabaseManager.ConnectionString);

            // Databricks
            appHostSettings.ProcessEnvironmentVariables.Add(
                nameof(DatabricksJobsOptions.WorkspaceUrl),
                IntegrationTestConfiguration.DatabricksSettings.WorkspaceUrl);
            appHostSettings.ProcessEnvironmentVariables.Add(
                nameof(DatabricksJobsOptions.WorkspaceToken),
                IntegrationTestConfiguration.DatabricksSettings.WorkspaceAccessToken);
            appHostSettings.ProcessEnvironmentVariables.Add(
                nameof(DatabricksJobsOptions.WarehouseId),
                IntegrationTestConfiguration.DatabricksSettings.WarehouseId);

            // DataLake
            appHostSettings.ProcessEnvironmentVariables.Add(
                nameof(DataLakeOptions.STORAGE_ACCOUNT_URI),
                AzuriteManager.BlobStorageServiceUri.ToString());
            appHostSettings.ProcessEnvironmentVariables.Add(
                nameof(DataLakeOptions.STORAGE_CONTAINER_NAME),
                "wholesale");

            // ServiceBus connection strings
            appHostSettings.ProcessEnvironmentVariables.Add(
                nameof(ServiceBusOptions.SERVICE_BUS_SEND_CONNECTION_STRING),
                ServiceBusResourceProvider.ConnectionString);
            appHostSettings.ProcessEnvironmentVariables.Add(
                nameof(ServiceBusOptions.SERVICE_BUS_TRANCEIVER_CONNECTION_STRING),
                ServiceBusResourceProvider.ConnectionString);

            return appHostSettings;
        }

        /// <summary>
        /// Create storage container. Note: Azurite is based on the Blob Storage API, but sinceData Lake Storage Gen2 is built on top of it, we can still create the container like this
        /// </summary>
        private async Task EnsureCalculationStorageContainerExistsAsync()
        {
            var dataLakeServiceClient = new DataLakeServiceClient(
                serviceUri: AzuriteManager.BlobStorageServiceUri,
                credential: new DefaultAzureCredential());

            var fileSystemClient = dataLakeServiceClient.GetFileSystemClient("wholesale");

            await fileSystemClient.CreateIfNotExistsAsync();
        }

        private static void StartHost(FunctionAppHostManager hostManager)
        {
            IEnumerable<string> hostStartupLog;

            try
            {
                hostManager.StartHost();
            }
            catch (Exception)
            {
                // Function App Host failed during startup.
                // Exception has already been logged by host manager.
                hostStartupLog = hostManager.GetHostLogSnapshot();

                if (Debugger.IsAttached)
                    Debugger.Break();

                // Rethrow
                throw;
            }

            // Function App Host started.
            hostStartupLog = hostManager.GetHostLogSnapshot();
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
