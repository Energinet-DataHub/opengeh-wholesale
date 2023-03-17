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
using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Infrastructure.EventPublishers;
using Energinet.DataHub.Wholesale.Infrastructure.IntegrationEventDispatching;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Infrastructure.IntegrationEventPublishing;

public class IntegrationEventDispatcherTests
{
    [Theory]
    [AutoMoqData]
    public async Task PublishIntegrationEventsAsync_ReturnsVoid(
        [Frozen] Mock<IOutboxMessageRepository> outboxMessageRepositoryMock,
        [Frozen] Mock<IClock> clockMock,
        [Frozen] Mock<IDatabaseContext> databaseContextMock,
        [Frozen] Mock<ILogger<IntegrationEventDispatcher>> loggerMock,
        [Frozen] Mock<IServiceBusMessageFactory> serviceBusMessageFactoryMock,
        [Frozen] Mock<IIntegrationEventTopicServiceBusSender> integrationEventTopicServiceBusSenderMock)
    {
        // Arrange
        var outboxMessage1 = CreateOutboxMessage(new byte[10], CalculationResultCompleted.BalanceFixingEventName); //TODO: should we create a custom type to use when testing?
        outboxMessageRepositoryMock.Setup(x => x.GetByTakeAsync(50, default))
            .ReturnsAsync(new List<OutboxMessage> { outboxMessage1 });

        var sut = new IntegrationEventDispatcher(
            integrationEventTopicServiceBusSenderMock.Object,
            outboxMessageRepositoryMock.Object,
            clockMock.Object,
            databaseContextMock.Object,
            loggerMock.Object,
            serviceBusMessageFactoryMock.Object);

        // Act
        await sut.PublishIntegrationEventsAsync(default);

        // Assert
        serviceBusMessageFactoryMock.Verify(x => x.CreateProcessCompleted(It.IsAny<byte[]>(), CalculationResultCompleted.BalanceFixingEventName));
        databaseContextMock.Verify(x => x.SaveChangesAsync(default));
        integrationEventTopicServiceBusSenderMock.Verify(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default));
    }

    private static OutboxMessage CreateOutboxMessage(byte[] protobufEventData, string messageType)
    {
        return new OutboxMessage(new IntegrationEventDto(protobufEventData, messageType, SystemClock.Instance.GetCurrentInstant()));
    }
}
