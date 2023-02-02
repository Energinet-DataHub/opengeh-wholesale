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
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Infrastructure.Integration;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Tests.Infrastructure.Processes;

public class ProcessCompletedIntegrationEventPublisherTests
{
    public class TestServiceBusSender : ServiceBusSender { }

    [Theory]
    [InlineAutoMoqData]
    public async Task PublishAsync_Publishes(
        ProcessCompleted processCompleted,
        ProcessCompletedEventDto eventDto,
        Mock<IServiceBusMessageFactory> factoryMock,
        Mock<IProcessCompletedIntegrationEventMapper> mapperMock)
    {
        // Arrange
        var senderMock = new Mock<IntegrationEventTopicServiceBusSender>();
        mapperMock
            .Setup(mapper => mapper.MapFrom(eventDto))
            .Returns(processCompleted);
        var sut = new ProcessCompletedIntegrationEventPublisher(senderMock.Object, factoryMock.Object, mapperMock.Object);

        // Act
        await sut.PublishAsync(eventDto);

        // Assert
        senderMock.Verify(
            sender => sender.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineAutoMoqData(ProcessType.BalanceFixing, ProcessCompleted.BalanceFixingProcessType)]
    public async Task PublishAsync_PublishesWithCorrectMessageType(
        ProcessType processType,
        string expectedMessageType,
        ProcessCompleted processCompleted,
        Mock<IServiceBusMessageFactory> factoryMock,
        Mock<IProcessCompletedIntegrationEventMapper> mapperMock)
    {
        // Arrange
        var eventDto = CreateProcessCompletedEventDto(processType);
        var senderMock = new Mock<IntegrationEventTopicServiceBusSender>();
        mapperMock
            .Setup(mapper => mapper.MapFrom(eventDto))
            .Returns(processCompleted);
        factoryMock
            .Setup(factory => factory.Create(processCompleted, expectedMessageType))
            .Returns(new ServiceBusMessage
            {
                ApplicationProperties = { { MessageMetaDataConstants.MessageType, expectedMessageType } },
            });
        var sut = new ProcessCompletedIntegrationEventPublisher(senderMock.Object, factoryMock.Object, mapperMock.Object);

        // Act
        await sut.PublishAsync(eventDto);

        // Assert
        senderMock.Verify(
            sender => sender.SendMessageAsync(
                It.Is<ServiceBusMessage>(message => (string)message.ApplicationProperties[MessageMetaDataConstants.MessageType] == expectedMessageType),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static ProcessCompletedEventDto CreateProcessCompletedEventDto(ProcessType processType)
    {
        return new ProcessCompletedEventDto(
            "some-grid-area",
            Guid.NewGuid(),
            processType,
            Instant.MinValue,
            Instant.MinValue);
    }
}
