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
using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Calculations;
using Energinet.DataHub.Wholesale.Calculations.UnitTests.Infrastructure.CalculationAggregate;
using FluentAssertions;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Calculations.UnitTests.Infrastructure.CalculationExecutionStateDomainService;

[UnitTest]
public class CalculationExecutionStateDomainServiceTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_WhenJobStateIsRunning_UpdateCalculationToExecuting(
        [Frozen] Mock<ICalculationRepository> calculationRepositoryMock,
        [Frozen] Mock<ICalculationInfrastructureService> calculatorJobRunnerMock,
        CalculationExecutionStateInfrastructureService sut)
    {
        // Arrange
        var calculation = new CalculationBuilder().WithStatePending().Build();
        var pendingCalculations = new List<Calculation>() { calculation };
        calculationRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<CalculationExecutionState>>()))
            .ReturnsAsync(pendingCalculations);
        calculatorJobRunnerMock.Setup(runner => runner.GetStatusAsync(calculation.CalculationId!)).ReturnsAsync(CalculationState.Running);

        // Act
        await sut.UpdateExecutionStateAsync();

        // Assert
        calculation.ExecutionState.Should().Be(CalculationExecutionState.Executing);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_WhenJobStateIsCompleted_UpdateCalculationToCompleted(
        [Frozen] Mock<IClock> clockMock,
        [Frozen] Mock<ICalculationRepository> calculationRepositoryMock,
        [Frozen] Mock<ICalculationInfrastructureService> calculatorJobRunnerMock,
        CalculationExecutionStateInfrastructureService sut)
    {
        // Arrange
        var calculation = new CalculationBuilder().WithStateExecuting().Build();
        var executionTimeEndGreaterThanStart = calculation.ExecutionTimeStart!.Value.Plus(Duration.FromDays(2));
        clockMock.Setup(clock => clock.GetCurrentInstant()).Returns(executionTimeEndGreaterThanStart);
        var executingCalculations = new List<Calculation>() { calculation };
        calculationRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<CalculationExecutionState>>()))
            .ReturnsAsync(executingCalculations);
        calculatorJobRunnerMock.Setup(runner => runner.GetStatusAsync(calculation.CalculationId!)).ReturnsAsync(CalculationState.Completed);

        // Act
        await sut.UpdateExecutionStateAsync();

        // Assert
        calculation.ExecutionState.Should().Be(CalculationExecutionState.Completed);
    }

    /// <summary>
    /// Jobs may be cancelled in Databricks for various reasons. For example they can be cancelled due to migrations in CD.
    /// Setting calculation state back to "created" ensure they will be picked up and started again.
    /// </summary>
    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_WhenJobStateIsCancelled_UpdateCalculationToCreated(
        [Frozen] Mock<ICalculationRepository> calculationRepositoryMock,
        [Frozen] Mock<ICalculationInfrastructureService> calculatorJobRunnerMock,
        CalculationExecutionStateInfrastructureService sut)
    {
        // Arrange
        var calculation = new CalculationBuilder().WithStateExecuting().Build();
        var executingCalculations = new List<Calculation>() { calculation };
        calculationRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<CalculationExecutionState>>()))
            .ReturnsAsync(executingCalculations);
        calculatorJobRunnerMock.Setup(runner => runner.GetStatusAsync(calculation.CalculationId!)).ReturnsAsync(CalculationState.Canceled);

        // Act
        await sut.UpdateExecutionStateAsync();

        // Assert
        calculation.ExecutionState.Should().Be(CalculationExecutionState.Created);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_ToCompleted(
        [Frozen] Mock<IClock> clockMock,
        [Frozen] Mock<ICalculationRepository> calculationRepositoryMock,
        [Frozen] Mock<ICalculationInfrastructureService> calculatorJobRunnerMock,
        CalculationExecutionStateInfrastructureService sut)
    {
        // Arrange
        var calculation1 = new CalculationBuilder().WithStatePending().Build();
        var calculation2 = new CalculationBuilder().WithStateExecuting().Build();
        var calculations = new List<Calculation>() { calculation1, calculation2 };
        var executionTimeEndGreaterThanStart = calculation1.ExecutionTimeStart!.Value.Plus(Duration.FromDays(2));
        clockMock.Setup(clock => clock.GetCurrentInstant()).Returns(executionTimeEndGreaterThanStart);

        calculationRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<CalculationExecutionState>>()))
            .ReturnsAsync(calculations);
        calculatorJobRunnerMock.Setup(runner => runner.GetStatusAsync(calculation1.CalculationId!))
            .ReturnsAsync(CalculationState.Pending); // Unchanged
        calculatorJobRunnerMock.Setup(runner => runner.GetStatusAsync(calculation2.CalculationId!))
            .ReturnsAsync(CalculationState.Completed);

        // Act
        await sut.UpdateExecutionStateAsync();

        // Assert
        calculation2.ExecutionState.Should().Be(CalculationExecutionState.Completed);
        calculation1.ExecutionState.Should().Be(CalculationExecutionState.Pending); // Unchanged
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_WhenCompleting_CompletedCalculation(
        [Frozen] Mock<IClock> clockMock,
        [Frozen] Mock<ICalculationRepository> calculationRepositoryMock,
        [Frozen] Mock<ICalculationInfrastructureService> calculatorJobRunnerMock,
        CalculationExecutionStateInfrastructureService sut)
    {
        // Arrange
        var calculation1 = new CalculationBuilder().WithStatePending().Build();
        var calculation2 = new CalculationBuilder().WithStateExecuting().Build();
        var calculations = new List<Calculation>() { calculation1, calculation2 };
        var executionTimeEndGreaterThanStart = calculation1.ExecutionTimeStart!.Value.Plus(Duration.FromDays(2));
        clockMock.Setup(clock => clock.GetCurrentInstant()).Returns(executionTimeEndGreaterThanStart);
        calculationRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<CalculationExecutionState>>()))
            .ReturnsAsync(calculations);
        calculatorJobRunnerMock.Setup(runner => runner.GetStatusAsync(calculation2.CalculationId!))
            .ReturnsAsync(CalculationState.Completed);

        // Act
        await sut.UpdateExecutionStateAsync();

        // Assert
        calculation2.ExecutionState.Should().Be(CalculationExecutionState.Completed);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionState_When_JobRunnerThrowsException_Then_SkipCalculation(
        [Frozen] Mock<IClock> clockMock,
        [Frozen] Mock<ICalculationRepository> calculationRepositoryMock,
        [Frozen] Mock<ICalculationInfrastructureService> calculatorJobRunnerMock,
        CalculationExecutionStateInfrastructureService sut)
    {
        // Arrange
        var calculation1 = new CalculationBuilder().WithStateSubmitted().Build();
        var calculation2 = new CalculationBuilder().WithStateSubmitted().Build();
        var calculation3 = new CalculationBuilder().WithStateSubmitted().Build();
        var calculations = new List<Calculation>() { calculation1, calculation2, calculation3 };

        var executionTimeEndGreaterThanStart = calculation1.ExecutionTimeStart!.Value.Plus(Duration.FromDays(2));
        clockMock.Setup(clock => clock.GetCurrentInstant()).Returns(executionTimeEndGreaterThanStart);
        calculationRepositoryMock.Setup(repo => repo.GetByStatesAsync(It.IsAny<IEnumerable<CalculationExecutionState>>()))
            .ReturnsAsync(calculations);
        calculatorJobRunnerMock.Setup(runner => runner.GetStatusAsync(calculation1.CalculationId!))
            .ReturnsAsync(CalculationState.Completed);
        calculatorJobRunnerMock.Setup(runner => runner.GetStatusAsync(calculation2.CalculationId!)).ThrowsAsync(default);
        calculatorJobRunnerMock.Setup(runner => runner.GetStatusAsync(calculation3.CalculationId!))
            .ReturnsAsync(CalculationState.Completed);

        // Act
        await sut.UpdateExecutionStateAsync();

        // Assert: Events was published for calculation1 and calculation3, but not for calculation2
        calculation1.ExecutionState.Should().Be(CalculationExecutionState.Completed);
        calculation2.ExecutionState.Should().Be(CalculationExecutionState.Submitted);
        calculation3.ExecutionState.Should().Be(CalculationExecutionState.Completed);
    }
}
