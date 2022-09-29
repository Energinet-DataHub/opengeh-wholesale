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
using Energinet.DataHub.Wholesale.Application.JobRunner;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Tests.Domain.BatchAggregate;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Application;

[UnitTest]
public class MapBatchExecutionStateTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task MapBatchExecutionState_WhenJobStateIsRunning_UpdateBatchToExecuting(
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        [Frozen] Mock<ICalculatorJobRunner> calculatorJobRunnerMock,
        MapBatchExecutionState sut)
    {
        // Arrange
        var batch = new BatchBuilder().WithState(BatchExecutionState.Pending).Build();
        batch.SetJobRunId(new JobRunId(11));
        var pendingBatches = new List<Batch>() { batch };
        batchRepositoryMock.Setup(repo => repo.GetPendingAndExecutingAsync()).ReturnsAsync(pendingBatches);
        calculatorJobRunnerMock.Setup(runner => runner.GetJobStateAsync(batch.RunId!)).ReturnsAsync(JobState.Running);

        // Act
        await sut.UpdateExecutionStatesInBatchRepositoryAsync(batchRepositoryMock.Object, calculatorJobRunnerMock.Object);

        // Assert
        Assert.Equal(BatchExecutionState.Executing, batch.ExecutionState);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task MapBatchExecutionState_FromPendingToCompleted_ThrowsException(
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        [Frozen] Mock<ICalculatorJobRunner> calculatorJobRunnerMock,
        MapBatchExecutionState sut)
    {
        // Arrange
        var batch = new BatchBuilder().WithState(BatchExecutionState.Pending).Build();
        batch.SetJobRunId(new JobRunId(11));
        var pendingBatches = new List<Batch>() { batch };
        batchRepositoryMock.Setup(repo => repo.GetPendingAndExecutingAsync()).ReturnsAsync(pendingBatches);
        calculatorJobRunnerMock.Setup(runner => runner.GetJobStateAsync(batch.RunId!)).ReturnsAsync(JobState.Completed);

        // Act and assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.UpdateExecutionStatesInBatchRepositoryAsync(batchRepositoryMock.Object, calculatorJobRunnerMock.Object));
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task MapBatchExecutionState_WhenJobStateIsCompleted_UpdateBatchToCompleted(
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        [Frozen] Mock<ICalculatorJobRunner> calculatorJobRunnerMock,
        MapBatchExecutionState sut)
    {
        // Arrange
        var batch = new BatchBuilder().WithState(BatchExecutionState.Executing).Build();
        batch.SetJobRunId(new JobRunId(11));
        var executingBatches = new List<Batch>() { batch };
        batchRepositoryMock.Setup(repo => repo.GetPendingAndExecutingAsync()).ReturnsAsync(executingBatches);
        calculatorJobRunnerMock.Setup(runner => runner.GetJobStateAsync(batch.RunId!)).ReturnsAsync(JobState.Completed);

        // Act
        await sut.UpdateExecutionStatesInBatchRepositoryAsync(batchRepositoryMock.Object, calculatorJobRunnerMock.Object);

        // Assert
        Assert.Equal(BatchExecutionState.Completed, batch.ExecutionState);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task MapBatchExecutionState_(
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        [Frozen] Mock<ICalculatorJobRunner> calculatorJobRunnerMock,
        MapBatchExecutionState sut)
    {
        // Arrange
        var pendingBatch = new BatchBuilder().WithState(BatchExecutionState.Pending).Build();
        pendingBatch.SetJobRunId(new JobRunId(11));
        var executingBatch = new BatchBuilder().WithState(BatchExecutionState.Executing).Build();
        executingBatch.SetJobRunId(new JobRunId(12));
        var batches = new List<Batch>() { pendingBatch, executingBatch };

        batchRepositoryMock.Setup(repo => repo.GetPendingAndExecutingAsync()).ReturnsAsync(batches);
        calculatorJobRunnerMock.Setup(runner => runner.GetJobStateAsync(executingBatch.RunId!)).ReturnsAsync(JobState.Completed);

        // Act
        var completedBatches = (await sut.UpdateExecutionStatesInBatchRepositoryAsync(batchRepositoryMock.Object, calculatorJobRunnerMock.Object)).ToList();

        // Assert
        Assert.Single(completedBatches);
        Assert.Equal(executingBatch, completedBatches.First());
    }
}
