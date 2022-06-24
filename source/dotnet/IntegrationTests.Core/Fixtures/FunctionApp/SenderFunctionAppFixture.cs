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
using Energinet.DataHub.Wholesale.IntegrationTests.Core.TestCommon.Authorization;
using Microsoft.Extensions.Configuration;
using HostSettings = Energinet.DataHub.Wholesale.Sender.Configuration.Settings;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Core.Fixtures.FunctionApp
{
    public class SenderFunctionAppFixture : FunctionAppFixture
    {
        public SenderFunctionAppFixture()
        {
            AzuriteManager = new AzuriteManager();
            IntegrationTestConfiguration = new IntegrationTestConfiguration();
            AuthorizationConfiguration = new AuthorizationConfiguration(
                "u002",
                "integrationtest.local.settings.json",
                "AZURE_SECRETS_KEYVAULT_URL");
        }

        public AuthorizationConfiguration AuthorizationConfiguration { get; }

        private AzuriteManager AzuriteManager { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

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
            Environment.SetEnvironmentVariable(HostSettings.AppInsightsInstrumentationKey.Key, IntegrationTestConfiguration.ApplicationInsightsInstrumentationKey);
            Environment.SetEnvironmentVariable(HostSettings.AzureWebJobsStorage.Key, "UseDevelopmentStorage=true");
        }

        /// <inheritdoc/>
        protected override Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            AzuriteManager.StartAzurite();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override Task OnFunctionAppHostFailedAsync(IReadOnlyList<string> hostLogSnapshot, Exception exception)
        {
            if (Debugger.IsAttached)
                Debugger.Break();

            return base.OnFunctionAppHostFailedAsync(hostLogSnapshot, exception);
        }

        /// <inheritdoc/>
        protected override Task OnDisposeFunctionAppDependenciesAsync()
        {
            AzuriteManager.Dispose();
            return Task.CompletedTask;
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
