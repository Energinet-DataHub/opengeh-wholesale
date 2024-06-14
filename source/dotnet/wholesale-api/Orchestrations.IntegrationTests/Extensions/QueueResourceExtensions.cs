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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.EnergySupplying.RequestResponse.InboxEvents;
using Google.Protobuf;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions;

public static class QueueResourceExtensions
{
    public static Task SendActorMessagesEnqueuedAsync(
        this QueueResource queue,
        Guid calculationId,
        string orchestrationId,
        bool success = true)
    {
        var actorMessagesEnqueued = new ActorMessagesEnqueuedV1
        {
            CalculationId = calculationId.ToString(),
            OrchestrationInstanceId = orchestrationId,
            Success = success,
        };

        return SendMessageAsync(
            queue,
            ActorMessagesEnqueuedV1.EventName,
            actorMessagesEnqueued,
            calculationId.ToString());
    }

    private static async Task SendMessageAsync(QueueResource queue, string subject, IMessage? body, string referenceId)
    {
        var serviceBusMessage = new ServiceBusMessage
        {
            Subject = subject,
        };

        serviceBusMessage.ApplicationProperties.Add("ReferenceId", referenceId);

        if (body is not null)
            serviceBusMessage.Body = new BinaryData(body.ToByteArray());

        await queue.SenderClient.SendMessageAsync(serviceBusMessage);
    }
}
