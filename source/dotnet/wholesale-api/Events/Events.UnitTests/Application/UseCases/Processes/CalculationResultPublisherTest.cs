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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Events.Application.CalculationResultPublishing;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using Energinet.DataHub.Wholesale.Events.Application.IntegrationEventsManagement;
using Energinet.DataHub.Wholesale.Events.UnitTests.Fixtures;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Application.UseCases.Processes;

public class CalculationResultPublisherTest
{
    [Theory]
    [InlineAutoMoqData]
    public async Task
        PublishAsync_PublishEventForGridArea(
            CompletedBatch completedBatch,
            CalculationResultBuilder calculationResultBuilder,
            [Frozen] Mock<ICalculationResultQueries> calculationResultQueriesMock,
            [Frozen] Mock<IIntegrationEventPublisher> integrationEventPublisherMock,
            [Frozen] Mock<ICalculationResultCompletedFactory> calculationResultCompletedFactoryMock,
            IntegrationEventDto integrationEventDto,
            CalculationResultPublisher sut)
    {
        // Arrange
        var calculationResult = calculationResultBuilder.WithId(completedBatch.Id).Build();

        calculationResultQueriesMock
            .Setup(p => p.GetAsync(completedBatch.Id))
            .Returns(ResultAsyncEnumerable(calculationResult));

        calculationResultCompletedFactoryMock
            .Setup(c => c.CreateForTotalGridArea(calculationResult))
            .Returns(integrationEventDto);

        // Act
        await sut.PublishForBatchAsync(completedBatch);

        // Assert
        integrationEventPublisherMock.Verify(x => x.PublishAsync(integrationEventDto), Times.Once);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task PublishAsync_PublishEventForEnergySupplier(
        CompletedBatch completedBatch,
        CalculationResultBuilder calculationResultBuilder,
        [Frozen] Mock<ICalculationResultQueries> calculationResultQueriesMock,
        [Frozen] Mock<IIntegrationEventPublisher> integrationEventPublisherMock,
        [Frozen] Mock<ICalculationResultCompletedFactory> calculationResultCompletedFactoryMock,
        IntegrationEventDto integrationEventDto,
        CalculationResultPublisher sut)
    {
        // Arrange
        var calculationResult = calculationResultBuilder
            .WithId(completedBatch.Id)
            .WithEnergySupplier()
            .Build();

        calculationResultQueriesMock
            .Setup(p => p.GetAsync(completedBatch.Id))
            .Returns(ResultAsyncEnumerable(calculationResult));

        calculationResultCompletedFactoryMock
            .Setup(c => c.CreateForEnergySupplier(
                calculationResult))
            .Returns(integrationEventDto);

        //Act
        await sut.PublishForBatchAsync(completedBatch);

        // Assert
        integrationEventPublisherMock.Verify(x => x.PublishAsync(integrationEventDto), Times.Once);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task
        PublishAsyncPublishEventForBalanceResponsibleParty(
            CompletedBatch completedBatch,
            CalculationResultBuilder calculationResultBuilder,
            [Frozen] Mock<ICalculationResultQueries> calculationResultQueriesMock,
            [Frozen] Mock<IIntegrationEventPublisher> integrationEventPublisherMock,
            [Frozen] Mock<ICalculationResultCompletedFactory> calculationResultCompletedFactoryMock,
            IntegrationEventDto integrationEventDto,
            CalculationResultPublisher sut)
    {
        // Arrange
        var calculationResult = calculationResultBuilder
            .WithId(completedBatch.Id)
            .WithBalanceResponsibleParty()
            .Build();

        calculationResultQueriesMock
            .Setup(p => p.GetAsync(completedBatch.Id))
            .Returns(ResultAsyncEnumerable(calculationResult));

        calculationResultCompletedFactoryMock
            .Setup(c => c.CreateForBalanceResponsibleParty(calculationResult))
            .Returns(integrationEventDto);

        // Act
        await sut.PublishForBatchAsync(completedBatch);

        // Assert
        integrationEventPublisherMock.Verify(x => x.PublishAsync(integrationEventDto), Times.Once);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task
        PublishAsync_PublishEventForEnergySupplierByBalanceResponsibleParty(
            CompletedBatch completedBatch,
            CalculationResultBuilder calculationResultBuilder,
            [Frozen] Mock<ICalculationResultQueries> calculationResultQueriesMock,
            [Frozen] Mock<IIntegrationEventPublisher> integrationEventPublisherMock,
            [Frozen] Mock<ICalculationResultCompletedFactory> calculationResultCompletedFactoryMock,
            IntegrationEventDto integrationEventDto,
            CalculationResultPublisher sut)
    {
        // Arrange
        var calculationResult = calculationResultBuilder
            .WithId(completedBatch.Id)
            .WithEnergySupplier()
            .WithBalanceResponsibleParty()
            .Build();

        calculationResultQueriesMock
            .Setup(p => p.GetAsync(completedBatch.Id))
            .Returns(ResultAsyncEnumerable(calculationResult));

        calculationResultCompletedFactoryMock
            .Setup(c => c.CreateForEnergySupplierByBalanceResponsibleParty(
                calculationResult))
            .Returns(integrationEventDto);

        // Act
        await sut.PublishForBatchAsync(completedBatch);

        // Assert
        integrationEventPublisherMock.Verify(x => x.PublishAsync(integrationEventDto), Times.Once);
    }

    private async IAsyncEnumerable<CalculationResult> ResultAsyncEnumerable(CalculationResult calculationResult)
    {
        yield return calculationResult;
        await Task.Delay(0);
    }
}
