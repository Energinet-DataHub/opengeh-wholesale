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
using Energinet.DataHub.Wholesale.Application.ProcessStep;
using Energinet.DataHub.Wholesale.Application.ProcessStep.Model;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.ActorAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using FluentAssertions;
using Moq;
using Test.Core;
using Xunit;
using Xunit.Categories;
using Actor = Energinet.DataHub.Wholesale.Domain.ActorAggregate.Actor;
using MarketRole = Energinet.DataHub.Wholesale.Domain.ActorAggregate.MarketRole;
using TimeSeriesType = Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Tests.Application.ProcessStep;

[UnitTest]
public class ProcessStepApplicationServiceTests
{
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
        var quality = "A04";

        const string gridAreaCode = "805";
        var batchId = Guid.NewGuid();

        var sut = new ProcessStepApplicationService(
            processStepResultRepositoryMock.Object,
            new ProcessStepResultMapper(),
            actorRepositoryMock.Object);

        processActorResultRepositoryMock.Setup(p => p.GetAsync(batchId, new GridAreaCode(gridAreaCode), TimeSeriesType.Production, "grid_area"))
            .ReturnsAsync(new ProcessStepResult(new[] { new TimeSeriesPoint(time, quantity, quality) }));

        // Act
        var actual = await sut.GetResultAsync(
            new ProcessStepResultRequestDto(
                batchId,
                gridAreaCode,
                ProcessStepType.AggregateProductionPerGridArea));

        // Assert
        actual.TimeSeriesPoints.First().Time.Should().Be(time);
        actual.TimeSeriesPoints.First().Quantity.Should().Be(quantity);
        actual.TimeSeriesPoints.First().Quality.Should().Be(quality);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetActorsAsync_WhenNoActors_ReturnsEmptyCollection(
        Guid batchId,
        [Frozen] Mock<IActorRepository> actorRepositoryMock,
        ProcessStepApplicationService sut)
    {
        // Arrange
        var actorsRequest = new ProcessStepActorsRequest(
            batchId,
            "805",
            Contracts.TimeSeriesType.Production,
            Contracts.MarketRole.EnergySupplier);

        actorRepositoryMock
            .Setup(x => x.GetAsync(
                actorsRequest.BatchId,
                new GridAreaCode(actorsRequest.GridAreaCode),
                TimeSeriesType.Production,
                MarketRole.EnergySupplier)).ReturnsAsync(new Actor[] { });

        // Act
        var actors = await sut.GetActorsAsync(actorsRequest);

        // Assert
        actors.Should().BeEmpty();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetActorsAsync_ReturnsActors(
        Guid batchId,
        [Frozen] Mock<IActorRepository> actorRepositoryMock,
        ProcessStepApplicationService sut)
    {
        // Arrange
        var actorsRequest = new ProcessStepActorsRequest(
            batchId,
            "805",
            Contracts.TimeSeriesType.Production,
            Contracts.MarketRole.EnergySupplier);

        var expectedGlnNumber = "ExpectedGlnNumber";
        actorRepositoryMock
            .Setup(x => x.GetAsync(
                actorsRequest.BatchId,
                new GridAreaCode(actorsRequest.GridAreaCode),
                TimeSeriesType.Production,
                MarketRole.EnergySupplier)).ReturnsAsync(new Actor[] { new(expectedGlnNumber) });

        // Act
        var actors = await sut.GetActorsAsync(actorsRequest);

        // Assert
        actors.Single().Gln.Should().Be(expectedGlnNumber);
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
            .Setup(repository => repository.GetAsync(request.BatchId, new GridAreaCode(request.GridAreaCode), TimeSeriesType.Production, "grid_area"))
            .ReturnsAsync(() => result);
        mapperMock
            .Setup(mapper => mapper.MapToDto(result))
            .Returns(() => resultDto);

        // Act
        var actual = await sut.GetResultAsync(request);

        actual.Should().BeEquivalentTo(resultDto);
    }
}
