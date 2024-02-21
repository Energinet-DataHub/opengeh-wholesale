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
using Energinet.DataHub.Core.Databricks.Jobs.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Configuration.Options;
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

            AzuriteManager = new AzuriteManager();
            DatabaseManager = new WholesaleDatabaseManager<DatabaseContext>();
            IntegrationTestConfiguration = new IntegrationTestConfiguration();

            HostConfigurationBuilder = new FunctionAppHostConfigurationBuilder();
        }

        public ITestDiagnosticsLogger TestLogger { get; }

        [NotNull]
        public FunctionAppHostManager? AppHostManager { get; private set; }

        private AzuriteManager AzuriteManager { get; }

        private WholesaleDatabaseManager<DatabaseContext> DatabaseManager { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

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
            appHostSettings.ProcessEnvironmentVariables.Add("AzureWebJobsStorage", "UseDevelopmentStorage=true");
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

            return appHostSettings;
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
