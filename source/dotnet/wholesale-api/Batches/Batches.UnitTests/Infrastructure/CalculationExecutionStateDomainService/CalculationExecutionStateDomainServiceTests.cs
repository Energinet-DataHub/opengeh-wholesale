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
using Energinet.DataHub.Wholesale.Batches.Application;
using Energinet.DataHub.Wholesale.Batches.Application.Model;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.CalculationState;
using Energinet.DataHub.Wholesale.Batches.UnitTests.Infrastructure.CalculationAggregate;
using FluentAssertions;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Batches.UnitTests.Infrastructure.CalculationExecutionStateDomainService;

public class CalculationExecutionStateDomainServiceTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_WhenJobStateIsRunning_UpdateBatchToExecuting(
        [Frozen] Mock<ICalculationRepository> batchRepositoryMock,
        [Frozen] Mock<ICalculationInfrastructureService> calculatorJobRunnerMock,
        CalculationExecutionStateInfrastructureService sut)
    {
        // Arrange
        var batch = new CalculationBuilder().WithStatePending().Build();
        var pendingBatches = new List<Calculation>() { batch };
        batchRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<CalculationExecutionState>>()))
            .ReturnsAsync(pendingBatches);
        calculatorJobRunnerMock.Setup(runner => runner.GetStatusAsync(batch.CalculationId!)).ReturnsAsync(CalculationState.Running);

        // Act
        await sut.UpdateExecutionStateAsync();

        // Assert
        batch.ExecutionState.Should().Be(CalculationExecutionState.Executing);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_WhenJobStateIsCompleted_UpdateBatchToCompleted(
        [Frozen] Mock<IClock> clockMock,
        [Frozen] Mock<ICalculationRepository> batchRepositoryMock,
        [Frozen] Mock<ICalculationInfrastructureService> calculatorJobRunnerMock,
        CalculationExecutionStateInfrastructureService sut)
    {
        // Arrange
        var batch = new CalculationBuilder().WithStateExecuting().Build();
        var executionTimeEndGreaterThanStart = batch.ExecutionTimeStart!.Value.Plus(Duration.FromDays(2));
        clockMock.Setup(clock => clock.GetCurrentInstant()).Returns(executionTimeEndGreaterThanStart);
        var executingBatches = new List<Calculation>() { batch };
        batchRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<CalculationExecutionState>>()))
            .ReturnsAsync(executingBatches);
        calculatorJobRunnerMock.Setup(runner => runner.GetStatusAsync(batch.CalculationId!)).ReturnsAsync(CalculationState.Completed);

        // Act
        await sut.UpdateExecutionStateAsync();

        // Assert
        batch.ExecutionState.Should().Be(CalculationExecutionState.Completed);
    }

    /// <summary>
    /// Jobs may be cancelled in Databricks for various reasons. For example they can be cancelled due to migrations in CD.
    /// Setting batch state back to "created" ensure they will be picked up and started again.
    /// </summary>
    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_WhenJobStateIsCancelled_UpdateBatchToCreated(
        [Frozen] Mock<ICalculationRepository> batchRepositoryMock,
        [Frozen] Mock<ICalculationInfrastructureService> calculatorJobRunnerMock,
        CalculationExecutionStateInfrastructureService sut)
    {
        // Arrange
        var batch = new CalculationBuilder().WithStateExecuting().Build();
        var executingBatches = new List<Calculation>() { batch };
        batchRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<CalculationExecutionState>>()))
            .ReturnsAsync(executingBatches);
        calculatorJobRunnerMock.Setup(runner => runner.GetStatusAsync(batch.CalculationId!)).ReturnsAsync(CalculationState.Canceled);

        // Act
        await sut.UpdateExecutionStateAsync();

        // Assert
        batch.ExecutionState.Should().Be(CalculationExecutionState.Created);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_ToCompleted(
        [Frozen] Mock<IClock> clockMock,
        [Frozen] Mock<ICalculationRepository> batchRepositoryMock,
        [Frozen] Mock<ICalculationInfrastructureService> calculatorJobRunnerMock,
        CalculationExecutionStateInfrastructureService sut)
    {
        // Arrange
        var batch1 = new CalculationBuilder().WithStatePending().Build();
        var batch2 = new CalculationBuilder().WithStateExecuting().Build();
        var batches = new List<Calculation>() { batch1, batch2 };
        var executionTimeEndGreaterThanStart = batch1.ExecutionTimeStart!.Value.Plus(Duration.FromDays(2));
        clockMock.Setup(clock => clock.GetCurrentInstant()).Returns(executionTimeEndGreaterThanStart);

        batchRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<CalculationExecutionState>>()))
            .ReturnsAsync(batches);
        calculatorJobRunnerMock.Setup(runner => runner.GetStatusAsync(batch1.CalculationId!))
            .ReturnsAsync(CalculationState.Pending); // Unchanged
        calculatorJobRunnerMock.Setup(runner => runner.GetStatusAsync(batch2.CalculationId!))
            .ReturnsAsync(CalculationState.Completed);

        // Act
        await sut.UpdateExecutionStateAsync();

        // Assert
        batch2.ExecutionState.Should().Be(CalculationExecutionState.Completed);
        batch1.ExecutionState.Should().Be(CalculationExecutionState.Pending); // Unchanged
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_WhenCompleting_CompletedBatch(
        [Frozen] Mock<IClock> clockMock,
        [Frozen] Mock<ICalculationRepository> batchRepositoryMock,
        [Frozen] Mock<ICalculationInfrastructureService> calculatorJobRunnerMock,
        CalculationExecutionStateInfrastructureService sut)
    {
        // Arrange
        var batch1 = new CalculationBuilder().WithStatePending().Build();
        var batch2 = new CalculationBuilder().WithStateExecuting().Build();
        var batches = new List<Calculation>() { batch1, batch2 };
        var executionTimeEndGreaterThanStart = batch1.ExecutionTimeStart!.Value.Plus(Duration.FromDays(2));
        clockMock.Setup(clock => clock.GetCurrentInstant()).Returns(executionTimeEndGreaterThanStart);
        batchRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<CalculationExecutionState>>()))
            .ReturnsAsync(batches);
        calculatorJobRunnerMock.Setup(runner => runner.GetStatusAsync(batch2.CalculationId!))
            .ReturnsAsync(CalculationState.Completed);

        // Act
        await sut.UpdateExecutionStateAsync();

        // Assert
        batch2.ExecutionState.Should().Be(CalculationExecutionState.Completed);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_When_JobRunnerThrowsException_Then_SkipBatch(
        [Frozen] Mock<IClock> clockMock,
        [Frozen] Mock<ICalculationRepository> batchRepositoryMock,
        [Frozen] Mock<ICalculationInfrastructureService> calculatorJobRunnerMock,
        CalculationExecutionStateInfrastructureService sut)
    {
        // Arrange
        var batch1 = new CalculationBuilder().WithStateSubmitted().Build();
        var batch2 = new CalculationBuilder().WithStateSubmitted().Build();
        var batch3 = new CalculationBuilder().WithStateSubmitted().Build();
        var batches = new List<Calculation>() { batch1, batch2, batch3 };

        var executionTimeEndGreaterThanStart = batch1.ExecutionTimeStart!.Value.Plus(Duration.FromDays(2));
        clockMock.Setup(clock => clock.GetCurrentInstant()).Returns(executionTimeEndGreaterThanStart);
        batchRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<CalculationExecutionState>>()))
            .ReturnsAsync(batches);
        calculatorJobRunnerMock.Setup(runner => runner.GetStatusAsync(batch1.CalculationId!))
            .ReturnsAsync(CalculationState.Completed);
        calculatorJobRunnerMock.Setup(runner => runner.GetStatusAsync(batch2.CalculationId!)).ThrowsAsync(default);
        calculatorJobRunnerMock.Setup(runner => runner.GetStatusAsync(batch3.CalculationId!))
            .ReturnsAsync(CalculationState.Completed);

        // Act
        await sut.UpdateExecutionStateAsync();

        // Assert: Events was published for batch1 and batch3, but not for batch2
        batch1.ExecutionState.Should().Be(CalculationExecutionState.Completed);
        batch2.ExecutionState.Should().Be(CalculationExecutionState.Submitted);
        batch3.ExecutionState.Should().Be(CalculationExecutionState.Completed);
    }
}
