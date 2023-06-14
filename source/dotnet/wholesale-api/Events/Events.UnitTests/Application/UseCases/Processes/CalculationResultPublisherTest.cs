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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.Actors;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.Actors.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Common.Models;
using Energinet.DataHub.Wholesale.Events.Application.CalculationResultPublishing;
using Energinet.DataHub.Wholesale.Events.Application.CalculationResultPublishing.Model;
using Energinet.DataHub.Wholesale.Events.Application.IntegrationEventsManagement;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Application.UseCases.Processes;

public class CalculationResultPublisherTest
{
    [Theory]
    [InlineAutoMoqData(TimeSeriesType.NonProfiledConsumption)]
    [InlineAutoMoqData(TimeSeriesType.FlexConsumption)]
    [InlineAutoMoqData(TimeSeriesType.Production)]
    [InlineAutoMoqData(TimeSeriesType.NetExchangePerGa)]
    [InlineAutoMoqData(TimeSeriesType.NetExchangePerNeighboringGa)]
    public async Task
        PublishAsync_PublishEventForGridArea(
            TimeSeriesType timeSeriesType,
            [Frozen] Mock<IActorClient> actorClientMock,
            [Frozen] Mock<ICalculationResultClient> calculationResultClientMock,
            [Frozen] Mock<IIntegrationEventPublisher> integrationEventPublisherMock,
            [Frozen] Mock<ICalculationResultCompletedFactory> calculationResultCompletedFactoryMock,
            IntegrationEventDto integrationEventDto,
            CalculationResultPublisher sut)
    {
        // Arrange
        var eventDto = new BatchGridAreaInfo(
            "805",
            Guid.NewGuid(),
            ProcessType.BalanceFixing,
            Instant.MinValue,
            Instant.MinValue);

        var calculationResult = new CalculationResult(
            timeSeriesType,
            new[] { new TimeSeriesPoint(DateTimeOffset.Now, 10.0m, QuantityQuality.Estimated) });

        calculationResultClientMock.Setup(p => p.GetAsync(
            eventDto.BatchId,
            It.IsAny<string>(),
            timeSeriesType,
            null,
            null)).ReturnsAsync(calculationResult);

        actorClientMock
            .Setup(a => a.GetEnergySuppliersAsync(
                eventDto.BatchId,
                eventDto.GridAreaCode,
                It.IsAny<TimeSeriesType>())).ReturnsAsync(Array.Empty<Actor>());

        calculationResultCompletedFactoryMock
            .Setup(c => c.CreateForTotalGridArea(
                calculationResult,
                eventDto))
            .Returns(integrationEventDto);

        // Act
        await sut.PublishForGridAreaAsync(eventDto);

        // Assert
        integrationEventPublisherMock.Verify(x => x.PublishAsync(integrationEventDto), Times.Once);
    }

    [Theory]
    [InlineAutoMoqData(TimeSeriesType.NonProfiledConsumption)]
    [InlineAutoMoqData(TimeSeriesType.FlexConsumption)]
    [InlineAutoMoqData(TimeSeriesType.Production)]
    public async Task PublishAsync_PublishEventForEnergySupplier(
        TimeSeriesType timeSeriesType,
        [Frozen] Mock<IActorClient> actorClientMock,
        [Frozen] Mock<ICalculationResultClient> calculationResultClientMock,
        [Frozen] Mock<IIntegrationEventPublisher> integrationEventPublisherMock,
        [Frozen] Mock<ICalculationResultCompletedFactory> calculationResultCompletedFactoryMock,
        IntegrationEventDto integrationEventDto,
        string glnNumber,
        CalculationResultPublisher sut)
    {
        // Arrange
        var eventDto = new BatchGridAreaInfo(
            "805",
            Guid.NewGuid(),
            ProcessType.BalanceFixing,
            Instant.MinValue,
            Instant.MinValue);

        var calculationResult = new CalculationResult(
            timeSeriesType,
            new[] { new TimeSeriesPoint(DateTimeOffset.Now, 10.0m, QuantityQuality.Estimated) });

        calculationResultClientMock.Setup(p => p.GetAsync(
            eventDto.BatchId,
            It.IsAny<string>(),
            timeSeriesType,
            glnNumber,
            null)).ReturnsAsync(calculationResult);

        actorClientMock
            .Setup(a => a.GetEnergySuppliersAsync(
                eventDto.BatchId,
                It.IsAny<string>(),
                It.IsAny<TimeSeriesType>())).ReturnsAsync(new[] { new Actor(glnNumber) });

        calculationResultCompletedFactoryMock
            .Setup(c => c.CreateForEnergySupplier(
                calculationResult,
                eventDto,
                glnNumber))
            .Returns(integrationEventDto);

        //Act
        await sut.PublishForGridAreaAsync(eventDto);

        // Assert
        integrationEventPublisherMock.Verify(x => x.PublishAsync(integrationEventDto), Times.Once);
    }

    [Theory]
    [InlineAutoMoqData(TimeSeriesType.NonProfiledConsumption)]
    [InlineAutoMoqData(TimeSeriesType.FlexConsumption)]
    [InlineAutoMoqData(TimeSeriesType.Production)]
    public async Task
        PublishAsyncPublishEventForBalanceResponsibleParty(
            TimeSeriesType timeSeriesType,
            [Frozen] Mock<IActorClient> actorClientMock,
            [Frozen] Mock<ICalculationResultClient> calculationResultClientMock,
            [Frozen] Mock<IIntegrationEventPublisher> integrationEventPublisherMock,
            [Frozen] Mock<ICalculationResultCompletedFactory> calculationResultCompletedFactoryMock,
            string brpGlnNumber,
            IntegrationEventDto integrationEventDto,
            CalculationResultPublisher sut)
    {
        // Arrange
        var eventDto = new BatchGridAreaInfo(
            "805",
            Guid.NewGuid(),
            ProcessType.BalanceFixing,
            Instant.MinValue,
            Instant.MinValue);

        var calculationResult = new CalculationResult(
            timeSeriesType,
            new[] { new TimeSeriesPoint(DateTimeOffset.Now, 10.0m, QuantityQuality.Estimated) });

        calculationResultClientMock.Setup(p => p.GetAsync(
            eventDto.BatchId,
            It.IsAny<string>(),
            timeSeriesType,
            null,
            brpGlnNumber)).ReturnsAsync(calculationResult);

        actorClientMock
            .Setup(a => a.GetBalanceResponsiblePartiesAsync(
                eventDto.BatchId,
                It.IsAny<string>(),
                It.IsAny<TimeSeriesType>())).ReturnsAsync(new[] { new Actor(brpGlnNumber) });

        calculationResultCompletedFactoryMock
            .Setup(c => c.CreateForBalanceResponsibleParty(calculationResult, eventDto, brpGlnNumber))
            .Returns(integrationEventDto);

        // Act
        await sut.PublishForGridAreaAsync(eventDto);

        // Assert
        integrationEventPublisherMock.Verify(x => x.PublishAsync(integrationEventDto), Times.Once);
    }

    [Theory]
    [InlineAutoMoqData(TimeSeriesType.NonProfiledConsumption)]
    [InlineAutoMoqData(TimeSeriesType.FlexConsumption)]
    [InlineAutoMoqData(TimeSeriesType.Production)]
    public async Task
        PublishAsync_PublishEventForEnergySupplierByBalanceResponsibleParty(
            TimeSeriesType timeSeriesType,
            [Frozen] Mock<IActorClient> actorClientMock,
            [Frozen] Mock<ICalculationResultClient> calculationResultClientMock,
            [Frozen] Mock<IIntegrationEventPublisher> integrationEventPublisherMock,
            [Frozen] Mock<ICalculationResultCompletedFactory> calculationResultCompletedFactoryMock,
            IntegrationEventDto integrationEventDto,
            string brpGlnNumber,
            string glnNumber,
            CalculationResultPublisher sut)
    {
        // Arrange
        var eventDto = new BatchGridAreaInfo(
            "805",
            Guid.NewGuid(),
            ProcessType.BalanceFixing,
            Instant.MinValue,
            Instant.MinValue);

        var calculationResult = new CalculationResult(
            timeSeriesType,
            new[] { new TimeSeriesPoint(DateTimeOffset.Now, 10.0m, QuantityQuality.Estimated) });

        calculationResultClientMock.Setup(p => p.GetAsync(
            eventDto.BatchId,
            It.IsAny<string>(),
            timeSeriesType,
            glnNumber,
            brpGlnNumber)).ReturnsAsync(calculationResult);

        actorClientMock
            .Setup(a => a.GetBalanceResponsiblePartiesAsync(
                eventDto.BatchId,
                It.IsAny<string>(),
                It.IsAny<TimeSeriesType>())).ReturnsAsync(new[] { new Actor(brpGlnNumber) });

        actorClientMock
            .Setup(a => a.GetEnergySuppliersByBalanceResponsiblePartyAsync(
                eventDto.BatchId,
                It.IsAny<string>(),
                It.IsAny<TimeSeriesType>(),
                brpGlnNumber)).ReturnsAsync(new[] { new Actor(glnNumber) });

        calculationResultCompletedFactoryMock
            .Setup(c => c.CreateForEnergySupplierByBalanceResponsibleParty(
                calculationResult,
                eventDto,
                glnNumber,
                brpGlnNumber))
            .Returns(integrationEventDto);

        // Act
        await sut.PublishForGridAreaAsync(eventDto);

        // Assert
        integrationEventPublisherMock.Verify(x => x.PublishAsync(integrationEventDto), Times.Once);
    }
}
