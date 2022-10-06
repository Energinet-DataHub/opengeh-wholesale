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

using Energinet.DataHub.Wholesale.Application.JobRunner;
using Energinet.DataHub.Wholesale.Components.DatabricksClient;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Microsoft.Azure.Databricks.Client;

namespace Energinet.DataHub.Wholesale.Infrastructure.JobRunner;

public sealed class DatabricksCalculatorJobRunner : ICalculatorJobRunner
{
    private readonly IDatabricksCalculatorJobSelector _databricksCalculatorJobSelector;
    private readonly IDatabricksWheelClient _wheelClient;

    public DatabricksCalculatorJobRunner(
        IDatabricksCalculatorJobSelector databricksCalculatorJobSelector,
        IDatabricksWheelClient wheelClient)
    {
        _databricksCalculatorJobSelector = databricksCalculatorJobSelector;
        _wheelClient = wheelClient;
    }

    public async Task<JobRunId> SubmitJobAsync(IEnumerable<string> jobParameters)
    {
        var calculatorJob = await _databricksCalculatorJobSelector
            .GetAsync()
            .ConfigureAwait(false);

        var runParameters = MergeRunParameters(calculatorJob, jobParameters);

        var runId = await _wheelClient
            .Jobs
            .RunNow(calculatorJob.JobId, runParameters)
            .ConfigureAwait(false);

        return new JobRunId(runId.RunId);
    }

    public async Task<JobState> GetJobStateAsync(JobRunId jobRunId)
    {
        var runState = await _wheelClient
            .Jobs
            .RunsGet(jobRunId.Id)
            .ConfigureAwait(false);

        return runState.State.LifeCycleState switch
        {
            RunLifeCycleState.PENDING => JobState.Pending,
            RunLifeCycleState.RUNNING => JobState.Running,
            RunLifeCycleState.TERMINATING => JobState.Running,
            RunLifeCycleState.SKIPPED => JobState.Canceled,
            RunLifeCycleState.INTERNAL_ERROR => JobState.Failed,
            RunLifeCycleState.TERMINATED => runState.State.ResultState switch
            {
                RunResultState.SUCCESS => JobState.Completed,
                RunResultState.FAILED => JobState.Failed,
                RunResultState.CANCELED => JobState.Canceled,
                RunResultState.TIMEDOUT => JobState.Canceled,
                _ => throw new ArgumentOutOfRangeException(nameof(runState.State)),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(runState.State)),
        };
    }

    private static RunParameters MergeRunParameters(WheelJob job, IEnumerable<string> jobParameters)
    {
        var pythonWheelTask = job.Settings.Tasks.FirstOrDefault(x => x.PythonWheelTask != null);

        var sourceParams =
            pythonWheelTask?.PythonWheelTask?.Parameters ??
            throw new InvalidOperationException($"Parameters for job {job.JobId} could not be found.");

        sourceParams.AddRange(jobParameters);

        return RunParameters.CreatePythonParams(sourceParams);
    }
}
