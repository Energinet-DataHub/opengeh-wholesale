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
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.DatabricksClient.Api.V2;
using Microsoft.Azure.Databricks.Client;

namespace Energinet.DataHub.Wholesale.Infrastructure.DatabricksClient.JobRunner;

public sealed class DatabricksJobRunner : IJobRunner
{
    private readonly DatabricksJobSelector _databricksJobSelector;
    private readonly DatabricksWheelClient _wheelClient;

    public DatabricksJobRunner(
        DatabricksJobSelector databricksJobSelector,
        DatabricksWheelClient wheelClient)
    {
        _databricksJobSelector = databricksJobSelector;
        _wheelClient = wheelClient;
    }

    public async Task<JobRunId> SubmitJobAsync(Batch batch)
    {
        var calculatorJob = await _databricksJobSelector
            .SelectCalculatorJobAsync()
            .ConfigureAwait(false);

        var runParameters = MergeRunParameters(calculatorJob, batch);

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

        return runState.State.ResultState switch
        {
            RunResultState.SUCCESS => JobState.Completed,
            RunResultState.FAILED => JobState.Failed,
            RunResultState.TIMEDOUT => JobState.Failed,
            RunResultState.CANCELED => JobState.Canceled,
            null => JobState.Running,
            _ => throw new ArgumentOutOfRangeException(nameof(runState.State)),
        };
    }

    private static RunParameters MergeRunParameters(WheelJob job, Batch batch)
    {
        var sourceParams =
            job.Settings.SparkPythonTask?.Parameters ?? // Python file remove this in PR #157
            job.Settings.PythonWheelTask?.Parameters ?? // Wheel
            throw new InvalidOperationException($"Parameters for job {job.JobId} could not be found.");

        var invocationParam = $"--batch-id={batch.Id.Id}";
        return RunParameters.CreatePythonParams(sourceParams.Append(invocationParam));
    }
}
