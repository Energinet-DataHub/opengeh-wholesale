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

using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Authorization;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.Database;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.WebApi;
using Energinet.DataHub.Wholesale.WebApi;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.WebApi
{
    public class WholesaleWebApiFixture : WebApiFixture
    {
        public AuthorizationConfiguration AuthorizationConfiguration { get; }

        public WholesaleWebApiFixture()
        {
            DatabaseManager = new WholesaleDatabaseManager();
            AuthorizationConfiguration = new AuthorizationConfiguration(
                "u002",
                "integrationtest.local.settings.json",
                "AZURE_SECRETS_KEYVAULT_URL");
        }

        public WholesaleDatabaseManager DatabaseManager { get; }

        /// <inheritdoc/>
        protected override void OnConfigureEnvironment()
        {
        }

        /// <inheritdoc/>
        protected override async Task OnInitializeWebApiDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            await DatabaseManager.CreateDatabaseAsync();

            // Overwrites the setting so the Web Api app uses the database we have control of in the test
            Environment.SetEnvironmentVariable(
                $"CONNECTIONSTRINGS:{EnvironmentSettingNames.DbConnectionString}",
                DatabaseManager.ConnectionString);

            Environment.SetEnvironmentVariable(EnvironmentSettingNames.FrontEndOpenIdUrl, AuthorizationConfiguration.FrontendOpenIdUrl);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.FrontEndServiceAppId, AuthorizationConfiguration.FrontendAppId);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

            Environment.SetEnvironmentVariable(EnvironmentSettingNames.CalculationStorageConnectionString, "UseDevelopmentStorage=true");
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.CalculationStorageContainerName, "processes");
        }

        /// <inheritdoc/>
        protected override Task OnDisposeWebApiDependenciesAsync()
        {
            return DatabaseManager.DeleteDatabaseAsync();
        }
    }
}
