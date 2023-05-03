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
using Energinet.DataHub.Wholesale.CalculationResults.Application;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.ProcessStep.Model;
using FluentAssertions;
using Moq;
using Test.Core;
using Xunit;
using Xunit.Categories;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Tests.Application.ProcessStep;

[UnitTest]
public class ProcessStepApplicationServiceTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task GetEnergySuppliersAsync_WhenNoActors_ReturnsEmptyCollection(
        Guid batchId,
        [Frozen] Mock<IActorRepository> actorRepositoryMock,
        ProcessStepApplicationService sut)
    {
        // Arrange
        const string gridAreaCode = "805";
        const Interfaces.ProcessStep.Model.TimeSeriesType timeSeriesType = Interfaces.ProcessStep.Model.TimeSeriesType.Production;

        actorRepositoryMock
            .Setup(x => x.GetEnergySuppliersAsync(
                batchId,
                gridAreaCode,
                TimeSeriesType.Production)).ReturnsAsync(Array.Empty<Interfaces.Actor>());

        // Act
        var actors = await sut.GetEnergySuppliersAsync(batchId, gridAreaCode, timeSeriesType);

        // Assert
        actors.Should().BeEmpty();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetBalanceResponsiblePartiesAsync_WhenNoActors_ReturnsEmptyCollection(
        Guid batchId,
        [Frozen] Mock<IActorRepository> actorRepositoryMock,
        ProcessStepApplicationService sut)
    {
        // Arrange
        const string gridAreaCode = "805";
        const Interfaces.ProcessStep.Model.TimeSeriesType timeSeriesType = Interfaces.ProcessStep.Model.TimeSeriesType.Production;

        actorRepositoryMock
            .Setup(x => x.GetBalanceResponsiblePartiesAsync(
                batchId,
                gridAreaCode,
                TimeSeriesType.Production)).ReturnsAsync(Array.Empty<Interfaces.Actor>());

        // Act
        var actors = await sut.GetBalanceResponsiblePartiesAsync(batchId, gridAreaCode, timeSeriesType);

        // Assert
        actors.Should().BeEmpty();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetEnergySuppliersAsync_ReturnsExpectedGlns(
        Guid batchId,
        [Frozen] Mock<IActorRepository> actorRepositoryMock,
        ProcessStepApplicationService sut)
    {
        // Arrange
        const string gridAreaCode = "805";
        const Interfaces.ProcessStep.Model.TimeSeriesType timeSeriesType = Interfaces.ProcessStep.Model.TimeSeriesType.Production;
        const string expectedGlnNumber = "ExpectedGlnNumber";
        actorRepositoryMock
            .Setup(x => x.GetEnergySuppliersAsync(
                batchId,
                gridAreaCode,
                TimeSeriesType.Production)).ReturnsAsync(new Interfaces.Actor[] { new(expectedGlnNumber) });

        // Act
        var actors = await sut.GetEnergySuppliersAsync(batchId, gridAreaCode, timeSeriesType);

        // Assert
        actors.Single().Gln.Should().Be(expectedGlnNumber);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetBalanceResponsiblePartiesAsync_ReturnsExpectedGlns(
        Guid batchId,
        [Frozen] Mock<IActorRepository> actorRepositoryMock,
        ProcessStepApplicationService sut)
    {
        // Arrange
        const string gridAreaCode = "805";
        const Interfaces.ProcessStep.Model.TimeSeriesType timeSeriesType = Interfaces.ProcessStep.Model.TimeSeriesType.Production;
        const string expectedGlnNumber = "ExpectedGlnNumber";
        actorRepositoryMock
            .Setup(x => x.GetBalanceResponsiblePartiesAsync(
                batchId,
                gridAreaCode,
                TimeSeriesType.Production)).ReturnsAsync(new Interfaces.Actor[] { new(expectedGlnNumber) });

        // Act
        var actors = await sut.GetBalanceResponsiblePartiesAsync(batchId, gridAreaCode, timeSeriesType);

        // Assert
        actors.Single().Gln.Should().Be(expectedGlnNumber);
    }

    [Theory]
    [AutoMoqData]
    public async Task GetResultAsync_TimeSeriesPoint_IsRead(
        [Frozen] Mock<IProcessStepResultRepository> processActorResultRepositoryMock,
        [Frozen] Mock<IProcessStepResultRepository> processStepResultRepositoryMock,
        [Frozen] Mock<IActorRepository> actorRepositoryMock)
    {
        // Arrange
        var time = new DateTimeOffset(2022, 05, 15, 22, 15, 0, TimeSpan.Zero);
        var quantity = 1.000m;
        var quality = QuantityQuality.Measured;

        const string gridAreaCode = "805";
        var batchId = Guid.NewGuid();

        var sut = new ProcessStepApplicationService(
            processStepResultRepositoryMock.Object,
            new ProcessStepResultMapper(),
            actorRepositoryMock.Object);

        processActorResultRepositoryMock.Setup(p => p.GetAsync(batchId, gridAreaCode, TimeSeriesType.Production, null, null))
            .ReturnsAsync(new ProcessStepResult(TimeSeriesType.Production, new[] { new TimeSeriesPoint(time, quantity, quality) }));

        // Act
        var actual = await sut.GetResultAsync(
            batchId,
            gridAreaCode,
            Interfaces.ProcessStep.Model.TimeSeriesType.Production,
            null,
            null);

        // Assert
        actual.TimeSeriesPoints.First().Time.Should().Be(time);
        actual.TimeSeriesPoints.First().Quantity.Should().Be(quantity);
        actual.TimeSeriesPoints.First().Quality.Should().Be(quality.ToString());
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetResultAsync_ReturnsDto(
        ProcessStepResultRequestDto request,
        ProcessStepResult result,
        ProcessStepResultDto resultDto,
        [Frozen] Mock<IProcessStepResultRepository> repositoryMock,
        [Frozen] Mock<IProcessStepResultMapper> mapperMock,
        ProcessStepApplicationService sut)
    {
        // Arrange
        request.SetPrivateProperty(dto => dto.GridAreaCode, "123");
        repositoryMock
            .Setup(repository => repository.GetAsync(request.BatchId, request.GridAreaCode, TimeSeriesType.Production, null, null))
            .ReturnsAsync(() => result);
        mapperMock
            .Setup(mapper => mapper.MapToDto(result))
            .Returns(() => resultDto);

        // Act
        var actual = await sut.GetResultAsync(request.BatchId, request.GridAreaCode, Interfaces.ProcessStep.Model.TimeSeriesType.Production, null, null);

        actual.Should().BeEquivalentTo(resultDto);
    }
}
