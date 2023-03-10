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
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Infrastructure.EventPublishing
{
    public class OutboxMessagePublisher : IOutboxMessagePublisher
    {
        private readonly IOutboxMessageRepository _outboxMessageRepository;
        private readonly IClock _clock;
        private readonly IDatabaseContext _context;
        private readonly IIntegrationEventTopicServiceBusSender _serviceBusSender;

        public OutboxMessagePublisher(IOutboxMessageRepository outboxMessageRepository, IClock clock, IDatabaseContext context,  IIntegrationEventTopicServiceBusSender serviceBusSender)
        {
            _outboxMessageRepository = outboxMessageRepository;
            _clock = clock;
            _context = context;
            _serviceBusSender = serviceBusSender;
        }

        public async Task DispatchAsync()
        {
            var message = _outboxMessageRepository.FirstNotProcessedOrNull();

            var messageData = message?.Data;

            // TODO AJW
            // await _serviceBusSender.SendMessageAsync(messageData, CancellationToken.None).ConfigureAwait(false);
            message?.SetProcessed(_clock.GetCurrentInstant());

            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
