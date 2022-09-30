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

namespace Energinet.DataHub.Wholesale.Tests.Infrastructure.JobRunner;

public class DatabricksCalculatorJobRunnerTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task Test_GetJobStateAsync(
        [Frozen] Mock<IDatabricksWheelClient> databricksWheelClientMock,
        DatabricksCalculatorJobRunner sut)
    {
        var jobRunId = new JobRunId(1);
        var runState = new Run { State = new RunState { LifeCycleState = RunLifeCycleState.RUNNING } };
        databricksWheelClientMock.Setup(x => x.Jobs.RunsGet(jobRunId.Id, CancellationToken.None)).ReturnsAsync(runState);
        var jobState = await sut.GetJobStateAsync(jobRunId);
        Assert.Equal(JobState.Running, jobState);
    }
}
