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

using Azure.Messaging.ServiceBus;

namespace Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;

public static class ServiceBusMessageExtensions
{
    public static void SetOperationCorrelationId(this ServiceBusMessage serviceBusMessage, string operationCorrelationId)
    {
        serviceBusMessage.ApplicationProperties.Add(MessageMetaDataConstants.OperationCorrelationId, operationCorrelationId);
    }

    public static string GetOperationCorrelationId(this ServiceBusMessage serviceBusMessage)
    {
        return serviceBusMessage.ApplicationProperties[MessageMetaDataConstants.OperationCorrelationId].ToString()!;
    }

    public static void SetMessageType(this ServiceBusMessage serviceBusMessage, string messageType)
    {
        serviceBusMessage.ApplicationProperties.Add(MessageMetaDataConstants.MessageType, messageType);
    }

    public static string GetMessageType(this ServiceBusMessage serviceBusMessage)
    {
        return serviceBusMessage.ApplicationProperties[MessageMetaDataConstants.MessageType].ToString()!;
    }
}
