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
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Infrastructure.Processes;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Tests.Infrastructure.Processes;

public class ProcessCompletedIntegrationEventPublisherTests
{
    public class TestServiceBusSender : ServiceBusSender { }

    [Theory]
    [InlineAutoMoqData]
    public async Task PublishAsync_Publishes(
        ProcessCompletedEventDto eventDto,
        Mock<IServiceBusMessageFactory> factory)
    {
        // Arrange
        var senderMock = new Mock<TestServiceBusSender>();
        var sut = new ProcessCompletedIntegrationEventPublisher(senderMock.Object, factory.Object);

        // Act
        await sut.PublishAsync(eventDto);

        // Assert
        senderMock.Verify(
            sender => sender.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
