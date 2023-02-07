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
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;

namespace Energinet.DataHub.Wholesale.Infrastructure.EventPublishers;

public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IDomainEventTopicServiceBusSender _serviceBusSender;
    private readonly IServiceBusMessageFactory _serviceBusMessageFactory;

    public DomainEventPublisher(
        IDomainEventTopicServiceBusSender serviceBusSender,
        IServiceBusMessageFactory serviceBusMessageFactory)
    {
        _serviceBusSender = serviceBusSender;
        _serviceBusMessageFactory = serviceBusMessageFactory;
    }

    public async Task PublishAsync<TDomainEventDto>(TDomainEventDto domainEvent)
        where TDomainEventDto : DomainEventDto
    {
        var message = _serviceBusMessageFactory.Create(domainEvent);
        await _serviceBusSender.SendMessageAsync(message).ConfigureAwait(false);
    }

    public async Task PublishAsync<TDomainEventDto>(IList<TDomainEventDto> domainEvents)
        where TDomainEventDto : DomainEventDto
    {
        var messages = _serviceBusMessageFactory.Create(domainEvents);
        await _serviceBusSender.SendMessagesAsync(messages).ConfigureAwait(false);
    }
}
