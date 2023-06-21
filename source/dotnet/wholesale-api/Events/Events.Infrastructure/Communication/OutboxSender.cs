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
using Energinet.DataHub.Wholesale.Events.Infrastructure.ServiceBus;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.Communication;

public class OutboxSender<TOutboxRepository> : BackgroundService
    where TOutboxRepository : IOutboxRepository
{
    private readonly TOutboxRepository _outboxRepository;

    public OutboxSender(TOutboxRepository outboxRepository)
    {
        _outboxRepository = outboxRepository;
    }

    // Operation time stamp (hændelsen): when the event was completed (in business process)
    // Send time stamp (afsendelsen): when the event was actually sent on the bus
    // Subject/message name can be used in filters i.e. <prefix-domain>MeteringPointCreated
    // Note naming? MessageName becomes subject, how do I know that?
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        await foreach (var events in _outboxRepository.GetAsync(token))
        {
            foreach (var @event in events)
            {
                var serviceBusMessage = new ServiceBusMessage
                {
                    Body = new BinaryData(@event.Message),
                    Subject = @event.MessageName,
                    MessageId = @event.EventIdentification.ToString(),
                };
                serviceBusMessage.ApplicationProperties.Add("OperationTimeStamp", @event.OperationTimeStamp);
                serviceBusMessage.ApplicationProperties.Add("MessageVersion", @event.MessageVersion);
            }
        }
    }
}
