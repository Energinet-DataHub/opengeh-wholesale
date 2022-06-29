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

namespace Energinet.DataHub.Wholesale.Sender.Configuration;

public static class EnvironmentSettingNames
{
    public const string AppInsightsInstrumentationKey = "APPINSIGHTS_INSTRUMENTATIONKEY";

    public const string DatabaseConnectionString = "DB_CONNECTION_STRING";

    /// <summary>
    /// Used for health check of all inter domain service bus connections (integration events and Message Hub)
    /// </summary>
    public const string DataHubServiceBusManageConnectionString = "INTEGRATIONEVENT_MANAGER_CONNECTION_STRING";

    public const string ServiceBusManageConnectionString = "SERVICE_BUS_MANAGE_CONNECTION_STRING";
    public const string ServiceBusListenConnectionString = "SERVICE_BUS_LISTEN_CONNECTION_STRING";
    public const string ProcessCompletedTopicName = "PROCESS_COMPLETED_TOPIC_NAME";
    public const string ProcessCompletedSubscriptionName = "PROCESS_COMPLETED_SUBSCRIPTION_NAME";

    public const string MessageHubServiceBusConnectionString = "MESSAGE_HUB_SERVICE_BUS_CONNECTION_STRING";
    public const string MessageHubDataAvailableQueueName = "MESSAGE_HUB_DATA_AVAILABLE_QUEUE_NAME";
    public const string MessageHubReplyQueueName = "MESSAGE_HUB_REPLY_QUEUE_NAME";
    public const string MessageHubStorageConnectionString = "MESSAGE_HUB_STORAGE_CONNECTION_STRING";
    public const string MessageHubStorageContainerName = "MESSAGE_HUB_STORAGE_CONTAINER_NAME";
}
