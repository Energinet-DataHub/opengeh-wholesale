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
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.Communication;

public class OutboxSender<TOutboxRepository> : BackgroundService
    where TOutboxRepository : IOutboxRepository
{
    private readonly TOutboxRepository _outboxRepository;
    private readonly ServiceBusSender _sender;

    public OutboxSender(TOutboxRepository outboxRepository, ServiceBusSender sender)
    {
        _outboxRepository = outboxRepository;
        _sender = sender;
    }

    // Send time stamp (afsendelsen): when the event was actually sent on the bus
    // Note naming? MessageName becomes subject, how do I know that?
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        var batch = await _sender.CreateMessageBatchAsync(token).ConfigureAwait(false);

        await foreach (var events in _outboxRepository.GetAsync(token))
        {
            foreach (var @event in events)
            {
                var serviceBusMessage = CreateServiceBusMessage(@event);
                if (!batch.TryAddMessage(serviceBusMessage))
                {
                    await _sender.SendMessagesAsync(batch, token).ConfigureAwait(false);
                    batch = await _sender.CreateMessageBatchAsync(token).ConfigureAwait(false);
                    if (!batch.TryAddMessage(serviceBusMessage))
                    {
                        // Here we send a single service bus message because is too large to fit in the current batch
                        await SendSingleServiceBusMessageAsync(token, serviceBusMessage).ConfigureAwait(false);
                    }
                }
            }
        }

        await _sender.SendMessagesAsync(batch, token).ConfigureAwait(false);
    }

    private async Task SendSingleServiceBusMessageAsync(CancellationToken token, ServiceBusMessage serviceBusMessage)
    {
        await _sender.SendMessageAsync(serviceBusMessage, token).ConfigureAwait(false);
    }

    private static ServiceBusMessage CreateServiceBusMessage(OutboxEvent @event)
    {
        var serviceBusMessage = new ServiceBusMessage
        {
            Body = new BinaryData(@event.Message),

            // Subject (message name) can be used in filters i.e. <prefix-domain>MeteringPointCreated
            Subject = @event.MessageName,
            MessageId = @event.EventIdentification.ToString(),
        };

        // The Operation Time Stamp is when the event was actually completed (in the business process logic)
        serviceBusMessage.ApplicationProperties.Add("OperationTimeStamp", @event.OperationTimeStamp);
        serviceBusMessage.ApplicationProperties.Add("MessageVersion", @event.MessageVersion);
        return serviceBusMessage;
    }
}
