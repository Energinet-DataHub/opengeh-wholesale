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

using Energinet.DataHub.Wholesale.Domain;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox;

namespace Energinet.DataHub.Wholesale.Infrastructure.EventPublishing
{
    public class IntegrationEventPublisher : IIntegrationEventPublisher
    {
        private readonly OutboxMessageRepository _outboxProvider;
        private readonly OutboxMessageFactory _outboxMessageFactory;
        private readonly IntegrationEventMapper _integrationEventMapper;

        public IntegrationEventPublisher(OutboxMessageRepository outboxProvider, OutboxMessageFactory outboxMessageFactory, IntegrationEventMapper integrationEventMapper)
        {
            _outboxProvider = outboxProvider;
            _outboxMessageFactory = outboxMessageFactory;
            _integrationEventMapper = integrationEventMapper;
        }

        public Task PublishAsync<TEvent>(TEvent integrationEvent)
        {
            if (integrationEvent == null) throw new ArgumentNullException(nameof(integrationEvent));
            var eventMetadata = _integrationEventMapper.GetByType(integrationEvent.GetType());
            var message = integrationEvent.ToString() ?? throw new InvalidCastException("Message cannot be empty.");
            var messageType = eventMetadata.EventName;

            _outboxProvider.Add(_outboxMessageFactory.CreateFrom(message, messageType));
            return Task.CompletedTask;
        }
    }
}
