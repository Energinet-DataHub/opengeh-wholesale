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

using AutoFixture.Xunit2;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Infrastructure.IntegrationEventDispatching;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Infrastructure.IntegrationEventPublishing;

public class IntegrationEventDispatcherTests
{
    /* TODO AJW Fix
    [Theory]
    [AutoMoqData]
    public async Task DispatchIntegrationEventsAsync_CallsCreateServiceBusMessageAndSendMessagesAsyncWithCorrectParameters(
        [Frozen] Mock<IOutboxMessageRepository> outboxMessageRepositoryMock,
        [Frozen] Mock<IServiceBusMessageFactory> serviceBusMessageFactoryMock,
        [Frozen] Mock<IIntegrationEventTopicServiceBusSender> integrationEventTopicServiceBusSenderMock,
        ServiceBusMessageBatch serviceBusMessageBatch,
        ServiceBusMessage serviceBusMessage,
        IntegrationEventDispatcher sut)
    {
        // Arrange
        serviceBusMessageFactoryMock
            .Setup(x => x.CreateServiceBusMessage(new byte[10], CalculationResultCompleted.BalanceFixingEventName))
            .Returns(serviceBusMessage);

        var outboxMessage = CreateOutboxMessage(CalculationResultCompleted.BalanceFixingEventName);
        outboxMessageRepositoryMock
            .Setup(x => x.GetByTakeAsync(11))
            .ReturnsAsync(new List<OutboxMessage> { outboxMessage });

        integrationEventTopicServiceBusSenderMock.Setup(x => x.CreateBusMessageBatchAsync())
            .ReturnsAsync(serviceBusMessageBatch);

        // Act
        await sut.DispatchIntegrationEventsAsync(10);

        // Assert
        serviceBusMessageFactoryMock.Verify(x => x.CreateServiceBusMessage(It.IsAny<byte[]>(), CalculationResultCompleted.BalanceFixingEventName));
        integrationEventTopicServiceBusSenderMock.Verify(x => x.SendAsync(new List<ServiceBusMessage> { serviceBusMessage }));
    }

    [Theory]
    [InlineAutoMoqData(0, 0, false)]
    [InlineAutoMoqData(0, 1, false)]
    [InlineAutoMoqData(1, 0, true)]
    [InlineAutoMoqData(1, 1, false)]
    [InlineAutoMoqData(2, 1, true)]
    [InlineAutoMoqData(3, 1, true)]
    public async Task DispatchIntegrationEventsAsync_ReturnsFalseWhenTheNumberOfMessagesLeftAreLesserThanBulkSize(
        int numberOfMessages,
        int bulkSize,
        bool expected,
        [Frozen] Mock<IOutboxMessageRepository> outboxMessageRepositoryMock,
        [Frozen] Mock<IServiceBusMessageFactory> serviceBusMessageFactoryMock,
        ServiceBusMessage serviceBusMessage,
        IntegrationEventDispatcher sut)
    {
        // Arrange
        serviceBusMessageFactoryMock
            .Setup(x => x.CreateServiceBusMessage(new byte[10], CalculationResultCompleted.BalanceFixingEventName))
            .Returns(serviceBusMessage);

        var outboxMessages = GenerateOutboxMessages(numberOfMessages);
        outboxMessageRepositoryMock
            .Setup(x => x.GetByTakeAsync(bulkSize + 1))
            .ReturnsAsync(outboxMessages);

        // Act
        var actual = await sut.DispatchIntegrationEventsAsync(bulkSize);

        // Assert
        Assert.Equal(expected, actual);
    }*/

    private static List<OutboxMessage> GenerateOutboxMessages(int numberOfMessages)
    {
        var outboxMessages = new List<OutboxMessage>();
        for (var i = 0; i < numberOfMessages; i++)
        {
            var message = CreateOutboxMessage(CalculationResultCompleted.BalanceFixingEventName);
            outboxMessages.Add(message);
        }

        return outboxMessages;
    }

    private static OutboxMessage CreateOutboxMessage(string messageType)
    {
        return new OutboxMessage(new byte[10], messageType, SystemClock.Instance.GetCurrentInstant());
    }
}
