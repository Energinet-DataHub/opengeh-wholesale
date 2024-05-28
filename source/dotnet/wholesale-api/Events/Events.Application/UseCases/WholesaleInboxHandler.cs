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
using Energinet.DataHub.Wholesale.Edi;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Events.Application.UseCases;

public class WholesaleInboxHandler(
    ILogger<WholesaleInboxHandler> logger,
    IEnumerable<IWholesaleInboxRequestHandler> requestHandlers)
{
    public async Task ProcessAsync(ServiceBusReceivedMessage receivedMessage, CancellationToken cancellationToken)
    {
        if (!receivedMessage.ApplicationProperties.TryGetValue("ReferenceId", out var referenceIdValue))
        {
            logger.LogError("Missing reference id for Wholesale inbox service bus message (MessageId: {MessageId}, Subject: {Subject})", receivedMessage.MessageId, receivedMessage.Subject);
            throw new InvalidOperationException("Missing reference id for received Wholesale inbox service bus message");
        }

        logger.LogInformation("Processing Wholesale inbox message with reference id: {reference_id}, subject: {subject}, message id: {message_id}.", referenceIdValue, receivedMessage.Subject, receivedMessage.MessageId);

        var referenceId = referenceIdValue.ToString() ?? throw new InvalidOperationException("Cannot get referenceId as string");

        if (string.IsNullOrEmpty(receivedMessage.Subject))
            throw new InvalidOperationException("Missing subject for received Wholesale inbox service bus message");

        var requestHandler = requestHandlers.SingleOrDefault(h => h.CanHandle(receivedMessage.Subject)) ??
                             throw new InvalidOperationException(
                                 $"No request handler found for Wholesale inbox message with subject \"{receivedMessage.Subject}\"");

        await requestHandler.ProcessAsync(receivedMessage, referenceId, cancellationToken).ConfigureAwait(true);
    }
}
