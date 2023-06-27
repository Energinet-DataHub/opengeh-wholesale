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
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Core.Messaging.Communication;

/// <summary>
/// The sender runs as a background service
/// </summary>
public class OutboxSender : IOutboxSender
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly ServiceBusSender _sender;
    private readonly IServiceBusMessageFactory _serviceBusMessageFactory;
    private ILogger _logger;

    public OutboxSender(
        IOutboxRepository outboxRepository,
        ServiceBusSender sender,
        IServiceBusMessageFactory serviceBusMessageFactory,
        ILogger<OutboxSender> logger)
    {
        _outboxRepository = outboxRepository;
        _sender = sender;
        _serviceBusMessageFactory = serviceBusMessageFactory;
        _logger = logger;
    }

    // Send time stamp (afsendelsen): when the event was actually sent on the bus
    // Note naming? MessageName becomes subject, how do I know that?
    public async Task SendAsync()
    {
        var batch = await _sender.CreateMessageBatchAsync().ConfigureAwait(false);

        await foreach (var events in _outboxRepository.GetAsync())
        {
            foreach (var @event in events)
            {
                var serviceBusMessage = _serviceBusMessageFactory.Create(@event);
                if (!batch.TryAddMessage(serviceBusMessage))
                {
                    await _sender.SendMessagesAsync(batch).ConfigureAwait(false);
                    batch = await _sender.CreateMessageBatchAsync().ConfigureAwait(false);
                    if (!batch.TryAddMessage(serviceBusMessage))
                    {
                        // Here we send a single service bus message because is too large to fit in the current batch
                        await SendSingleServiceBusMessageAsync(serviceBusMessage).ConfigureAwait(false);
                    }
                }
            }
        }

        await _sender.SendMessagesAsync(batch).ConfigureAwait(false);
    }

    private async Task SendSingleServiceBusMessageAsync(ServiceBusMessage serviceBusMessage)
    {
        await _sender.SendMessageAsync(serviceBusMessage).ConfigureAwait(false);
    }
}
