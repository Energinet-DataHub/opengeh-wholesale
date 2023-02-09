﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.BatchExecutionStateDomainService;
using Energinet.DataHub.Wholesale.Domain.CalculationDomainService;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Tests.Domain.BatchAggregate;
using FluentAssertions;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;
using ProcessType = Energinet.DataHub.Wholesale.Domain.ProcessAggregate.ProcessType;

namespace Energinet.DataHub.Wholesale.Tests.Application.Batches;

[UnitTest]
public class BatchApplicationServiceTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task SearchAsync_NoMatchingBatches_ReturnsZeroBatches(
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        BatchApplicationService sut)
    {
        // Arrange
        var noBatches = new List<Batch>();
        batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Instant>(), It.IsAny<Instant>())).ReturnsAsync(noBatches);
        var batchSearchDto = new BatchSearchDto(DateTimeOffset.Now, DateTimeOffset.Now);

        // Act
        var searchResult = await sut.SearchAsync(batchSearchDto);

        // Assert
        searchResult.Count().Should().Be(0);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task SearchAsync_NoMatchingBatches_DoesNotThrowException(
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        BatchApplicationService sut)
    {
        // Arrange
        var noBatches = new List<Batch>();
        batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Instant>(), It.IsAny<Instant>())).ReturnsAsync(noBatches);

        // Act
        var batchSearchDto = new BatchSearchDto(DateTimeOffset.Now, DateTimeOffset.Now);
        await sut.SearchAsync(batchSearchDto);

        // Assert
        // --- If we had an exception the test will fail - so no need for more assert
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task SearchAsync_ReturnsCorrectNumberOfBatches(
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        BatchApplicationService sut)
    {
        // Arrange
        const int numberOfBatches = 3;
        var batches = new List<Batch>()
        {
            new BatchBuilder().Build(),
            new BatchBuilder().Build(),
            new BatchBuilder().Build(),
        };
        batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Instant>(), It.IsAny<Instant>())).ReturnsAsync(batches);

        // Act
        var batchSearchDto = new BatchSearchDto(DateTimeOffset.Now, DateTimeOffset.Now);
        var searchResult = await sut.SearchAsync(batchSearchDto);

        // Assert
        searchResult.Count().Should().Be(numberOfBatches);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task StartCalculationAsync_ActivatesDomainServiceAndCommits(
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<ICalculationDomainService> calculationDomainServiceMock,
        Guid batchId,
        BatchApplicationService sut)
    {
        // Arrange & Act
        await sut.StartCalculationAsync(batchId);

        // Assert
        unitOfWorkMock.Verify(x => x.CommitAsync());
        calculationDomainServiceMock.Verify(x => x.StartAsync(batchId));
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionStateAsync_ActivatesDomainServiceAndCommits(
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IBatchExecutionStateDomainService> calculationDomainServiceMock,
        BatchApplicationService sut)
    {
        // Arrange & Act
        await sut.UpdateExecutionStateAsync();

        // Assert
        unitOfWorkMock.Verify(x => x.CommitAsync());
        calculationDomainServiceMock.Verify(x => x.UpdateExecutionStateAsync());
    }

    [Theory]
    [InlineAutoMoqData(CalculationState.Pending, BatchExecutionState.Pending)]
    [InlineAutoMoqData(CalculationState.Running, BatchExecutionState.Executing)]
    [InlineAutoMoqData(CalculationState.Completed, BatchExecutionState.Completed)]
    [InlineAutoMoqData(CalculationState.Canceled, BatchExecutionState.Canceled)]
    [InlineAutoMoqData(CalculationState.Failed, BatchExecutionState.Failed)]
    public void MapStateSuccess(CalculationState calculationState, BatchExecutionState expectedBatchExecutionState, BatchExecutionStateDomainService sut)
    {
        // Act
        var actualBatchExecutionState = sut.MapState(calculationState);

        // Assert
        actualBatchExecutionState.Should().Be(expectedBatchExecutionState);
    }

    [Theory]
    [InlineAutoMoqData]
    public void MapStateException(BatchExecutionStateDomainService sut)
    {
        // Arrange
        const CalculationState unexpectedCalculationState = (CalculationState)99;

        // Act
        Action action = () => sut.MapState(unexpectedCalculationState);

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("calculationState")
            .And.ActualValue.Should().Be(unexpectedCalculationState);
    }

    [Theory]
    [InlineAutoMoqData(BatchExecutionState.Pending, BatchExecutionState.Pending, 0)]
    [InlineAutoMoqData(BatchExecutionState.Executing, BatchExecutionState.Executing, 0)]
    [InlineAutoMoqData(BatchExecutionState.Completed, BatchExecutionState.Completed, 1)]
    [InlineAutoMoqData(BatchExecutionState.Failed, BatchExecutionState.Failed, 0)]
    [InlineAutoMoqData(BatchExecutionState.Canceled, BatchExecutionState.Created, 0)]

    public void HandleNewStateSuccess(BatchExecutionState state, BatchExecutionState expectedState, int expectedCompletedBatchesCount,  BatchExecutionStateDomainService sut)
    {
        // Arrange
        var batch = new Batch(
            ProcessType.BalanceFixing,
            new List<GridAreaCode> { new GridAreaCode("123") },
            Instant.MinValue,
            Instant.FromUtc(2023, 1, 1, 0, 0),
            Instant.MinValue,
            DateTimeZone.Utc);
        var completedBatches = new List<Batch>();

        // Act
        sut.HandleNewState(state, batch, completedBatches);

        // Assert
        batch.ExecutionState.Should().Be(expectedState);
        completedBatches.Count.Should().Be(expectedCompletedBatchesCount);
    }

    [Theory]
    [InlineAutoMoqData]
    public void HandleNewStateException(BatchExecutionStateDomainService sut)
    {
        // Arrange
        var state = (BatchExecutionState)99;
        var batch = new Batch(
            ProcessType.BalanceFixing,
            new List<GridAreaCode> { new GridAreaCode("123") },
            Instant.MinValue,
            Instant.FromUtc(2023, 1, 1, 0, 0),
            Instant.MinValue,
            DateTimeZone.Utc);
        var completedBatches = new List<Batch>();

        // Act
        Action action = () => sut.HandleNewState(state, batch, completedBatches);

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}
