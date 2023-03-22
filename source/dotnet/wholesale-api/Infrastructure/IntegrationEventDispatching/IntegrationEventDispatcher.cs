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
using Energinet.DataHub.Wholesale.Infrastructure.EventPublishers;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Infrastructure.IntegrationEventDispatching
{
    public class IntegrationEventDispatcher : IIntegrationEventDispatcher
    {
        private readonly IIntegrationEventTopicServiceBusSender _integrationEventTopicServiceBusSender;
        private readonly IOutboxMessageRepository _outboxMessageRepository;
        private readonly IClock _clock;
        private readonly ILogger<IntegrationEventDispatcher> _logger;
        private readonly IServiceBusMessageFactory _serviceBusMessageFactory;

        public IntegrationEventDispatcher(
            IIntegrationEventTopicServiceBusSender integrationEventTopicServiceBusSender,
            IOutboxMessageRepository outboxMessageRepository,
            IClock clock,
            ILogger<IntegrationEventDispatcher> logger,
            IServiceBusMessageFactory serviceBusMessageFactory)
        {
            _integrationEventTopicServiceBusSender = integrationEventTopicServiceBusSender;
            _outboxMessageRepository = outboxMessageRepository;
            _clock = clock;
            _logger = logger;
            _serviceBusMessageFactory = serviceBusMessageFactory;
        }

        public async Task<bool> DispatchIntegrationEventsAsync(int numberOfIntegrationEventsToDispatch)
        {
            // Note: For future reference we log the publishing duration time.
            var watch = new Stopwatch();
            watch.Start();

            // Add 1 to number of messages ensure that the logic returns correctly.
            numberOfIntegrationEventsToDispatch += 1;

            var outboxMessages = await _outboxMessageRepository.GetByTakeAsync(numberOfIntegrationEventsToDispatch).ConfigureAwait(false);
            var serviceBusMessages = CreateServiceBusMessages(outboxMessages);
            await PublishServiceBusMessagesAsync(serviceBusMessages).ConfigureAwait(false);

            watch.Stop();
            _logger.LogInformation($"Publishing {outboxMessages.Count} service bus messages took {watch.Elapsed.Milliseconds} ms.");

            return outboxMessages.Count > numberOfIntegrationEventsToDispatch;
        }

        private IEnumerable<ServiceBusMessage> CreateServiceBusMessages(IEnumerable<OutboxMessage> outboxMessages)
        {
            var serviceBusMessages = new List<ServiceBusMessage>();
            foreach (var outboxMessage in outboxMessages)
            {
                var serviceBusMessage = _serviceBusMessageFactory.CreateServiceBusMessage(outboxMessage.Data, outboxMessage.MessageType);
                serviceBusMessages.Add(serviceBusMessage);
                outboxMessage.SetProcessed(_clock.GetCurrentInstant());
            }

            return serviceBusMessages;
        }

        private async Task PublishServiceBusMessagesAsync(IEnumerable<ServiceBusMessage> serviceBusMessages)
        {
            try
            {
                await _integrationEventTopicServiceBusSender
                    .SendMessagesAsync(serviceBusMessages)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Exception caught while trying to send service bus messages.");
                throw;
            }
        }
    }
}
