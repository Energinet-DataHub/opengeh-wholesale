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
using Energinet.DataHub.Wholesale.IntegrationEventsPublishing.Infrastructure.IntegrationEventDispatching;
using Energinet.DataHub.Wholesale.IntegrationEventsPublishing.Infrastructure.Persistence.Outbox;
using Energinet.DataHub.Wholesale.IntegrationEventsPublishing.Infrastructure.ServiceBus;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Infrastructure.IntegrationEventPublishing;

public class IntegrationEventDispatcherTests
{
    [Theory]
    [AutoMoqData]
    public async Task DispatchIntegrationEventsAsync_WhenDispatchingEvents_CallsSendWithTheCorrectServiceBusMessageBatch(
        [Frozen] Mock<IOutboxMessageRepository> outboxMessageRepositoryMock,
        [Frozen] Mock<IIntegrationEventTopicServiceBusSender> integrationEventTopicServiceBusSenderMock,
        IntegrationEventDispatcher sut)
    {
        // Arrange
        var outboxMessage = CreateOutboxMessage(new byte[10], CalculationResultCompleted.BalanceFixingEventName);
        outboxMessageRepositoryMock
            .Setup(x => x.GetByTakeAsync(11))
            .ReturnsAsync(new List<OutboxMessage> { outboxMessage });

        // It is not possible to mock a ServiceBusMessageBatch - hasn't a public constructor or an interface
        var serviceBusMessageBatch = ServiceBusModelFactory.ServiceBusMessageBatch(1000, new List<ServiceBusMessage>(), new CreateMessageBatchOptions());
        integrationEventTopicServiceBusSenderMock.Setup(x => x.CreateBusMessageBatchAsync())
            .ReturnsAsync(serviceBusMessageBatch);

        // Act
        await sut.DispatchIntegrationEventsAsync(10);

        // Assert
        integrationEventTopicServiceBusSenderMock.Verify(x => x.SendAsync(serviceBusMessageBatch));
    }

    [Theory]
    [AutoMoqData]
    public async Task DispatchIntegrationEventsAsync_CallsCreateServiceBusMessageWithTheCorrectParameters(
        [Frozen] Mock<IOutboxMessageRepository> outboxMessageRepositoryMock,
        [Frozen] Mock<IServiceBusMessageFactory> serviceBusMessageFactoryMock,
        [Frozen] Mock<IIntegrationEventTopicServiceBusSender> integrationEventTopicServiceBusSenderMock,
        byte[] bytes,
        ServiceBusMessage serviceBusMessage,
        IntegrationEventDispatcher sut)
    {
        // Arrange
        serviceBusMessageFactoryMock
            .Setup(x => x.CreateServiceBusMessage(bytes, CalculationResultCompleted.BalanceFixingEventName))
            .Returns(serviceBusMessage);

        var outboxMessage = CreateOutboxMessage(bytes, CalculationResultCompleted.BalanceFixingEventName);
        outboxMessageRepositoryMock
            .Setup(x => x.GetByTakeAsync(11))
            .ReturnsAsync(new List<OutboxMessage> { outboxMessage });

        var serviceBusMessageBatch = ServiceBusModelFactory.ServiceBusMessageBatch(1000, new List<ServiceBusMessage>(), new CreateMessageBatchOptions());
        integrationEventTopicServiceBusSenderMock.Setup(x => x.CreateBusMessageBatchAsync())
            .ReturnsAsync(serviceBusMessageBatch);

        // Act
        await sut.DispatchIntegrationEventsAsync(10);

        // Assert
        serviceBusMessageFactoryMock.Verify(x => x.CreateServiceBusMessage(bytes, CalculationResultCompleted.BalanceFixingEventName));
    }

    [Theory]
    [AutoMoqData]
    public async Task DispatchIntegrationEventsAsync_MustMarkOutboxMessagesAsProcessed(
        [Frozen] Mock<IOutboxMessageRepository> outboxMessageRepositoryMock,
        [Frozen] Mock<IIntegrationEventTopicServiceBusSender> integrationEventTopicServiceBusSenderMock,
        IntegrationEventDispatcher sut)
    {
        // Arrange
        var outboxMessage1 = CreateOutboxMessage(new byte[10], CalculationResultCompleted.BalanceFixingEventName);
        var outboxMessage2 = CreateOutboxMessage(new byte[10], CalculationResultCompleted.BalanceFixingEventName);
        var outboxMessages = new List<OutboxMessage> { outboxMessage1, outboxMessage2 };
        outboxMessageRepositoryMock
            .Setup(x => x.GetByTakeAsync(11))
            .ReturnsAsync(outboxMessages);

        // It is not possible to mock a ServiceBusMessageBatch - it doesn't have a public constructor or interface
        var serviceBusMessageBatch = ServiceBusModelFactory.ServiceBusMessageBatch(1000, new List<ServiceBusMessage>(), new CreateMessageBatchOptions());
        integrationEventTopicServiceBusSenderMock.Setup(x => x.CreateBusMessageBatchAsync())
            .ReturnsAsync(serviceBusMessageBatch);

        // Act
        await sut.DispatchIntegrationEventsAsync(10);

        // Assert
        Assert.True(outboxMessages.TrueForAll(x => x.ProcessedDate.HasValue));
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
        [Frozen] Mock<IIntegrationEventTopicServiceBusSender> integrationEventTopicServiceBusSenderMock,
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

        var serviceBusMessageBatch = ServiceBusModelFactory.ServiceBusMessageBatch(1000, new List<ServiceBusMessage>(), new CreateMessageBatchOptions());
        integrationEventTopicServiceBusSenderMock.Setup(x => x.CreateBusMessageBatchAsync())
            .ReturnsAsync(serviceBusMessageBatch);

        // Act
        var actual = await sut.DispatchIntegrationEventsAsync(bulkSize);

        // Assert
        Assert.Equal(expected, actual);
    }

    private static List<OutboxMessage> GenerateOutboxMessages(int numberOfMessages)
    {
        var outboxMessages = new List<OutboxMessage>();
        for (var i = 0; i < numberOfMessages; i++)
        {
            var message = CreateOutboxMessage(new byte[10], CalculationResultCompleted.BalanceFixingEventName);
            outboxMessages.Add(message);
        }

        return outboxMessages;
    }

    private static OutboxMessage CreateOutboxMessage(byte[] data, string messageType)
    {
        return new OutboxMessage(data, messageType, SystemClock.Instance.GetCurrentInstant());
    }
}
