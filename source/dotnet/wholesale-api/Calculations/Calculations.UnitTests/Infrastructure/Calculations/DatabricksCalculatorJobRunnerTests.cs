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
using Energinet.DataHub.Core.Databricks.Jobs.Abstractions;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Calculations;
using Microsoft.Azure.Databricks.Client.Models;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Calculations.UnitTests.Infrastructure.Calculations;

public class DatabricksCalculatorJobRunnerTests
{
    [Theory]

    // When LifeCycleState is not Terminated, LifeCycleState will determine JobState
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Pending, RunLifeCycleState.PENDING)]
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Running, RunLifeCycleState.RUNNING)]
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Running, RunLifeCycleState.TERMINATING)]
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Canceled, RunLifeCycleState.SKIPPED)]
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Failed, RunLifeCycleState.INTERNAL_ERROR)]

    // When LifCycleState is Terminated, ResultState will determine JobState
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Completed, RunLifeCycleState.TERMINATED, RunResultState.SUCCESS)]
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Failed, RunLifeCycleState.TERMINATED, RunResultState.FAILED)]
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Canceled, RunLifeCycleState.TERMINATED, RunResultState.CANCELED)]
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Canceled, RunLifeCycleState.TERMINATED, RunResultState.TIMEDOUT)]

    // LifeCycleState determine JobState since LifeCycleState is not Terminated
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Running, RunLifeCycleState.TERMINATING, RunResultState.SUCCESS)]
    public async Task GivenRunState_WhenGetJobStateAsyncIsCalled_ThenReturnCorrectJobState(
        Wholesale.Calculations.Application.Model.CalculationState expectedCalculationState,
        RunLifeCycleState runLifeCycleState,
        RunResultState runResultState,
        [Frozen] Mock<IJobsApiClient> jobsApiMock,
        CalculationEngineClient sut)
    {
        var jobRunId = new CalculationJobId(1);
        var runState = new Run { State = new RunState { LifeCycleState = runLifeCycleState, ResultState = runResultState } };
        jobsApiMock.Setup(x => x.Jobs.RunsGet(jobRunId.Id, false, CancellationToken.None)).ReturnsAsync((runState, new RepairHistory()));
        var jobState = await sut.GetStatusAsync(jobRunId);
        Assert.Equal(expectedCalculationState, jobState);
    }
}
