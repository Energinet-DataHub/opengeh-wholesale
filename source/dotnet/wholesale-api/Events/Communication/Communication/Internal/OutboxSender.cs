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

using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Core.Messaging.Communication.Internal;

/// <summary>
/// The sender runs as a background service
/// </summary>
public class OutboxSender : IOutboxSender
{
    private readonly IIntegrationEventProvider _integrationEventProvider;
    private readonly IServiceBusSenderProvider _senderProvider;
    private readonly IServiceBusMessageFactory _serviceBusMessageFactory;
    private readonly ILogger _logger;

    public OutboxSender(
        IIntegrationEventProvider integrationEventProvider,
        IServiceBusSenderProvider senderProvider,
        IServiceBusMessageFactory serviceBusMessageFactory,
        ILogger<OutboxSender> logger)
    {
        _integrationEventProvider = integrationEventProvider;
        _senderProvider = senderProvider;
        _serviceBusMessageFactory = serviceBusMessageFactory;
        _logger = logger;
    }

    public async Task SendAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var eventCount = 0;
        var messageBatch = await _senderProvider.Instance.CreateMessageBatchAsync(cancellationToken).ConfigureAwait(false);

        await foreach (var @event in _integrationEventProvider.GetAsync().ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            eventCount++;
            var serviceBusMessage = _serviceBusMessageFactory.Create(@event);
            if (!messageBatch.TryAddMessage(serviceBusMessage))
            {
                await SendBatchAsync(messageBatch).ConfigureAwait(false);
                messageBatch = await _senderProvider.Instance.CreateMessageBatchAsync().ConfigureAwait(false);

                if (!messageBatch.TryAddMessage(serviceBusMessage))
                {
                    await SendMessageThatExceedsBatchLimitAsync(serviceBusMessage).ConfigureAwait(false);
                }
            }
        }

        try
        {
            await _senderProvider.Instance.SendMessagesAsync(messageBatch, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to send messages from outbox");
        }

        if (eventCount > 0)
            _logger.LogDebug("Sent {EventCount} integration events in {Time} ms", eventCount, stopwatch.Elapsed.TotalMilliseconds);
    }

    private async Task SendBatchAsync(ServiceBusMessageBatch batch)
    {
        await _senderProvider.Instance.SendMessagesAsync(batch).ConfigureAwait(false);
    }

    private async Task SendMessageThatExceedsBatchLimitAsync(ServiceBusMessage serviceBusMessage)
    {
        await _senderProvider.Instance.SendMessageAsync(serviceBusMessage).ConfigureAwait(false);
    }
}
