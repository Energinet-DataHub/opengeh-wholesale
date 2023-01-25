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
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Application.CalculationJobs;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Tests.Domain.BatchAggregate;
using FluentAssertions;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Application;

[UnitTest]
public class MapBatchExecutionStateTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_WhenJobStateIsRunning_UpdateBatchToExecuting(
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        [Frozen] Mock<ICalculatorJobRunner> calculatorJobRunnerMock,
        BatchExecutionStateHandler sut)
    {
        // Arrange
        var batch = new BatchBuilder().WithStatePending().Build();
        var pendingBatches = new List<Batch>() { batch };
        batchRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<BatchExecutionState>>()))
            .ReturnsAsync(pendingBatches);
        calculatorJobRunnerMock.Setup(runner => runner.GetJobStateAsync(batch.RunId!)).ReturnsAsync(JobState.Running);

        // Act
        await sut.UpdateExecutionStateAsync(batchRepositoryMock.Object, calculatorJobRunnerMock.Object);

        // Assert
        batch.ExecutionState.Should().Be(BatchExecutionState.Executing);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_WhenJobStateIsCompleted_UpdateBatchToCompleted(
        [Frozen] Mock<IClock> clockMock,
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        [Frozen] Mock<ICalculatorJobRunner> calculatorJobRunnerMock,
        BatchExecutionStateHandler sut)
    {
        // Arrange
        var batch = new BatchBuilder().WithStateExecuting().Build();
        var executionTimeEndGreaterThanStart = batch.ExecutionTimeStart.Plus(Duration.FromDays(2));
        clockMock.Setup(clock => clock.GetCurrentInstant()).Returns(executionTimeEndGreaterThanStart);
        var executingBatches = new List<Batch>() { batch };
        batchRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<BatchExecutionState>>()))
            .ReturnsAsync(executingBatches);
        calculatorJobRunnerMock.Setup(runner => runner.GetJobStateAsync(batch.RunId!)).ReturnsAsync(JobState.Completed);

        // Act
        await sut.UpdateExecutionStateAsync(batchRepositoryMock.Object, calculatorJobRunnerMock.Object);

        // Assert
        batch.ExecutionState.Should().Be(BatchExecutionState.Completed);
    }

    /// <summary>
    /// Jobs may be cancelled in Databricks for various reasons. For example they can be cancelled due to migrations in CD.
    /// Setting batch state back to "created" ensure they will be picked up and started again.
    /// </summary>
    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_WhenJobStateIsCancelled_UpdateBatchToCreated(
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        [Frozen] Mock<ICalculatorJobRunner> calculatorJobRunnerMock,
        BatchExecutionStateHandler sut)
    {
        // Arrange
        var batch = new BatchBuilder().WithStateExecuting().Build();
        var executingBatches = new List<Batch>() { batch };
        batchRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<BatchExecutionState>>()))
            .ReturnsAsync(executingBatches);
        calculatorJobRunnerMock.Setup(runner => runner.GetJobStateAsync(batch.RunId!)).ReturnsAsync(JobState.Canceled);

        // Act
        await sut.UpdateExecutionStateAsync(batchRepositoryMock.Object, calculatorJobRunnerMock.Object);

        // Assert
        batch.ExecutionState.Should().Be(BatchExecutionState.Created);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_ToCompleted(
        [Frozen] Mock<IClock> clockMock,
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        [Frozen] Mock<ICalculatorJobRunner> calculatorJobRunnerMock,
        BatchExecutionStateHandler sut)
    {
        // Arrange
        var batch1 = new BatchBuilder().WithStatePending().Build();
        var batch2 = new BatchBuilder().WithStateExecuting().Build();
        var batches = new List<Batch>() { batch1, batch2 };
        var executionTimeEndGreaterThanStart = batch1.ExecutionTimeStart.Plus(Duration.FromDays(2));
        clockMock.Setup(clock => clock.GetCurrentInstant()).Returns(executionTimeEndGreaterThanStart);

        batchRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<BatchExecutionState>>()))
            .ReturnsAsync(batches);
        calculatorJobRunnerMock.Setup(runner => runner.GetJobStateAsync(batch2.RunId!))
            .ReturnsAsync(JobState.Completed);

        // Act
        var completedBatches =
            (await sut.UpdateExecutionStateAsync(batchRepositoryMock.Object, calculatorJobRunnerMock.Object)).ToList();

        // Assert
        completedBatches.Should().ContainSingle();
        completedBatches.First().Should().Be(batch2);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_When_JobRunnerThrowsException_Then_SkipBatch(
        [Frozen] Mock<IClock> clockMock,
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        [Frozen] Mock<ICalculatorJobRunner> calculatorJobRunnerMock,
        BatchExecutionStateHandler sut)
    {
        // Arrange
        var batch1 = new BatchBuilder().WithStateSubmitted().Build();
        var batch2 = new BatchBuilder().WithStateSubmitted().Build();
        var batch3 = new BatchBuilder().WithStateSubmitted().Build();
        var batches = new List<Batch>() { batch1, batch2, batch3 };

        var executionTimeEndGreaterThanStart = batch1.ExecutionTimeStart.Plus(Duration.FromDays(2));
        clockMock.Setup(clock => clock.GetCurrentInstant()).Returns(executionTimeEndGreaterThanStart);
        batchRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<BatchExecutionState>>()))
            .ReturnsAsync(batches);
        calculatorJobRunnerMock.Setup(runner => runner.GetJobStateAsync(batch1.RunId!))
            .ReturnsAsync(JobState.Completed);
        calculatorJobRunnerMock.Setup(runner => runner.GetJobStateAsync(batch2.RunId!)).ThrowsAsync(default);
        calculatorJobRunnerMock.Setup(runner => runner.GetJobStateAsync(batch3.RunId!))
            .ReturnsAsync(JobState.Completed);

        // Act
        var completedBatches =
            (await sut.UpdateExecutionStateAsync(batchRepositoryMock.Object, calculatorJobRunnerMock.Object)).ToList();

        // Assert
        completedBatches.Should().Contain(new[] { batch1, batch3 });
        completedBatches.Should().NotContain(batch2);
    }
}
