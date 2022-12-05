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

namespace Energinet.DataHub.Wholesale.ProcessManager;

public static class EnvironmentSettingNames
{
    public const string AzureWebJobsStorage = "AzureWebJobsStorage";
    public const string AppInsightsInstrumentationKey = "APPINSIGHTS_INSTRUMENTATIONKEY";

    public const string DatabaseConnectionString = "DB_CONNECTION_STRING";

    /// <summary>
    /// Connection string to manage the wholesale domain service bus namespace.
    /// </summary>
    public const string ServiceBusManageConnectionString = "SERVICE_BUS_MANAGE_CONNECTION_STRING";

    /// <summary>
    /// Connection string to subscribe to the wholesale domain service bus queues and topics.
    /// </summary>
    public const string ServiceBusSendConnectionString = "SERVICE_BUS_SEND_CONNECTION_STRING";

    public const string ServiceBusListenConnectionString = "SERVICE_BUS_LISTEN_CONNECTION_STRING";

    public const string DatabricksWorkspaceUrl = "DATABRICKS_WORKSPACE_URL";
    public const string DatabricksWorkspaceToken = "DATABRICKS_WORKSPACE_TOKEN";

    public const string CalculationStorageConnectionString = "STORAGE_CONNECTION_STRING";
    public const string CalculationStorageContainerName = "STORAGE_CONTAINER_NAME";

    public const string DomainEventsTopicName = "DOMAIN_EVENTS_TOPIC_NAME";
    public const string ZipBasisDataWhenCompletedBatchSubscriptionName = "ZIP_BASIS_DATA_WHEN_COMPLETED_BATCH_SUBSCRIPTION_NAME";
    public const string PublishProcessesCompletedWhenCompletedBatchSubscriptionName = "PUBLISH_PROCESSES_COMPLETED_WHEN_COMPLETED_BATCH_SUBSCRIPTION_NAME";

    public const string BatchCompletedEventName = "BATCH_COMPLETED_EVENT_NAME";
    public const string ProcessCompletedEventName = "PROCESS_COMPLETED_EVENT_NAME";
}
