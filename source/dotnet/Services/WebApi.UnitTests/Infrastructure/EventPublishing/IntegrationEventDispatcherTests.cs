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

using System.Text;
using AutoFixture.Xunit2;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Domain;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.EventPublishing;
using Energinet.DataHub.Wholesale.Infrastructure.Integration;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using FluentAssertions;
using Google.Protobuf;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Infrastructure.EventPublishing;

public class IntegrationEventDispatcherTests
{
   /* [Theory]
    [InlineAutoMoqData]
    public async Task PublishIntegrationEventsAsync_ReturnVoid(
        [Frozen] Mock<IIntegrationEventTopicServiceBusSender> integrationEventTopicServiceBusSenderMock,
        [Frozen] Mock<IOutboxMessageRepository> outboxMessageRepositoryMock,
        [Frozen] Mock<IJsonSerializer> jsonSerializerMock,
        [Frozen] Mock<IClock> clockMock,
        [Frozen] Mock<IDatabaseContext> databaseContextMock,
        IList<OutboxMessage> outboxMessages,
        IntegrationEventDispatcher sut)
    {
        // Arrange
        outboxMessageRepositoryMock.Setup(x => x.GetByTakeAsync(50, default)).ReturnsAsync(outboxMessages);
        jsonSerializerMock.Setup(x => x.Serialize()).Returns();

        // Act
        await sut.PublishIntegrationEventsAsync(default);

        // Assert
        databaseContextMock.Verify(x => x.SaveChangesAsync(default), Times.Exactly(3));
    }*/
}
