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
using Energinet.DataHub.EnergySupplying.RequestResponse.IntegrationEvents;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.Options;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation;

internal class ActorMessagesEnqueuedTrigger(ILogger<ActorMessagesEnqueuedTrigger> logger)
{
    [Function(nameof(ActorMessagesEnqueuedTrigger))]
    public async Task ReceiveActorMessagesEnqueuedEvent(
        [ServiceBusTrigger(
            $"%{WholesaleInboxQueueOptions.SectionName}:{nameof(WholesaleInboxQueueOptions.QueueName)}%",
            Connection = $"{ServiceBusNamespaceOptions.SectionName}:{nameof(ServiceBusNamespaceOptions.ConnectionString)}")]
        ServiceBusReceivedMessage serviceBusReceivedMessage,
        [DurableClient] DurableTaskClient durableTaskClient)
    {
        logger.LogInformation(
            "Received ActorMessagesEnqueued event with MessageId: {EventId}, Subject: {Subject} from Service Bus",
            serviceBusReceivedMessage.MessageId,
            serviceBusReceivedMessage.Subject);

        var messageEnqueuedEvent = ParseServiceBusMessage(
            serviceBusReceivedMessage.Subject,
            serviceBusReceivedMessage.Body);

        logger.LogInformation(
            "Raising event \"{OrchestrationEventName}\" to orchestration with OrchestrationInstanceId: {OrchestrationInstanceId}, CalculationId: {CalculationId}, ServiceBusMessageId: {ServiceBusMessageId}",
            messageEnqueuedEvent.EventName,
            messageEnqueuedEvent.OrchestrationInstanceId,
            messageEnqueuedEvent.CalculationId,
            serviceBusReceivedMessage.MessageId);

        await durableTaskClient.RaiseEventAsync(
                messageEnqueuedEvent.OrchestrationInstanceId,
                messageEnqueuedEvent.EventName,
                messageEnqueuedEvent)
            .ConfigureAwait(false);
    }

    private (string OrchestrationInstanceId, string EventName, string CalculationId, object Event) ParseServiceBusMessage(string subject, BinaryData body)
    {
        var eventName = subject;

        switch (eventName)
        {
            case MessagesEnqueuedV1.EventName:
                var parsed = MessagesEnqueuedV1.Parser.ParseFrom(body);
                return (parsed.OrchestrationInstanceId, MessagesEnqueuedV1.EventName, parsed.CalculationId, parsed);
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(eventName),
                    eventName,
                    $"Cannot parse service bus message to an ActorMessagesEnqueued event");
        }
    }
}
