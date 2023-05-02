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
using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Application.IntegrationEventsManagement;
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.ActorAggregate;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Moq;
using NodaTime;
using Xunit;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Application.Processes;

public class ProcessApplicationServiceTest
{
    [Theory]
    [InlineAutoMoqData]
    public async Task PublishProcessCompletedEventsAsync_Publishes(
        BatchCompletedEventDto batchCompleted,
        [Frozen] Mock<IDomainEventPublisher> publisherMock,
        ProcessApplicationService sut)
    {
        await sut.PublishProcessCompletedEventsAsync(batchCompleted);
        publisherMock.Verify(publisher => publisher.PublishAsync(It.IsAny<List<ProcessCompletedEventDto>>()), Times.Once);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task
        PublishCalculationResultReadyIntegrationEventsAsync_WhenCalled_PublishEventForTotalGridAreaProductionOnce(
            [Frozen] Mock<IActorRepository> actorRepositoryMock,
            [Frozen] Mock<IProcessStepResultRepository> processStepResultRepositoryMock,
            [Frozen] Mock<IIntegrationEventPublisher> integrationEventPublisherMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            [Frozen] Mock<ICalculationResultCompletedFactory> calculationResultCompletedFactoryMock,
            IntegrationEventDto integrationEventDto,
            ProcessApplicationService sut)
    {
        // Arrange
        var eventDto = new ProcessCompletedEventDto(
            "805",
            Guid.NewGuid(),
            ProcessType.BalanceFixing,
            Instant.MinValue,
            Instant.MinValue);

        var processStepResult = new ProcessStepResult(
            TimeSeriesType.Production,
            new[] { new TimeSeriesPoint(DateTimeOffset.Now, 10.0m, QuantityQuality.Estimated) });

        processStepResultRepositoryMock.Setup(p => p.GetAsync(
            eventDto.BatchId,
            It.IsAny<GridAreaCode>(),
            TimeSeriesType.Production,
            null,
            null)).ReturnsAsync(processStepResult);

        actorRepositoryMock
            .Setup(a => a.GetEnergySuppliersAsync(
                eventDto.BatchId,
                new GridAreaCode(eventDto.GridAreaCode),
                It.IsAny<TimeSeriesType>())).ReturnsAsync(Array.Empty<Actor>());

        calculationResultCompletedFactoryMock
            .Setup(c => c.CreateForTotalGridArea(
                processStepResult,
                eventDto))
            .Returns(integrationEventDto);

        // Act
        await sut.PublishCalculationResultCompletedIntegrationEventsAsync(eventDto);

        // Assert
        integrationEventPublisherMock.Verify(x => x.PublishAsync(integrationEventDto), Times.Once);
        unitOfWorkMock.Verify(x => x.CommitAsync());
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task
        PublishCalculationResultReadyIntegrationEventsAsync_WhenCalled_PublishEventForTotalGridAreaNonProfiledConsumptionOnce(
            [Frozen] Mock<IActorRepository> actorRepositoryMock,
            [Frozen] Mock<IProcessStepResultRepository> processStepResultRepositoryMock,
            [Frozen] Mock<IIntegrationEventPublisher> integrationEventPublisherMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            [Frozen] Mock<ICalculationResultCompletedFactory> calculationResultCompletedFactoryMock,
            IntegrationEventDto integrationEventDto,
            ProcessApplicationService sut)
    {
        // Arrange
        var eventDto = new ProcessCompletedEventDto(
            "805",
            Guid.NewGuid(),
            ProcessType.BalanceFixing,
            Instant.MinValue,
            Instant.MinValue);

        var processStepResult = new ProcessStepResult(
            TimeSeriesType.Production,
            new[] { new TimeSeriesPoint(DateTimeOffset.Now, 10.0m, QuantityQuality.Estimated) });

        processStepResultRepositoryMock.Setup(p => p.GetAsync(
            eventDto.BatchId,
            It.IsAny<GridAreaCode>(),
            TimeSeriesType.NonProfiledConsumption,
            null,
            null)).ReturnsAsync(processStepResult);

        actorRepositoryMock
            .Setup(a => a.GetEnergySuppliersAsync(
                eventDto.BatchId,
                new GridAreaCode(eventDto.GridAreaCode),
                It.IsAny<TimeSeriesType>())).ReturnsAsync(Array.Empty<Actor>());

        calculationResultCompletedFactoryMock
            .Setup(c => c.CreateForTotalGridArea(
                processStepResult,
                eventDto))
            .Returns(integrationEventDto);

        // Act
        await sut.PublishCalculationResultCompletedIntegrationEventsAsync(eventDto);

        // Assert
        integrationEventPublisherMock.Verify(x => x.PublishAsync(integrationEventDto), Times.Once);
        unitOfWorkMock.Verify(x => x.CommitAsync());
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task PublishCalculationResultReadyIntegrationEventsAsync_WhenCalled_PublishEventForEnergySupplier(
        [Frozen] Mock<IActorRepository> actorRepositoryMock,
        [Frozen] Mock<IProcessStepResultRepository> processStepResultRepositoryMock,
        [Frozen] Mock<IIntegrationEventPublisher> integrationEventPublisherMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<ICalculationResultCompletedFactory> calculationResultCompletedFactoryMock,
        IntegrationEventDto integrationEventDto,
        string glnNumber,
        ProcessApplicationService sut)
    {
        // Arrange
        var eventDto = new ProcessCompletedEventDto(
            "805",
            Guid.NewGuid(),
            ProcessType.BalanceFixing,
            Instant.MinValue,
            Instant.MinValue);

        var processStepResult = new ProcessStepResult(
            TimeSeriesType.NonProfiledConsumption,
            new[] { new TimeSeriesPoint(DateTimeOffset.Now, 10.0m, QuantityQuality.Estimated) });

        processStepResultRepositoryMock.Setup(p => p.GetAsync(
            eventDto.BatchId,
            It.IsAny<GridAreaCode>(),
            TimeSeriesType.NonProfiledConsumption,
            glnNumber,
            null)).ReturnsAsync(processStepResult);

        actorRepositoryMock
            .Setup(a => a.GetEnergySuppliersAsync(
                eventDto.BatchId,
                It.IsAny<GridAreaCode>(),
                It.IsAny<TimeSeriesType>())).ReturnsAsync(new[] { new Actor(glnNumber) });

        calculationResultCompletedFactoryMock
            .Setup(c => c.CreateForEnergySupplier(
                processStepResult,
                eventDto,
                glnNumber))
            .Returns(integrationEventDto);

        //Act
        await sut.PublishCalculationResultCompletedIntegrationEventsAsync(eventDto);

        // Assert
        integrationEventPublisherMock.Verify(x => x.PublishAsync(integrationEventDto), Times.Once);
        unitOfWorkMock.Verify(x => x.CommitAsync());
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task
        PublishCalculationResultReadyIntegrationEventsAsync_WhenCalled_PublishEventForBalanceResponsibleParty(
            [Frozen] Mock<IActorRepository> actorRepositoryMock,
            [Frozen] Mock<IProcessStepResultRepository> processStepResultRepositoryMock,
            [Frozen] Mock<IIntegrationEventPublisher> integrationEventPublisherMock,
            [Frozen] Mock<ICalculationResultCompletedFactory> calculationResultCompletedFactoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            string brpGlnNumber,
            IntegrationEventDto integrationEventDto,
            ProcessApplicationService sut)
    {
        // Arrange
        var eventDto = new ProcessCompletedEventDto(
            "805",
            Guid.NewGuid(),
            ProcessType.BalanceFixing,
            Instant.MinValue,
            Instant.MinValue);

        var processStepResult = new ProcessStepResult(
            TimeSeriesType.NonProfiledConsumption,
            new[] { new TimeSeriesPoint(DateTimeOffset.Now, 10.0m, QuantityQuality.Estimated) });

        processStepResultRepositoryMock.Setup(p => p.GetAsync(
            eventDto.BatchId,
            It.IsAny<GridAreaCode>(),
            TimeSeriesType.NonProfiledConsumption,
            null,
            brpGlnNumber)).ReturnsAsync(processStepResult);

        actorRepositoryMock
            .Setup(a => a.GetBalanceResponsiblePartiesAsync(
                eventDto.BatchId,
                It.IsAny<GridAreaCode>(),
                It.IsAny<TimeSeriesType>())).ReturnsAsync(new[] { new Actor(brpGlnNumber) });

        calculationResultCompletedFactoryMock
            .Setup(c => c.CreateForBalanceResponsibleParty(processStepResult, eventDto, brpGlnNumber))
            .Returns(integrationEventDto);

        // Act
        await sut.PublishCalculationResultCompletedIntegrationEventsAsync(eventDto);

        // Assert
        integrationEventPublisherMock.Verify(x => x.PublishAsync(integrationEventDto), Times.Once);
        unitOfWorkMock.Verify(x => x.CommitAsync());
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task
        PublishCalculationResultReadyIntegrationEventsAsync_WhenCalled_PublishEventForEnergySupplierByBalanceResponsibleParty(
            [Frozen] Mock<IActorRepository> actorRepositoryMock,
            [Frozen] Mock<IProcessStepResultRepository> processStepResultRepositoryMock,
            [Frozen] Mock<IIntegrationEventPublisher> integrationEventPublisherMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            [Frozen] Mock<ICalculationResultCompletedFactory> calculationResultCompletedFactoryMock,
            IntegrationEventDto integrationEventDto,
            string brpGlnNumber,
            string glnNumber,
            ProcessApplicationService sut)
    {
        // Arrange
        var eventDto = new ProcessCompletedEventDto(
            "805",
            Guid.NewGuid(),
            ProcessType.BalanceFixing,
            Instant.MinValue,
            Instant.MinValue);

        var processStepResult = new ProcessStepResult(
            TimeSeriesType.NonProfiledConsumption,
            new[] { new TimeSeriesPoint(DateTimeOffset.Now, 10.0m, QuantityQuality.Estimated) });

        processStepResultRepositoryMock.Setup(p => p.GetAsync(
            eventDto.BatchId,
            It.IsAny<GridAreaCode>(),
            TimeSeriesType.NonProfiledConsumption,
            glnNumber,
            brpGlnNumber)).ReturnsAsync(processStepResult);

        actorRepositoryMock
            .Setup(a => a.GetBalanceResponsiblePartiesAsync(
                eventDto.BatchId,
                It.IsAny<GridAreaCode>(),
                It.IsAny<TimeSeriesType>())).ReturnsAsync(new[] { new Actor(brpGlnNumber) });

        actorRepositoryMock
            .Setup(a => a.GetEnergySuppliersByBalanceResponsiblePartyAsync(
                eventDto.BatchId,
                It.IsAny<GridAreaCode>(),
                It.IsAny<TimeSeriesType>(),
                brpGlnNumber)).ReturnsAsync(new[] { new Actor(glnNumber) });

        calculationResultCompletedFactoryMock
            .Setup(c => c.CreateForEnergySupplierByBalanceResponsibleParty(
                processStepResult,
                eventDto,
                glnNumber,
                brpGlnNumber))
            .Returns(integrationEventDto);

        // Act
        await sut.PublishCalculationResultCompletedIntegrationEventsAsync(eventDto);

        // Assert
        integrationEventPublisherMock.Verify(x => x.PublishAsync(integrationEventDto), Times.Once);
        unitOfWorkMock.Verify(x => x.CommitAsync());
    }
}
