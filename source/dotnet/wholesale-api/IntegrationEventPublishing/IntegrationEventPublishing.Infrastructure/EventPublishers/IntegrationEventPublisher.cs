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

using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application.IntegrationEventsManagement;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Infrastructure.Persistence.Outbox;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Infrastructure.ServiceBus;

namespace Energinet.DataHub.Wholesale.IntegrationEventPublishing.Infrastructure.EventPublishers;

public class IntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IServiceBusMessageFactory _serviceBusMessageFactory;
    private readonly IIntegrationEventTopicServiceBusSender _integrationEventTopicServiceBusSender;

    public IntegrationEventPublisher(
        IServiceBusMessageFactory serviceBusMessageFactory,
        IIntegrationEventTopicServiceBusSender integrationEventTopicServiceBusSender)
    {
        _serviceBusMessageFactory = serviceBusMessageFactory;
        _integrationEventTopicServiceBusSender = integrationEventTopicServiceBusSender;
    }

    public async Task PublishAsync(IntegrationEventDto integrationEventDto)
    {
        var serviceBusMessage = _serviceBusMessageFactory.CreateServiceBusMessage(integrationEventDto.EventData, integrationEventDto.MessageType);
        await _integrationEventTopicServiceBusSender.SendAsync(serviceBusMessage).ConfigureAwait(false);
    }
}
