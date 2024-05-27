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
using Energinet.DataHub.EnergySupplying.RequestResponse.InboxEvents;
using Energinet.DataHub.Wholesale.Edi;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.InboxEvents.ActorMessagesEnqueuedV1;

public class ActorMessagesEnqueuedV1RequestHandler(
    IDurableClientFactory durableClientFactory,
    ILogger<ActorMessagesEnqueuedV1RequestHandler> logger)
    : IWholesaleInboxRequestHandler
{
    public bool CanHandle(string requestSubject) => requestSubject.Equals(MessagesEnqueuedV1.EventName);

    public async Task ProcessAsync(ServiceBusReceivedMessage receivedMessage, string referenceId, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Received ActorMessagesEnqueued event with MessageId: {EventId}, Subject: {Subject} from Service Bus",
            receivedMessage.MessageId,
            receivedMessage.Subject);

        var messageEnqueuedEvent = MessagesEnqueuedV1.Parser.ParseFrom(receivedMessage.Body);

        logger.LogInformation(
            "Raising event \"{OrchestrationEventName}\" to orchestration with OrchestrationInstanceId: {OrchestrationInstanceId}, CalculationId: {CalculationId}, ServiceBusMessageId: {ServiceBusMessageId}",
            MessagesEnqueuedV1.EventName,
            messageEnqueuedEvent.OrchestrationInstanceId,
            messageEnqueuedEvent.CalculationId,
            receivedMessage.MessageId);

        var durableTaskClient = durableClientFactory.CreateClient(new DurableClientOptions
        {
            TaskHub = "TASKHUBNAME", // TODO: Get task hub name from options pattern in configuration
            ConnectionName = "CONNECTIONNAME", // TODO: Get connection name from options pattern in configuration
        });

        await durableTaskClient.RaiseEventAsync(
                messageEnqueuedEvent.OrchestrationInstanceId,
                MessagesEnqueuedV1.EventName,
                messageEnqueuedEvent)
            .ConfigureAwait(false);
    }
}
