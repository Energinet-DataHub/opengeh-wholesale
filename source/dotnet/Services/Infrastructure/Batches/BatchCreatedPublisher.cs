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

using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.Infrastructure.Batches;

public class BatchCreatedPublisher : IBatchCreatedPublisher
{
    private readonly IServiceBusMessageFactory _serviceBusMessageFactory;
    private readonly DomainEventTopicServiceBusSender _serviceBusSender;
    private readonly IOptions<DomainEventTopicSettings> _settings;

    public BatchCreatedPublisher(
        IServiceBusMessageFactory serviceBusMessageFactory,
        DomainEventTopicServiceBusSender serviceBusSender,
        IOptions<DomainEventTopicSettings> settings)
    {
        _serviceBusMessageFactory = serviceBusMessageFactory;
        _serviceBusSender = serviceBusSender;
        _settings = settings;
    }

    public async Task PublishAsync(BatchCreatedDomainEventDto batchCreatedDomainEventDto)
    {
        var message = _serviceBusMessageFactory.Create(batchCreatedDomainEventDto, _settings.Value.TopicName);
        await _serviceBusSender.SendMessageAsync(message).ConfigureAwait(false);
    }
}
