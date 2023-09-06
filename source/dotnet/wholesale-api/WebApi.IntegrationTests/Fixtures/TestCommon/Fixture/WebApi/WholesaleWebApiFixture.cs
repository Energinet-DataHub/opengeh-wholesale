﻿// Copyright 2020 Energinet DataHub A/S
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

using Azure.Identity;
using Azure.Storage.Files.DataLake;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Energinet.DataHub.Wholesale.Events.Application.Options;
using Energinet.DataHub.Wholesale.WebApi.Configuration.Options;
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
            AzuriteManager = new AzuriteManager(useOAuth: true);
            DatabaseManager = new WholesaleDatabaseManager();
            DatabricksTestManager = new DatabricksTestManager();
            IntegrationTestConfiguration = new IntegrationTestConfiguration();

            DatabricksTestManager.DatabricksUrl = IntegrationTestConfiguration.DatabricksSettings.WorkspaceUrl;
            DatabricksTestManager.DatabricksToken = IntegrationTestConfiguration.DatabricksSettings.WorkspaceAccessToken;

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
                $"{nameof(ConnectionStringsOptions.ConnectionStrings)}__{nameof(ConnectionStringsOptions.DB_CONNECTION_STRING)}",
                DatabaseManager.ConnectionString);

            Environment.SetEnvironmentVariable(nameof(JwtOptions.EXTERNAL_OPEN_ID_URL), "disabled");
            Environment.SetEnvironmentVariable(nameof(JwtOptions.INTERNAL_OPEN_ID_URL), "disabled");
            Environment.SetEnvironmentVariable(nameof(JwtOptions.BACKEND_BFF_APP_ID), "disabled");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

            Environment.SetEnvironmentVariable(nameof(DatabricksOptions.DATABRICKS_WORKSPACE_URL), DatabricksTestManager.DatabricksUrl);
            Environment.SetEnvironmentVariable(nameof(DatabricksOptions.DATABRICKS_WORKSPACE_TOKEN), DatabricksTestManager.DatabricksToken);
            Environment.SetEnvironmentVariable(nameof(DataLakeOptions.STORAGE_ACCOUNT_URI), AzuriteManager.BlobStorageServiceUri.ToString());
            Environment.SetEnvironmentVariable(nameof(DataLakeOptions.STORAGE_CONTAINER_NAME), "wholesale");

            await ServiceBusResourceProvider
                .BuildTopic("integration-events")
                .SetEnvironmentVariableToTopicName(nameof(ServiceBusOptions.INTEGRATIONEVENTS_TOPIC_NAME))
                .CreateAsync();

            Environment.SetEnvironmentVariable(nameof(ServiceBusOptions.SERVICE_BUS_SEND_CONNECTION_STRING), ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(nameof(ServiceBusOptions.SERVICE_BUS_MANAGE_CONNECTION_STRING), ServiceBusResourceProvider.ConnectionString);

            // Add events configuration variables
            Environment.SetEnvironmentVariable(nameof(Energinet.DataHub.Wholesale.Events.Application.Options.ServiceBusOptions.SERVICE_BUS_MANAGE_CONNECTION_STRING), ServiceBusResourceProvider.ConnectionString);
            await ServiceBusResourceProvider
                .BuildQueue("sbq-wholesale-inbox")
                .SetEnvironmentVariableToQueueName(nameof(Energinet.DataHub.Wholesale.Events.Application.Options.ServiceBusOptions.WHOLESALE_INBOX_MESSAGE_QUEUE_NAME))
                .CreateAsync();

            await ServiceBusResourceProvider
                .BuildQueue("sbq-edi-inbox")
                .SetEnvironmentVariableToQueueName(nameof(EdiInboxOptions.EDI_INBOX_MESSAGE_QUEUE_NAME))
                .CreateAsync();
            Environment.SetEnvironmentVariable(nameof(EdiInboxOptions.EDI_INBOX_MESSAGE_QUEUE_NAME), ServiceBusResourceProvider.ConnectionString);

            Environment.SetEnvironmentVariable(nameof(DateTimeOptions.TIME_ZONE), "Europe/Copenhagen");

            await EnsureCalculationStorageContainerExistsAsync();
        }

        /// <inheritdoc/>
        protected override Task OnDisposeWebApiDependenciesAsync()
        {
            AzuriteManager.Dispose();
            return DatabaseManager.DeleteDatabaseAsync();
        }

        /// <summary>
        /// Create storage container. Note: Azurite is based on the Blob Storage API, but sinceData Lake Storage Gen2 is built on top of it, we can still create the container like this
        /// </summary>
        private async Task EnsureCalculationStorageContainerExistsAsync()
        {
            var dataLakeServiceClient = new DataLakeServiceClient(
                serviceUri: AzuriteManager.BlobStorageServiceUri,
                credential: new DefaultAzureCredential());

            var fileSystemClient = dataLakeServiceClient.GetFileSystemClient(
                Environment.GetEnvironmentVariable("STORAGE_CONTAINER_NAME"));

            await fileSystemClient.CreateIfNotExistsAsync();
        }
    }
}
