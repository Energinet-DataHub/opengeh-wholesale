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
using FluentAssertions;
using Moq;
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
        var batch = new BatchBuilder().WithState(BatchExecutionState.Created).Build();
        batch.MarkAsSubmitted(new JobRunId(11));
        batch.MarkAsPending();
        var pendingBatches = new List<Batch>() { batch };
        batchRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<BatchExecutionState>>())).ReturnsAsync(pendingBatches);
        calculatorJobRunnerMock.Setup(runner => runner.GetJobStateAsync(batch.RunId!)).ReturnsAsync(JobState.Running);

        // Act
        await sut.UpdateExecutionStateAsync(batchRepositoryMock.Object, calculatorJobRunnerMock.Object);

        // Assert
        batch.ExecutionState.Should().Be(BatchExecutionState.Executing);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_WhenJobStateIsCompleted_UpdateBatchToCompleted(
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        [Frozen] Mock<ICalculatorJobRunner> calculatorJobRunnerMock,
        BatchExecutionStateHandler sut)
    {
        // Arrange
        var batch = new BatchBuilder().WithState(BatchExecutionState.Created).Build();
        batch.MarkAsSubmitted(new JobRunId(11));
        batch.MarkAsPending();
        batch.MarkAsExecuting();
        var executingBatches = new List<Batch>() { batch };
        batchRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<BatchExecutionState>>())).ReturnsAsync(executingBatches);
        calculatorJobRunnerMock.Setup(runner => runner.GetJobStateAsync(batch.RunId!)).ReturnsAsync(JobState.Completed);

        // Act
        await sut.UpdateExecutionStateAsync(batchRepositoryMock.Object, calculatorJobRunnerMock.Object);

        // Assert
        batch.ExecutionState.Should().Be(BatchExecutionState.Completed);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_ToCompleted(
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        [Frozen] Mock<ICalculatorJobRunner> calculatorJobRunnerMock,
        BatchExecutionStateHandler sut)
    {
        // Arrange
        var batch1 = new BatchBuilder().WithState(BatchExecutionState.Created).Build();
        batch1.MarkAsSubmitted(new JobRunId(11));
        batch1.MarkAsPending();
        var batch2 = new BatchBuilder().WithState(BatchExecutionState.Created).Build();
        batch2.MarkAsSubmitted(new JobRunId(12));
        batch2.MarkAsPending();
        batch2.MarkAsExecuting();
        var batches = new List<Batch>() { batch1, batch2 };

        batchRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<BatchExecutionState>>())).ReturnsAsync(batches);
        calculatorJobRunnerMock.Setup(runner => runner.GetJobStateAsync(batch2.RunId!)).ReturnsAsync(JobState.Completed);

        // Act
        var completedBatches = (await sut.UpdateExecutionStateAsync(batchRepositoryMock.Object, calculatorJobRunnerMock.Object)).ToList();

        // Assert
        completedBatches.Should().ContainSingle();
        completedBatches.First().Should().Be(batch2);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_When_RunnerThrowsException_Then_SkipBatch(
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        [Frozen] Mock<ICalculatorJobRunner> calculatorJobRunnerMock,
        BatchExecutionStateHandler sut)
    {
        // Arrange
        var batch1 = new BatchBuilder().WithState(BatchExecutionState.Created).Build();
        var batch2 = new BatchBuilder().WithState(BatchExecutionState.Created).Build();


        // Act

        // Assert
    }
}
