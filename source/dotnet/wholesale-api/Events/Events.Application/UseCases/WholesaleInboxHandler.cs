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
using Energinet.DataHub.Wholesale.Events.Interfaces;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Events.Application.UseCases;

public class WholesaleInboxHandler
{
    private readonly ILogger<WholesaleInboxHandler> _logger;
    private readonly IEnumerable<IWholesaleInboxRequestHandler> _requestHandlers;

    public WholesaleInboxHandler(
        ILogger<WholesaleInboxHandler> logger,
        IEnumerable<IWholesaleInboxRequestHandler> requestHandlers)
    {
        _logger = logger;
        _requestHandlers = requestHandlers;
    }

    public async Task ProcessAsync(ServiceBusReceivedMessage receivedMessage, CancellationToken cancellationToken)
    {
        receivedMessage.ApplicationProperties.TryGetValue("ReferenceId", out var referenceIdObject);

        if (referenceIdObject is not string referenceId)
        {
            _logger.LogError(
                "Missing reference id for Wholesale inbox service bus message (reference id: {reference_id}, subject: {subject}, message id: {message_id})",
                referenceIdObject ?? "null",
                receivedMessage.MessageId,
                receivedMessage.Subject);
            throw new InvalidOperationException("Missing reference id for received Wholesale inbox service bus message");
        }

        _logger.LogInformation(
            "Processing Wholesale inbox message (reference id: {reference_id}, subject: {subject}, message id: {message_id})",
            referenceId,
            receivedMessage.Subject,
            receivedMessage.MessageId);

        if (string.IsNullOrEmpty(receivedMessage.Subject))
            throw new InvalidOperationException("Missing subject for received Wholesale inbox service bus message");

        var requestHandler = _requestHandlers.SingleOrDefault(h => h.CanHandle(receivedMessage.Subject)) ??
                             throw new InvalidOperationException(
                                 $"No request handler found for Wholesale inbox message with subject \"{receivedMessage.Subject}\"");

        await requestHandler.ProcessAsync(receivedMessage, referenceId, cancellationToken).ConfigureAwait(true);

        _logger.LogInformation(
            "Finished processing Wholesale inbox message (reference id: {reference_id}, subject: {subject}, message id: {message_id}) from queue",
            referenceId,
            receivedMessage.Subject,
            receivedMessage.MessageId);
    }
}
