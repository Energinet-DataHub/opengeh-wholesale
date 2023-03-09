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

using System.Threading.Tasks;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox;
using Processing.Infrastructure.Configuration.EventPublishing.AzureServiceBus;
using Processing.Infrastructure.Configuration.EventPublishing.Protobuf;
using Processing.Infrastructure.Configuration.Outbox;

namespace Processing.Infrastructure.Configuration.EventPublishing
{
    public class EventDispatcher
    {
        private readonly OutboxMessageRepository _outboxProvider;
        private readonly IServiceBusMessageDispatcher _messageDispatcher;
        private readonly MessageParser _messageParser;

        public EventDispatcher(Outbox.OutboxProvider outboxProvider, ServiceBusMessageDispatcher messageDispatcher, MessageParser messageParser)
        {
            _outboxProvider = outboxProvider;
            _messageDispatcher = messageDispatcher;
            _messageParser = messageParser;
        }

        public async Task DispatchAsync()
        {
            OutboxMessage? message;
            while ((message = _outboxProvider.GetNext(OutboxMessageCategory.IntegrationEvent)) != null)
            {
                var integrationEvent = _messageParser.GetFrom(message.Type, message.Data);

                await _messageDispatcher.DispatchAsync(integrationEvent).ConfigureAwait(false);
                await _outboxProvider.MarkProcessedAsync(message).ConfigureAwait(false);
            }
        }
    }
}
