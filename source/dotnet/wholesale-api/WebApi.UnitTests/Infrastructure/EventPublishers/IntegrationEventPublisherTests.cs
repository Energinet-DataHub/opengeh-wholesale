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
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.IntegrationEventsPublishing.Application.IntegrationEventsManagement;
using Energinet.DataHub.Wholesale.IntegrationEventsPublishing.Infrastructure.EventPublishers;
using Energinet.DataHub.Wholesale.IntegrationEventsPublishing.Infrastructure.Persistence.Outbox;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Infrastructure.EventPublishers;

public class IntegrationEventPublisherTests
{
    [Theory]
    [AutoMoqData]
    public async Task AddAsync_WhenCreatingOutboxMessage(
        [Frozen] Mock<IOutboxMessageRepository> outboxMessageRepositoryMock,
        IntegrationEventDto integrationEventDto,
        IntegrationEventPublisher sut)
    {
        // Arrange & Act
        await sut.PublishAsync(integrationEventDto);

        // Assert
        outboxMessageRepositoryMock.Verify(x => x.AddAsync(It.Is<OutboxMessage>(message =>
            message.MessageType == integrationEventDto.MessageType
            && message.Data == integrationEventDto.EventData
            && message.CreationDate == integrationEventDto.CreationDate)));
    }
}
