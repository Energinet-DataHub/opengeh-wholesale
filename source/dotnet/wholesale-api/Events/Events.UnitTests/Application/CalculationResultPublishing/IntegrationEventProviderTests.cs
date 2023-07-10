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
using Energinet.DataHub.Core.Messaging.Communication.Internal;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using FluentAssertions;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Application.CalculationResultPublishing;

public class IntegrationEventProviderTests
{
    [Theory]
    [InlineAutoMoqData]
    public void GetAsync_WhenNoUnpublishedCalculations_DoesNotCommit(
        [Frozen] Mock<ICompletedCalculationRepository> completedCalculationRepositoryMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        completedCalculationRepositoryMock
            .Setup(p => p.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync((CompletedCalculation)null!);

        // Act
        var unused = sut.GetAsync().ToListAsync();

        // Assert
        unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
    }

    [Theory]
    [InlineAutoMoqData]
    public void GetAsync_WhenMultipleUnpublishedCalculations_CommitsOncePerCalculation(
        CompletedCalculation completedCalculation,
        CalculationResult calculationResult,
        IntegrationEvent anyIntegrationEvent,
        [Frozen] Mock<ICompletedCalculationRepository> completedCalculationRepositoryMock,
        [Frozen] Mock<ICalculationResultQueries> calculationResultQueriesMock,
        [Frozen] Mock<ICalculationResultIntegrationEventFactory> calculationResultIntegrationEventFactoryMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        completedCalculationRepositoryMock
            .SetupSequence(p => p.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedCalculation)
            .ReturnsAsync(completedCalculation)
            .ReturnsAsync((CompletedCalculation)null!);

        calculationResultQueriesMock
            .Setup(queries => queries.GetAsync(completedCalculation.Id))
            .Returns(AsAsyncEnumerable(calculationResult));

        calculationResultIntegrationEventFactoryMock
            .Setup(factory => factory.Create(calculationResult))
            .Returns(anyIntegrationEvent);

        // Act
        var unused = sut.GetAsync().ToListAsync();

        // Assert: Commits once per unpublished calculation
        unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Exactly(2));
    }

    [Theory]
    [InlineAutoMoqData]
    public void GetAsync_SetsPublishedTimeOfCalculation(
        Instant instant,
        CompletedCalculation completedCalculation,
        [Frozen] Mock<ICompletedCalculationRepository> completedCalculationRepositoryMock,
        [Frozen] Mock<ICalculationResultQueries> calculationResultQueriesMock,
        [Frozen] Mock<IClock> clockMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        completedCalculationRepositoryMock
            .SetupSequence(p => p.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedCalculation)
            .ReturnsAsync((CompletedCalculation)null!);

        calculationResultQueriesMock
            .Setup(queries => queries.GetAsync(completedCalculation.Id))
            .Returns(AsAsyncEnumerable<CalculationResult>());

        clockMock.Setup(c => c.GetCurrentInstant()).Returns(instant);

        // Act
        var unused = sut.GetAsync().ToListAsync();

        // Assert
        completedCalculation.PublishedTime.Should().Be(instant);
    }

    // TODO BJM: Redundant?
    [Theory]
    [InlineAutoMoqData]
    public void GetAsync_WhenMultipleUnpublishedCalculations_FetchesResultsForEach(
        CompletedCalculation completedCalculation,
        IntegrationEvent anyIntegrationEvent,
        CalculationResult calculationResult,
        [Frozen] Mock<ICompletedCalculationRepository> completedCalculationRepositoryMock,
        [Frozen] Mock<ICalculationResultQueries> calculationResultQueriesMock,
        [Frozen] Mock<ICalculationResultIntegrationEventFactory> calculationResultIntegrationEventFactoryMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        completedCalculationRepositoryMock
            .SetupSequence(p => p.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedCalculation)
            .ReturnsAsync(completedCalculation)
            .ReturnsAsync((CompletedCalculation)null!);

        calculationResultQueriesMock
            .Setup(queries => queries.GetAsync(completedCalculation.Id))
            .Returns(AsAsyncEnumerable(calculationResult));

        calculationResultIntegrationEventFactoryMock
            .Setup(factory => factory.Create(calculationResult))
            .Returns(anyIntegrationEvent);

        // Act
        var unused = sut.GetAsync().ToListAsync();

        // Assert: Fetches results once per unpublished calculation
        calculationResultQueriesMock.Verify(x => x.GetAsync(It.IsAny<Guid>()), Times.Exactly(2));
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenMultipleUnpublishedCalculations_ReturnsEachResult(
        CompletedCalculation completedCalculation1,
        CompletedCalculation completedCalculation2,
        CalculationResult calculationResult1,
        CalculationResult calculationResult2,
        CalculationResult calculationResult3,
        CalculationResult calculationResult4,
        IntegrationEvent integrationEvent1,
        IntegrationEvent integrationEvent2,
        IntegrationEvent integrationEvent3,
        IntegrationEvent integrationEvent4,
        [Frozen] Mock<ICompletedCalculationRepository> completedCalculationRepositoryMock,
        [Frozen] Mock<ICalculationResultIntegrationEventFactory> calculationResultIntegrationEventFactoryMock,
        [Frozen] Mock<ICalculationResultQueries> calculationResultQueriesMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        completedCalculationRepositoryMock
            .SetupSequence(p => p.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedCalculation1)
            .ReturnsAsync(completedCalculation2)
            .ReturnsAsync((CompletedCalculation)null!);

        calculationResultQueriesMock
            .Setup(queries => queries.GetAsync(completedCalculation1.Id))
            .Returns(AsAsyncEnumerable(calculationResult1, calculationResult2));
        calculationResultQueriesMock
            .Setup(queries => queries.GetAsync(completedCalculation2.Id))
            .Returns(AsAsyncEnumerable(calculationResult3, calculationResult4));

        calculationResultIntegrationEventFactoryMock
            .Setup(factory => factory.Create(calculationResult1))
            .Returns(integrationEvent1);
        calculationResultIntegrationEventFactoryMock
            .SetupSequence(factory => factory.Create(calculationResult2))
            .Returns(integrationEvent2);
        calculationResultIntegrationEventFactoryMock
            .Setup(factory => factory.Create(calculationResult3))
            .Returns(integrationEvent3);
        calculationResultIntegrationEventFactoryMock
            .SetupSequence(factory => factory.Create(calculationResult4))
            .Returns(integrationEvent4);

        // Act
        var actual = await sut.GetAsync().ToListAsync();

        // Assert
        actual.Should().BeEquivalentTo(new[] { integrationEvent1, integrationEvent2, integrationEvent3, integrationEvent4 });
    }

    private async IAsyncEnumerable<T> AsAsyncEnumerable<T>(params T[] items)
    {
        foreach (var item in items)
            yield return item;
        await Task.Delay(0);
    }
}
