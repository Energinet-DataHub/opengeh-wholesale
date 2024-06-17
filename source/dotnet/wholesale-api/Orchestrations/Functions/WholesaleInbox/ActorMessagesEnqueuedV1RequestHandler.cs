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
using Energinet.DataHub.Wholesale.Events.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.WholesaleInbox;

public class ActorMessagesEnqueuedV1RequestHandler(
    ILogger<ActorMessagesEnqueuedV1RequestHandler> logger,
    DurableTaskClientAccessor durableTaskClientAccessor) : IWholesaleInboxRequestHandler
{
    private readonly ILogger<ActorMessagesEnqueuedV1RequestHandler> _logger = logger;
    private readonly DurableTaskClientAccessor _durableTaskClientAccessor = durableTaskClientAccessor;

    public bool CanHandle(string requestSubject) => requestSubject.Equals(ActorMessagesEnqueuedV1.EventName);

    public async Task ProcessAsync(ServiceBusReceivedMessage receivedMessage, string referenceId, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling ActorMessagesEnqueued event with message id: {MessageId}, subject: {Subject}, reference id: {ReferenceId}",
            receivedMessage.MessageId,
            receivedMessage.Subject,
            referenceId);

        var messageEnqueuedEvent = ActorMessagesEnqueuedV1.Parser.ParseFrom(receivedMessage.Body);

        _logger.LogInformation(
            "Raising event \"{OrchestrationEventName}\" to orchestration with OrchestrationInstanceId: {OrchestrationInstanceId}, CalculationId: {CalculationId}, ServiceBusMessageId: {ServiceBusMessageId}",
            ActorMessagesEnqueuedV1.EventName,
            messageEnqueuedEvent.OrchestrationInstanceId,
            messageEnqueuedEvent.CalculationId,
            receivedMessage.MessageId);

        try
        {
            await _durableTaskClientAccessor.Current.RaiseEventAsync(
                    messageEnqueuedEvent.OrchestrationInstanceId,
                    ActorMessagesEnqueuedV1.EventName,
                    messageEnqueuedEvent,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (RpcException exception)
        {
            // Exception thrown if OrchestrationInstanceId couldn't be found:
            //      Grpc.Core.RpcException: Status(StatusCode="Unknown", Detail="Exception was thrown by handler.")
            // This can happen if the orchestration has finished and a duplicate ActorMessagesEnqueued event is received
            // or if the orchestration has timed out and the ActorMessagesEnqueued is received afterwards
            _logger.LogWarning(
                exception,
                "An error occured when raising event to orchestrator for OrchestrationInstanceId: {OrchestrationInstanceId}, CalculationId: {CalculationId}, ServiceBusMessageId: {ServiceBusMessageId}",
                messageEnqueuedEvent.OrchestrationInstanceId,
                messageEnqueuedEvent.CalculationId,
                receivedMessage.MessageId);
        }
    }
}
