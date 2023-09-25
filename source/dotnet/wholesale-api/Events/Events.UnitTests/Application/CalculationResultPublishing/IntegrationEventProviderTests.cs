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
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.Messaging.Communication;
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
using Test.Core.Attributes;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Application.CalculationResultPublishing;

public class IntegrationEventProviderTests
{
    [Theory]
    [InlineAutoMoqData]
    public void GetAsync_WhenNoUnpublishedBatches_DoesNotCommit(
        [Frozen] Mock<ICompletedBatchRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        completedBatchRepositoryMock
            .Setup(p => p.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync((CompletedBatch)null!);

        // Act
        var unused = sut.GetAsync().ToListAsync();

        // Assert
        unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
    }

    [Theory]
    [InlineAutoMoqData]
    public void GetAsync_WhenMultipleUnpublishedBatches_CommitsOncePerBatch(
        CompletedBatch completedBatch,
        EnergyResult energyResult,
        IntegrationEvent anyIntegrationEvent,
        [Frozen] Mock<ICompletedBatchRepository> completedBatchRepositoryMock,
        [Frozen] Mock<ICalculationResultQueries> calculationResultQueriesMock,
        [Frozen] Mock<ICalculationResultIntegrationEventFactory> calculationResultIntegrationEventFactoryMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        completedBatchRepositoryMock
            .SetupSequence(p => p.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedBatch)
            .ReturnsAsync(completedBatch)
            .ReturnsAsync((CompletedBatch)null!);

        calculationResultQueriesMock
            .Setup(queries => queries.GetAsync(completedBatch.Id))
            .Returns(AsAsyncEnumerable(energyResult));

        calculationResultIntegrationEventFactoryMock
            .Setup(factory => factory.CreateCalculationResultCompleted(energyResult))
            .Returns(anyIntegrationEvent);

        // Act
        var unused = sut.GetAsync().ToListAsync();

        // Assert: Commits once per unpublished batch
        unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Exactly(2));
    }

    [Theory]
    [InlineAutoMoqData]
    public void GetAsync_SetsPublishedTimeOfBatch(
        Instant instant,
        CompletedBatch completedBatch,
        [Frozen] Mock<ICompletedBatchRepository> completedBatchRepositoryMock,
        [Frozen] Mock<ICalculationResultQueries> calculationResultQueriesMock,
        [Frozen] Mock<IClock> clockMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        completedBatchRepositoryMock
            .SetupSequence(p => p.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedBatch)
            .ReturnsAsync((CompletedBatch)null!);

        calculationResultQueriesMock
            .Setup(queries => queries.GetAsync(completedBatch.Id))
            .Returns(AsAsyncEnumerable<EnergyResult>());

        clockMock.Setup(c => c.GetCurrentInstant()).Returns(instant);

        // Act
        var unused = sut.GetAsync().ToListAsync();

        // Assert
        completedBatch.PublishedTime.Should().Be(instant);
    }

    [Theory]
    [InlineAutoMoqData]
    public void GetAsync_WhenMultipleUnpublishedBatches_FetchesResultsForEach(
        CompletedBatch completedBatch,
        IntegrationEvent anyIntegrationEvent,
        EnergyResult energyResult,
        [Frozen] Mock<ICompletedBatchRepository> completedBatchRepositoryMock,
        [Frozen] Mock<ICalculationResultQueries> calculationResultQueriesMock,
        [Frozen] Mock<ICalculationResultIntegrationEventFactory> calculationResultIntegrationEventFactoryMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        completedBatchRepositoryMock
            .SetupSequence(p => p.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedBatch)
            .ReturnsAsync(completedBatch)
            .ReturnsAsync((CompletedBatch)null!);

        calculationResultQueriesMock
            .Setup(queries => queries.GetAsync(completedBatch.Id))
            .Returns(AsAsyncEnumerable(energyResult));

        calculationResultIntegrationEventFactoryMock
            .Setup(factory => factory.CreateCalculationResultCompleted(energyResult))
            .Returns(anyIntegrationEvent);

        // Act
        var unused = sut.GetAsync().ToListAsync();

        // Assert: Fetches results once per unpublished batch
        calculationResultQueriesMock.Verify(x => x.GetAsync(It.IsAny<Guid>()), Times.Exactly(2));
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenMultipleUnpublishedBatches_ReturnsEachResult(
        CompletedBatch completedBatch1,
        CompletedBatch completedBatch2,
        EnergyResult energyResult1,
        EnergyResult energyResult2,
        EnergyResult energyResult3,
        EnergyResult energyResult4,
        IntegrationEvent integrationEvent1,
        IntegrationEvent integrationEvent2,
        IntegrationEvent integrationEvent3,
        IntegrationEvent integrationEvent4,
        [Frozen] Mock<ICompletedBatchRepository> completedBatchRepositoryMock,
        [Frozen] Mock<ICalculationResultIntegrationEventFactory> calculationResultIntegrationEventFactoryMock,
        [Frozen] Mock<ICalculationResultQueries> calculationResultQueriesMock,
        IntegrationEventProvider sut)
    {
        // Arrange
        completedBatchRepositoryMock
            .SetupSequence(p => p.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedBatch1)
            .ReturnsAsync(completedBatch2)
            .ReturnsAsync((CompletedBatch)null!);

        calculationResultQueriesMock
            .Setup(queries => queries.GetAsync(completedBatch1.Id))
            .Returns(AsAsyncEnumerable(energyResult1, energyResult2));
        calculationResultQueriesMock
            .Setup(queries => queries.GetAsync(completedBatch2.Id))
            .Returns(AsAsyncEnumerable(energyResult3, energyResult4));

        calculationResultIntegrationEventFactoryMock
            .Setup(factory => factory.CreateCalculationResultCompleted(energyResult1))
            .Returns(integrationEvent1);
        calculationResultIntegrationEventFactoryMock
            .SetupSequence(factory => factory.CreateCalculationResultCompleted(energyResult2))
            .Returns(integrationEvent2);
        calculationResultIntegrationEventFactoryMock
            .Setup(factory => factory.CreateCalculationResultCompleted(energyResult3))
            .Returns(integrationEvent3);
        calculationResultIntegrationEventFactoryMock
            .SetupSequence(factory => factory.CreateCalculationResultCompleted(energyResult4))
            .Returns(integrationEvent4);

        // Act
        var actual = await sut.GetAsync().ToListAsync();

        // Assert
        actual.Should().BeEquivalentTo(new[] { integrationEvent1, integrationEvent2, integrationEvent3, integrationEvent4 });
    }

    [Fact]
    [AcceptanceTest]
    public void AProvider_MustImplement_IIntegrationEventProvider()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(Energinet.DataHub.Wholesale.Events.Application.Root));
        var interfaceType = typeof(IIntegrationEventProvider);

        // Act
        var actualImplementations = assembly!.GetTypes()
            .Where(type => interfaceType.IsAssignableFrom(type) && !type.IsInterface)
            .ToList();

        // Assert
        Assert.True(actualImplementations.Count == 1, $"The interface {nameof(IIntegrationEventProvider)} must be implemented.");
    }

    private async IAsyncEnumerable<T> AsAsyncEnumerable<T>(params T[] items)
    {
        foreach (var item in items)
            yield return item;
        await Task.Delay(0);
    }
}
