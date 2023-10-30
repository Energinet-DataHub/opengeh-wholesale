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

using System.Reflection;
using AutoFixture;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Common.Models;
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using FluentAssertions;
using Moq;
using NodaTime;
using Test.Core.Attributes;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Application.Communication;

public class IntegrationEventProviderTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenNoUnpublishedBatches_DoesNotCommit(
        [Frozen] Mock<ICompletedBatchRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        completedBatchRepositoryMock
            .Setup(mock => mock.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync((CompletedBatch)null!);

        // Act
        await sut.GetAsync().ToListAsync();

        // Assert
        unitOfWorkMock.Verify(mock => mock.CommitAsync(), Times.Never);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenMultipleUnpublishedBatches_CommitsOncePerBatch(
        CompletedBatch completedBatch,
        [Frozen] Mock<ICompletedBatchRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        completedBatchRepositoryMock
            .SetupSequence(mock => mock.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedBatch)
            .ReturnsAsync(completedBatch)
            .ReturnsAsync((CompletedBatch)null!);

        // Act
        await sut.GetAsync().ToListAsync();

        // Assert
        unitOfWorkMock.Verify(mock => mock.CommitAsync(), Times.Exactly(2));
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenUnpublishedBatch_SetsPublishedTimeOfBatch(
        Instant instant,
        CompletedBatch completedBatch,
        [Frozen] Mock<ICompletedBatchRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IClock> clockMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        completedBatchRepositoryMock
            .SetupSequence(mock => mock.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedBatch)
            .ReturnsAsync((CompletedBatch)null!);

        clockMock
            .Setup(mock => mock.GetCurrentInstant())
            .Returns(instant);

        // Act
        await sut.GetAsync().ToListAsync();

        // Assert
        completedBatch.PublishedTime.Should().Be(instant);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenUnpublishedBatchWithMultipleEnergyResultEvents_ReturnsOneEventPerResult(
        CompletedBatch completedBatch,
        IntegrationEvent[] anyIntegrationEvents,
        [Frozen] Mock<ICompletedBatchRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IEnergyResultEventProvider> energyResultEventProviderMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        completedBatchRepositoryMock
            .SetupSequence(mock => mock.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedBatch)
            .ReturnsAsync((CompletedBatch)null!);

        energyResultEventProviderMock
            .Setup(mock => mock.GetAsync(completedBatch))
            .Returns(anyIntegrationEvents.ToAsyncEnumerable());

        // Act
        var actualEvents = await sut.GetAsync().ToListAsync();

        // Assert
        actualEvents.Should().HaveCount(anyIntegrationEvents.Length);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenBatchCanContainWholesaleResultsAndBatchWithMultipleWholesaleResultEvents_ReturnsOneEventPerResult(
        IntegrationEvent[] anyIntegrationEvents,
        [Frozen] Mock<ICompletedBatchRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IWholesaleResultEventProvider> wholesaleResultEventProviderMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        var fixture = new Fixture();
        var completedBatch = fixture
            .Build<CompletedBatch>()
            .With(p => p.ProcessType, ProcessType.WholesaleFixing)
            .Create();

        completedBatchRepositoryMock
            .SetupSequence(mock => mock.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedBatch)
            .ReturnsAsync((CompletedBatch)null!);

        wholesaleResultEventProviderMock
            .Setup(mock => mock.CanContainWholesaleResults(completedBatch))
            .Returns(true);
        wholesaleResultEventProviderMock
            .Setup(mock => mock.GetAsync(completedBatch))
            .Returns(anyIntegrationEvents.ToAsyncEnumerable());

        // Act
        var actualEvents = await sut.GetAsync().ToListAsync();

        // Assert
        actualEvents.Should().HaveCount(anyIntegrationEvents.Length);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenTwoBatchesWithMultipleEventsCombined_ReturnsOneEventPerResult(
        IntegrationEvent[] eventsFromEnergyResultsInAggrationBatch,
        IntegrationEvent[] eventsFromEnergyResultsInWholesaleFixingBatch,
        IntegrationEvent[] eventsFromWholesaleResultsInWholesaleFixingBatch,
        [Frozen] Mock<ICompletedBatchRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IEnergyResultEventProvider> energyResultEventProviderMock,
        [Frozen] Mock<IWholesaleResultEventProvider> wholesaleResultEventProviderMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        var expectedEventCount =
            eventsFromEnergyResultsInAggrationBatch.Length +
            eventsFromEnergyResultsInWholesaleFixingBatch.Length +
            eventsFromWholesaleResultsInWholesaleFixingBatch.Length;

        var fixture = new Fixture();
        var aggregationBatch = fixture
            .Build<CompletedBatch>()
            .With(p => p.ProcessType, ProcessType.Aggregation)
            .Create();
        var wholesaleFixingBatch = fixture
            .Build<CompletedBatch>()
            .With(p => p.ProcessType, ProcessType.WholesaleFixing)
            .Create();

        completedBatchRepositoryMock
            .SetupSequence(mock => mock.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(aggregationBatch)
            .ReturnsAsync(wholesaleFixingBatch)
            .ReturnsAsync((CompletedBatch)null!);

        energyResultEventProviderMock
            .Setup(mock => mock.GetAsync(aggregationBatch))
            .Returns(eventsFromEnergyResultsInAggrationBatch.ToAsyncEnumerable());

        energyResultEventProviderMock
            .Setup(mock => mock.GetAsync(wholesaleFixingBatch))
            .Returns(eventsFromEnergyResultsInWholesaleFixingBatch.ToAsyncEnumerable());
        wholesaleResultEventProviderMock
            .Setup(mock => mock.CanContainWholesaleResults(wholesaleFixingBatch))
            .Returns(true);
        wholesaleResultEventProviderMock
            .Setup(mock => mock.GetAsync(wholesaleFixingBatch))
            .Returns(eventsFromWholesaleResultsInWholesaleFixingBatch.ToAsyncEnumerable());

        // Act
        var actualEvents = await sut.GetAsync().ToListAsync();

        // Assert
        actualEvents.Should().HaveCount(expectedEventCount);
    }

    [Fact]
    [AcceptanceTest]
    public void AProvider_MustImplement_IIntegrationEventProvider()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(Events.Application.Root));
        var interfaceType = typeof(IIntegrationEventProvider);

        // Act
        var actualImplementations = assembly!.GetTypes()
            .Where(type => interfaceType.IsAssignableFrom(type) && !type.IsInterface)
            .ToList();

        // Assert
        actualImplementations.Should().HaveCount(1, $"The interface {nameof(IIntegrationEventProvider)} must be implemented.");
    }
}
