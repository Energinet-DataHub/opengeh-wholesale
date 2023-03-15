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
using System.Text;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Domain;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Infrastructure.EventPublishing
{
    public class IntegrationEventDispatcher : IIntegrationEventDispatcher
    {
        private readonly IIntegrationEventTopicServiceBusSender _integrationEventTopicServiceBusSender;
        private readonly IOutboxMessageRepository _outboxMessageRepository;
        private readonly IClock _clock;
        private readonly IDatabaseContext _context;
        private readonly ILogger<IntegrationEventDispatcher> _logger;
        private readonly IJsonSerializer _jsonSerializer;

        public IntegrationEventDispatcher(
            IIntegrationEventTopicServiceBusSender integrationEventTopicServiceBusSender,
            IOutboxMessageRepository outboxMessageRepository,
            IClock clock,
            IDatabaseContext context,
            ILogger<IntegrationEventDispatcher> logger,
            IJsonSerializer jsonSerializer)
        {
            _integrationEventTopicServiceBusSender = integrationEventTopicServiceBusSender;
            _outboxMessageRepository = outboxMessageRepository;
            _clock = clock;
            _context = context;
            _logger = logger;
            _jsonSerializer = jsonSerializer;
        }

        public async Task PublishIntegrationEventsAsync(CancellationToken token)
        {
            // Note: For future sake we log the publishing duration time.
            var watch = new Stopwatch();
            watch.Start();

            var outboxMessages = await _outboxMessageRepository.GetByTakeAsync(50, token).ConfigureAwait(false);
            foreach (var outboxMessage in outboxMessages)
            {
                try
                {
                    // TODO AJH var serviceBusMessage = System.Text.Json.JsonSerializer.Deserialize<ServiceBusMessage>(outboxMessage.Data);
                    var serviceBusMessage = _jsonSerializer.Deserialize<ServiceBusMessage>(Encoding.UTF8.GetString(outboxMessage.Data));
                    if (serviceBusMessage == null)
                    {
                        throw new NullReferenceException("serviceBusMessage");
                    }

                    await _integrationEventTopicServiceBusSender
                        .SendMessageAsync(serviceBusMessage, token)
                        .ConfigureAwait(false);

                    outboxMessage.SetProcessed(_clock.GetCurrentInstant());
                    await _context.SaveChangesAsync(token).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception caught while trying to publish integration event with ID {outboxMessageId}", outboxMessage.Id);
                }
            }

            watch.Stop();
            _logger.LogInformation($"Publishing {outboxMessages.Count} integration events took {watch.Elapsed.Milliseconds} ms.");
        }
    }
}
