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
using Energinet.DataHub.Wholesale.Application.JobRunner;
using Energinet.DataHub.Wholesale.Components.DatabricksClient;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.JobRunner;
using Microsoft.Azure.Databricks.Client;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Infrastructure.JobRunner;

public class DatabricksCalculatorJobRunnerTests
{
    [Theory]

    // When LifeCycleState is not Terminated, LifeCycleState will determine JobState
    [InlineAutoMoqData(JobState.Pending, RunLifeCycleState.PENDING)]
    [InlineAutoMoqData(JobState.Running, RunLifeCycleState.RUNNING)]
    [InlineAutoMoqData(JobState.Running, RunLifeCycleState.TERMINATING)]
    [InlineAutoMoqData(JobState.Canceled, RunLifeCycleState.SKIPPED)]
    [InlineAutoMoqData(JobState.Failed, RunLifeCycleState.INTERNAL_ERROR)]

    // When LifCycleState is Terminated, ResultState will determine JobState
    [InlineAutoMoqData(JobState.Completed, RunLifeCycleState.TERMINATED, RunResultState.SUCCESS)]
    [InlineAutoMoqData(JobState.Failed, RunLifeCycleState.TERMINATED, RunResultState.FAILED)]
    [InlineAutoMoqData(JobState.Canceled, RunLifeCycleState.TERMINATED, RunResultState.CANCELED)]
    [InlineAutoMoqData(JobState.Canceled, RunLifeCycleState.TERMINATED, RunResultState.TIMEDOUT)]

    // LifeCycleState determine JobState since LifeCycleState is not Terminated
    [InlineAutoMoqData(JobState.Running, RunLifeCycleState.TERMINATING, RunResultState.SUCCESS)]
    public async Task GivenRunState_WhenGetJobStateAsyncIsCalled_ThenReturnCorrectJobState(
        JobState expectedJobState,
        RunLifeCycleState runLifeCycleState,
        RunResultState runResultState,
        [Frozen] Mock<IDatabricksWheelClient> databricksWheelClientMock,
        DatabricksCalculatorJobRunner sut)
    {
        var jobRunId = new JobRunId(1);
        var runState = new Run { State = new RunState { LifeCycleState = runLifeCycleState, ResultState = runResultState } };
        databricksWheelClientMock.Setup(x => x.Jobs.RunsGet(jobRunId.Id, CancellationToken.None)).ReturnsAsync(runState);
        var jobState = await sut.GetJobStateAsync(jobRunId);
        Assert.Equal(expectedJobState, jobState);
    }
}
