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

using Azure.Storage.Blobs;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.Components;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestCommon.Fixture.Database;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.WebApi;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestCommon.Fixture.WebApi
{
    public class WholesaleWebApiFixture : WebApiFixture
    {
        public WholesaleWebApiFixture()
        {
            AzuriteManager = new AzuriteManager();
            DatabaseManager = new WholesaleDatabaseManager();
            DatabricksTestManager = new DatabricksTestManager();
            IntegrationTestConfiguration = new IntegrationTestConfiguration();

            ServiceBusResourceProvider = new ServiceBusResourceProvider(
                IntegrationTestConfiguration.ServiceBusConnectionString,
                TestLogger);
        }

        public WholesaleDatabaseManager DatabaseManager { get; }

        public DatabricksTestManager DatabricksTestManager { get; }

        private AzuriteManager AzuriteManager { get; }

        private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

        /// <inheritdoc/>
        protected override void OnConfigureEnvironment()
        {
        }

        /// <inheritdoc/>
        protected override async Task OnInitializeWebApiDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            AzuriteManager.StartAzurite();
            await DatabaseManager.CreateDatabaseAsync();

            // Overwrites the setting so the Web Api app uses the database we have control of in the test
            Environment.SetEnvironmentVariable(
                $"CONNECTIONSTRINGS:{ConfigurationSettingNames.DbConnectionString}",
                DatabaseManager.ConnectionString);

            Environment.SetEnvironmentVariable(ConfigurationSettingNames.ExternalOpenIdUrl, "disabled");
            Environment.SetEnvironmentVariable(ConfigurationSettingNames.InternalOpenIdUrl, "disabled");
            Environment.SetEnvironmentVariable(ConfigurationSettingNames.BackendAppId, "disabled");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

            Environment.SetEnvironmentVariable(ConfigurationSettingNames.DatabricksWorkspaceUrl, DatabricksTestManager.DatabricksUrl);
            Environment.SetEnvironmentVariable(ConfigurationSettingNames.DatabricksWorkspaceToken, DatabricksTestManager.DatabricksToken);

            Environment.SetEnvironmentVariable(ConfigurationSettingNames.CalculationStorageConnectionString, "UseDevelopmentStorage=true");
            Environment.SetEnvironmentVariable(ConfigurationSettingNames.CalculationStorageContainerName, "wholesale");

            await ServiceBusResourceProvider
                .BuildTopic("domain-events")
                .SetEnvironmentVariableToTopicName(ConfigurationSettingNames.DomainEventsTopicName)
                .CreateAsync();

            // Create storage container - ought to be a Data Lake file system
            var blobContainerClient = new BlobContainerClient(
                Environment.GetEnvironmentVariable(ConfigurationSettingNames.CalculationStorageConnectionString),
                Environment.GetEnvironmentVariable(ConfigurationSettingNames.CalculationStorageContainerName));

            Environment.SetEnvironmentVariable(ConfigurationSettingNames.ServiceBusSendConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(ConfigurationSettingNames.ServiceBusManageConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(ConfigurationSettingNames.BatchCreatedEventName, "batch-created");

            Environment.SetEnvironmentVariable(ConfigurationSettingNames.DateTimeZoneId, "Europe/Copenhagen");

            if (!await blobContainerClient.ExistsAsync())
                await blobContainerClient.CreateAsync();
        }

        /// <inheritdoc/>
        protected override Task OnDisposeWebApiDependenciesAsync()
        {
            AzuriteManager.Dispose();
            return DatabaseManager.DeleteDatabaseAsync();
        }
    }
}
