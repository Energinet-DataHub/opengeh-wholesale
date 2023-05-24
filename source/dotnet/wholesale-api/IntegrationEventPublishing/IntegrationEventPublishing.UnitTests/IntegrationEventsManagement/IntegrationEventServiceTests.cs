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
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application.IntegrationEventsManagement;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.IntegrationEventPublishing.UnitTests.IntegrationEventsManagement;

public class IntegrationEventServiceTests
{
    [Theory]
    [AutoMoqData]
    public async Task DispatchIntegrationEventsAsync_CallsCommitOnUnitOfWork(
        [Frozen] Mock<IIntegrationEventDispatcher> integrationEventDispatcherMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        IntegrationEventService sut)
    {
        // Arrange
        integrationEventDispatcherMock.Setup(x => x.DispatchIntegrationEventsAsync(1000)).ReturnsAsync(false);

        // Act
        await sut.DispatchIntegrationEventsAsync();

        // Assert
        unitOfWorkMock.Verify(x => x.CommitAsync());
    }

    [Theory]
    [AutoMoqData]
    public async Task DispatchIntegrationEventsAsync_UsesPositiveBulkSize(
        [Frozen] Mock<IIntegrationEventDispatcher> integrationEventDispatcherMock,
        IntegrationEventService sut)
    {
        // Arrange
        integrationEventDispatcherMock.Setup(x => x.DispatchIntegrationEventsAsync(1000)).ReturnsAsync(false);

        // Act
        await sut.DispatchIntegrationEventsAsync();

        // Assert
        integrationEventDispatcherMock.Verify(x => x.DispatchIntegrationEventsAsync(It.Is<int>(i => i > 0)));
    }

    [Theory]
    [AutoMoqData]
    public async Task DeleteOlderDispatchedIntegrationEventsAsync_CallsDeleteOlderDispatchedIntegrationEventsOnIntegrationEventCleanUpService(
        [Frozen] IOptions<IntegrationEventRetentionOptions> options,
        [Frozen] Mock<IIntegrationEventCleanUpService> integrationEventCleanUpServiceMock,
        IntegrationEventService sut)
    {
        // Act
        await sut.DeleteOlderDispatchedIntegrationEventsAsync();

        // Assert
        integrationEventCleanUpServiceMock.Verify(x => x.DeleteOlderDispatchedIntegrationEvents(options.Value.RetentionDays));
    }

    [Theory]
    [AutoMoqData]
    public async Task DeleteOlderDispatchedIntegrationEventsAsync_CommitsUnitOfWork(
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        IntegrationEventService sut)
    {
        // Act
        await sut.DeleteOlderDispatchedIntegrationEventsAsync();

        // Assert
        unitOfWorkMock.Verify(x => x.CommitAsync());
    }
}
