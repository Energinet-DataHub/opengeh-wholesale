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
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Application.Communication;

public class IntegrationEventProviderTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenNoUnpublishedBatches_DoesNotCommit(
        [Frozen] Mock<ICompletedCalculationRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        completedBatchRepositoryMock
            .Setup(mock => mock.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync((CompletedCalculation)null!);

        // Act
        await sut.GetAsync().ToListAsync();

        // Assert
        unitOfWorkMock.Verify(mock => mock.CommitAsync(), Times.Never);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenMultipleUnpublishedBatches_CommitsOncePerBatch(
        CompletedCalculation completedCalculation,
        [Frozen] Mock<ICompletedCalculationRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        completedBatchRepositoryMock
            .SetupSequence(mock => mock.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedCalculation)
            .ReturnsAsync(completedCalculation)
            .ReturnsAsync((CompletedCalculation)null!);

        // Act
        await sut.GetAsync().ToListAsync();

        // Assert
        unitOfWorkMock.Verify(mock => mock.CommitAsync(), Times.Exactly(2));
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenUnpublishedBatch_SetsPublishedTimeOfBatch(
        Instant instant,
        CompletedCalculation completedCalculation,
        [Frozen] Mock<ICompletedCalculationRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IClock> clockMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        completedBatchRepositoryMock
            .SetupSequence(mock => mock.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedCalculation)
            .ReturnsAsync((CompletedCalculation)null!);

        clockMock
            .Setup(mock => mock.GetCurrentInstant())
            .Returns(instant);

        // Act
        await sut.GetAsync().ToListAsync();

        // Assert
        completedCalculation.PublishedTime.Should().Be(instant);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenBatchWithMultipleEnergyResultEvents_ReturnsOneEventPerResult(
        CompletedCalculation completedCalculation,
        IntegrationEvent[] anyIntegrationEvents,
        [Frozen] Mock<ICompletedCalculationRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IEnergyResultEventProvider> energyResultEventProviderMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        completedBatchRepositoryMock
            .SetupSequence(mock => mock.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedCalculation)
            .ReturnsAsync((CompletedCalculation)null!);

        energyResultEventProviderMock
            .Setup(mock => mock.GetAsync(completedCalculation))
            .Returns(anyIntegrationEvents.ToAsyncEnumerable());

        // Act
        var actualEvents = await sut.GetAsync().ToListAsync();

        // Assert
        actualEvents.Should().HaveCount(anyIntegrationEvents.Length);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenRetrievalOfEnergyResultEventsFails_ReturnsEventsUpUntilFailureAndSetsPublishedTimeOfBatchToUnixEpoch(
        CompletedCalculation completedCalculation,
        IntegrationEvent[] anyIntegrationEvents,
        [Frozen] Mock<ICompletedCalculationRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IEnergyResultEventProvider> energyResultEventProviderMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        completedBatchRepositoryMock
            .SetupSequence(mock => mock.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedCalculation)
            .ReturnsAsync((CompletedCalculation)null!);

        energyResultEventProviderMock
            .Setup(mock => mock.GetAsync(completedCalculation))
            .Returns(ThrowsExceptionAfterAllItems(anyIntegrationEvents));

        // Act
        var actualEvents = await sut.GetAsync().ToListAsync();

        // Assert
        using var assertionAcope = new AssertionScope();
        completedCalculation.PublishedTime.Should().Be(NodaConstants.UnixEpoch);
        actualEvents.Should().HaveCount(anyIntegrationEvents.Length);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenBatchCanContainWholesaleResultsAndBatchWithMultipleWholesaleResultEvents_ReturnsOneEventPerResult(
        IntegrationEvent[] anyIntegrationEvents,
        [Frozen] Mock<ICompletedCalculationRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IWholesaleResultEventProvider> wholesaleResultEventProviderMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        var fixture = new Fixture();
        var completedBatch = fixture
            .Build<CompletedCalculation>()
            .With(p => p.ProcessType, ProcessType.WholesaleFixing)
            .Create();

        completedBatchRepositoryMock
            .SetupSequence(mock => mock.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedBatch)
            .ReturnsAsync((CompletedCalculation)null!);

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
    public async Task GetAsync_WhenBatchCanContainWholesaleResultsAndRetrievalOfWholesaleResultEventsFails_ReturnsEventsUpUntilFailureAndSetsPublishedTimeOfBatchToUnixEpoch(
        IntegrationEvent[] anyIntegrationEvents,
        [Frozen] Mock<ICompletedCalculationRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IWholesaleResultEventProvider> wholesaleResultEventProviderMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        var fixture = new Fixture();
        var completedBatch = fixture
            .Build<CompletedCalculation>()
            .With(p => p.ProcessType, ProcessType.WholesaleFixing)
            .Create();

        completedBatchRepositoryMock
            .SetupSequence(mock => mock.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedBatch)
            .ReturnsAsync((CompletedCalculation)null!);

        wholesaleResultEventProviderMock
            .Setup(mock => mock.CanContainWholesaleResults(completedBatch))
            .Returns(true);
        wholesaleResultEventProviderMock
            .Setup(mock => mock.GetAsync(completedBatch))
            .Returns(ThrowsExceptionAfterAllItems(anyIntegrationEvents));

        // Act
        var actualEvents = await sut.GetAsync().ToListAsync();

        // Assert
        using var assertionAcope = new AssertionScope();
        completedBatch.PublishedTime.Should().Be(NodaConstants.UnixEpoch);
        actualEvents.Should().HaveCount(anyIntegrationEvents.Length);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenTwoBatchesWithMultipleEventsCombined_ReturnsOneEventPerResult(
        IntegrationEvent[] eventsFromEnergyResultsInAggregationBatch,
        IntegrationEvent[] eventsFromEnergyResultsInWholesaleFixingBatch,
        IntegrationEvent[] eventsFromWholesaleResultsInWholesaleFixingBatch,
        [Frozen] Mock<ICompletedCalculationRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IEnergyResultEventProvider> energyResultEventProviderMock,
        [Frozen] Mock<IWholesaleResultEventProvider> wholesaleResultEventProviderMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        var expectedEventCount =
            eventsFromEnergyResultsInAggregationBatch.Length +
            eventsFromEnergyResultsInWholesaleFixingBatch.Length +
            eventsFromWholesaleResultsInWholesaleFixingBatch.Length;

        var fixture = new Fixture();
        var aggregationBatch = fixture
            .Build<CompletedCalculation>()
            .With(p => p.ProcessType, ProcessType.Aggregation)
            .Create();
        var wholesaleFixingBatch = fixture
            .Build<CompletedCalculation>()
            .With(p => p.ProcessType, ProcessType.WholesaleFixing)
            .Create();

        completedBatchRepositoryMock
            .SetupSequence(mock => mock.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(aggregationBatch)
            .ReturnsAsync(wholesaleFixingBatch)
            .ReturnsAsync((CompletedCalculation)null!);

        energyResultEventProviderMock
            .Setup(mock => mock.GetAsync(aggregationBatch))
            .Returns(eventsFromEnergyResultsInAggregationBatch.ToAsyncEnumerable());

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

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenRetrievalOfEnergyResultEventsFails_LogsExpectedMessage(
        CompletedBatch completedBatch,
        IntegrationEvent[] anyIntegrationEvents,
        [Frozen] Mock<ICompletedBatchRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IEnergyResultEventProvider> energyResultEventProviderMock,
        [Frozen] Mock<ILogger<IntegrationEventProvider>> loggerMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        const string expectedLogMessage = $"Failed energy result event publishing for completed calculation {LoggingConstants.CalculationId}. Handled '{LoggingConstants.EnergyResultCount}' energy results before failing.";
        completedBatchRepositoryMock
            .SetupSequence(mock => mock.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedBatch)
            .ReturnsAsync((CompletedBatch)null!);

        energyResultEventProviderMock
            .Setup(mock => mock.GetAsync(completedBatch))
            .Returns(ThrowsExceptionAfterAllItems(anyIntegrationEvents));

        // Act
        await sut.GetAsync().ToListAsync();

        // Assert
        loggerMock.ShouldBeCalledWith(LogLevel.Error, expectedLogMessage);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenRetrievalOfWholesaleResultEventsFails_LogsExpectedMessage(
        CompletedBatch completedBatch,
        IntegrationEvent[] anyIntegrationEvents,
        [Frozen] Mock<ICompletedBatchRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IWholesaleResultEventProvider> wholesaleResultEventProviderMock,
        [Frozen] Mock<ILogger<IntegrationEventProvider>> loggerMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        const string expectedLogMessage = $"Failed wholesale result event publishing for completed calculation {LoggingConstants.CalculationId}. Handled '{LoggingConstants.WholesaleResultCount}' wholesale results before failing.";
        completedBatchRepositoryMock
            .SetupSequence(mock => mock.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedBatch)
            .ReturnsAsync((CompletedBatch)null!);

        wholesaleResultEventProviderMock
            .Setup(mock => mock.CanContainWholesaleResults(completedBatch))
            .Returns(true);

        wholesaleResultEventProviderMock
            .Setup(mock => mock.GetAsync(completedBatch))
            .Returns(ThrowsExceptionAfterAllItems(anyIntegrationEvents));

        // Act
        await sut.GetAsync().ToListAsync();

        // Assert
        loggerMock.ShouldBeCalledWith(LogLevel.Error, expectedLogMessage);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenEnergyResultsEventsAreHandled_LogsExpectedMessages(
        CompletedBatch completedBatch,
        IntegrationEvent[] anyIntegrationEvents,
        [Frozen] Mock<ICompletedBatchRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IEnergyResultEventProvider> energyResultEventProviderMock,
        [Frozen] Mock<ILogger<IntegrationEventProvider>> loggerMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        const string expectedLogMessage =
            $"Handled {LoggingConstants.EnergyResultCount} energy results for completed calculation {LoggingConstants.CalculationId}.";
        completedBatchRepositoryMock
            .SetupSequence(mock => mock.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedBatch)
            .ReturnsAsync((CompletedBatch)null!);

        energyResultEventProviderMock
            .Setup(mock => mock.GetAsync(completedBatch))
            .Returns(anyIntegrationEvents.ToAsyncEnumerable());

        // Act
        await sut.GetAsync().ToListAsync();

        // Assert
        unitOfWorkMock.Verify(mock => mock.CommitAsync(), Times.Once);
        loggerMock.ShouldBeCalledWith(LogLevel.Information, expectedLogMessage);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenWholesaleResultsEventsAreHandled_LogsExpectedMessages(
        CompletedBatch completedBatch,
        IntegrationEvent[] anyIntegrationEvents,
        [Frozen] Mock<ICompletedBatchRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IWholesaleResultEventProvider> wholesaleResultEventProviderMock,
        [Frozen] Mock<ILogger<IntegrationEventProvider>> loggerMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        const string expectedLogMessage =
            $"Handled {LoggingConstants.WholesaleResultCount} wholesale results for completed calculation {LoggingConstants.CalculationId}.";
        completedBatchRepositoryMock
            .SetupSequence(mock => mock.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedBatch)
            .ReturnsAsync((CompletedBatch)null!);

        wholesaleResultEventProviderMock
            .Setup(mock => mock.GetAsync(completedBatch))
            .Returns(anyIntegrationEvents.ToAsyncEnumerable());

        wholesaleResultEventProviderMock
            .Setup(mock => mock.CanContainWholesaleResults(completedBatch))
            .Returns(true);

        // Act
        await sut.GetAsync().ToListAsync();

        // Assert
        unitOfWorkMock.Verify(mock => mock.CommitAsync(), Times.Once);
        loggerMock.ShouldBeCalledWith(LogLevel.Information, expectedLogMessage);
    }

    [Fact]
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

    private static async IAsyncEnumerable<IntegrationEvent> ThrowsExceptionAfterAllItems(IntegrationEvent[] integrationEvents)
    {
        await foreach (var integrationEvent in integrationEvents.ToAsyncEnumerable())
        {
            yield return integrationEvent;
        }

        throw new Exception("Simulate retrieval is failing with an exception.");
    }
}
