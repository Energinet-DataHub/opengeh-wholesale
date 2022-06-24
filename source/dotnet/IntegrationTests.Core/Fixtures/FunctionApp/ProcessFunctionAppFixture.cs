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
using Energinet.DataHub.Wholesale.Apps.Core.Configuration;
using Energinet.DataHub.Wholesale.IntegrationTests.Core.Fixtures.Database;
using Energinet.DataHub.Wholesale.IntegrationTests.Core.TestCommon.Authorization;
using Microsoft.Extensions.Configuration;
using HostSettings = Energinet.DataHub.Wholesale.ProcessManager.Configuration.Settings;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Core.Fixtures.FunctionApp
{
    public class ProcessFunctionAppFixture : FunctionAppFixture
    {
        public ProcessFunctionAppFixture()
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

        public WholesaleDatabaseManager DatabaseManager { get; }

        public AuthorizationConfiguration AuthorizationConfiguration { get; }

        public TopicResource ProcessCompletedTopic { get; private set; } = null!;

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
            Environment.SetEnvironmentVariable(HostSettings.AppInsightsInstrumentationKey.Key, IntegrationTestConfiguration.ApplicationInsightsInstrumentationKey);
            Environment.SetEnvironmentVariable(HostSettings.AzureWebJobsStorage.Key, "UseDevelopmentStorage=true");
            Environment.SetEnvironmentVariable(HostSettings.ServiceBusManageConnectionString.Key, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(Settings.DatabaseConnectionString.Key, DatabaseManager.ConnectionString);
            Environment.SetEnvironmentVariable(Settings.ServiceBusListenerConnectionString.Key, ServiceBusResourceProvider.ConnectionString);
        }

        /// <inheritdoc/>
        protected override async Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            AzuriteManager.StartAzurite();

            await DatabaseManager.CreateDatabaseAsync();

            ProcessCompletedTopic = await ServiceBusResourceProvider
                .BuildTopic("process-completed")
                .SetEnvironmentVariableToTopicName(Settings.ProcessCompletedTopicName.Key)
                .CreateAsync();
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
