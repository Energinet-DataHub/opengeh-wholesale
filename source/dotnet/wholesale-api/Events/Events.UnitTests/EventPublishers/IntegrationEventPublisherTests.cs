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
using Energinet.DataHub.Wholesale.Events.Application.IntegrationEventsManagement;
using Energinet.DataHub.Wholesale.Events.Infrastructure.EventPublishers;
using Energinet.DataHub.Wholesale.Events.Infrastructure.ServiceBus;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.EventPublishers;

public class IntegrationEventPublisherTests
{
    [Fact]
    public async Task PublishAsync_WhenCalled_UsesIntegrationEventTopicServiceSender()
    {
        // Arrange
        var serviceBusMessageFactory = new Mock<IServiceBusMessageFactory>();
        var integrationEventTopicServiceBusSender = new Mock<IIntegrationEventTopicServiceBusSender>();
        var integrationEventPublisher = new IntegrationEventPublisher(
            serviceBusMessageFactory.Object,
            integrationEventTopicServiceBusSender.Object);

        var integrationEventDto = new IntegrationEventDto();

        // Act
        await integrationEventPublisher.PublishAsync(integrationEventDto);

        // Assert
        integrationEventTopicServiceBusSender.Verify(
            x => x.SendAsync(It.IsAny<ServiceBusMessage>()),
            Times.Once);
    }
}
