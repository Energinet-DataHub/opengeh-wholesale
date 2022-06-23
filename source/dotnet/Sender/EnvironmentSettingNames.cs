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

namespace Energinet.DataHub.Wholesale.Sender;

public static class EnvironmentSettingNames
{
    public const string AppInsightsInstrumentationKey = "APPINSIGHTS_INSTRUMENTATIONKEY";
    public const string CompletedProcessTopicName = "COMPLETED_PROCESS_TOPIC_NAME";
    public const string CompletedProcessSubscriptionName = "COMPLETED_PROCESS_SUBSCRIPTION_NAME";
    public const string CompletedProcessServiceBusConnectionString = "COMPLETED_PROCESS_SERVICE_BUS_CONNECTION_STRING";
    public const string DataAvailableQueueName = "DATA_AVAILABLE_QUEUE_NAME";
    public const string DataAvailableServiceBusConnectionString = "DATA_AVAILABLE_SERVICE_BUS_CONNECTION_STRING";
}
