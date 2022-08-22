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

using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestCommon;

public static class ServiceBusTestMessage
{
    public static ServiceBusMessage Create(
        byte[] data,
        DateTime operationTimestamp,
        string operationCorrelationId,
        string messageType)
    {
        var serviceBusMessage = new ServiceBusMessage(data);
        Configure(serviceBusMessage, operationTimestamp, operationCorrelationId, messageType);
        return serviceBusMessage;
    }

    public static ServiceBusMessage Create<TData>(
        TData data,
        DateTime operationTimestamp,
        string operationCorrelationId,
        string messageType)
    {
        var body = JsonSerializer.Serialize(data);
        var serviceBusMessage = new ServiceBusMessage(body);
        Configure(serviceBusMessage, operationTimestamp, operationCorrelationId, messageType);
        return serviceBusMessage;
    }

    private static void Configure(
        ServiceBusMessage serviceBusMessage,
        DateTime operationTimestamp,
        string operationCorrelationId,
        string messageType)
    {
        serviceBusMessage.CorrelationId = Guid.NewGuid().ToString().Replace("-", string.Empty);
        serviceBusMessage.ApplicationProperties.Add("OperationTimestamp", operationTimestamp.ToUniversalTime());
        serviceBusMessage.ApplicationProperties.Add("OperationCorrelationId", operationCorrelationId);
        serviceBusMessage.ApplicationProperties.Add("MessageVersion", 1);
        serviceBusMessage.ApplicationProperties.Add("MessageType", messageType);
        serviceBusMessage.ApplicationProperties.Add("EventIdentification", "2542ed0d242e46b68b8b803e93ffbf7b");
    }
}
