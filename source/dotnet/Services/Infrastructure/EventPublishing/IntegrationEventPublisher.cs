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

using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Domain;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Infrastructure.EventPublishing
{
    public class IntegrationEventPublisher : IIntegrationEventPublisher
    {
        private readonly IIntegrationEventTopicServiceBusSender _integrationEventTopicServiceBusSender;
        private readonly IServiceBusMessageFactory _serviceBusMessageFactory;
        private readonly IOutboxMessageRepository _outboxMessageRepository;
        private readonly IClock _clock;
        private readonly IDatabaseContext _context;

        public IntegrationEventPublisher(
            IIntegrationEventTopicServiceBusSender integrationEventTopicServiceBusSender,
            IServiceBusMessageFactory serviceBusMessageFactory,
            IOutboxMessageRepository outboxMessageRepository,
            IClock clock,
            IDatabaseContext context)
        {
            _integrationEventTopicServiceBusSender = integrationEventTopicServiceBusSender;
            _serviceBusMessageFactory = serviceBusMessageFactory;
            _outboxMessageRepository = outboxMessageRepository;
            _clock = clock;
            _context = context;
        }

        public async Task PublishIntegrationEventsAsync(CancellationToken token)
        {
            // TODO AJH how many to get? try catch? How often? Retry?
            var outboxMessages = await _outboxMessageRepository.GetAllAsync(token).ConfigureAwait(false);
            foreach (var outboxMessage in outboxMessages)
            {
                outboxMessage.SetProcessed(_clock.GetCurrentInstant());

                var serviceBusMessage = _serviceBusMessageFactory.CreateProcessCompleted(outboxMessage.Data, outboxMessage.Type);

                await _integrationEventTopicServiceBusSender.SendMessageAsync(serviceBusMessage, token).ConfigureAwait(false);

                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
