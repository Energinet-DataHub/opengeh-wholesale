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

namespace Energinet.DataHub.Wholesale.WebApi
{
    /// <summary>
    /// Contains names of settings used by the web api.
    /// </summary>
    public static class EnvironmentSettingNames
    {
        // Function
        public const string AzureWebJobsStorage = "AzureWebJobsStorage";

        // Environment specific settings
        public const string AppInsightsInstrumentationKey = "APPINSIGHTS_INSTRUMENTATIONKEY";
        public const string DbConnectionString = "DB_CONNECTION_STRING";

        // JWT Token auth
        public const string ExternalOpenIdUrl = "EXTERNAL_OPEN_ID_URL";
        public const string InternalOpenIdUrl = "INTERNAL_OPEN_ID_URL";
        public const string BackendAppId = "BACKEND_SERVICE_APP_ID";

        public const string CalculationStorageConnectionString = "STORAGE_CONNECTION_STRING";
        public const string CalculationStorageContainerName = "STORAGE_CONTAINER_NAME";

        public const string DateTimeZoneId = "TIME_ZONE";

        #region ServiceBus

        /// <summary>
        /// Connection string to manage the wholesale domain service bus namespace.
        /// </summary>
        public const string ServiceBusManageConnectionString = "SERVICE_BUS_MANAGE_CONNECTION_STRING";

        /// <summary>
        /// Connection string to subscribe to the wholesale domain service bus queues and topics.
        /// </summary>
        public const string ServiceBusSendConnectionString = "SERVICE_BUS_SEND_CONNECTION_STRING";

        public const string BatchCreatedEventName = "BATCH_CREATED_EVENT_NAME";

        public const string DomainEventsTopicName = "DOMAIN_EVENTS_TOPIC_NAME";

        #endregion

        public const string DatabricksWorkspaceUrl = "DATABRICKS_WORKSPACE_URL";
        public const string DatabricksWorkspaceToken = "DATABRICKS_WORKSPACE_TOKEN";
    }
}
