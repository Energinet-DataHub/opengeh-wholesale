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
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Infrastructure.Persistence.Outbox;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.IntegrationEventPublishing.UnitTests.EventPublishers;

public class IntegrationEventCleanUpServiceTests
{
    [Theory]
    [AutoMoqData]
    public void DeleteOlderDispatchedIntegrationEvents_CallsDeleteProcessedOlderThan(
        [Frozen] Mock<IOutboxMessageRepository> outboxMessageRepositoryMock,
        [Frozen] Mock<IClock> clockMock,
        IntegrationEventCleanUpService sut)
    {
        // Arrange
        const int daysOld = 10;
        var instant = SystemClock.Instance.GetCurrentInstant();
        clockMock.Setup(x => x.GetCurrentInstant()).Returns(instant);

        // Act
        sut.DeleteOlderDispatchedIntegrationEvents(daysOld);

        // Assert
        outboxMessageRepositoryMock.Verify(x => x.DeleteProcessedOlderThan(instant.Minus(Duration.FromDays(daysOld))));
    }
}
