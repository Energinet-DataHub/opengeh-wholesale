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
        var pythonWheelTask = job.Settings.Tasks.FirstOrDefault(x => x.PythonWheelTask != null);

        var sourceParams =
            pythonWheelTask?.PythonWheelTask?.Parameters ??
            throw new InvalidOperationException($"Parameters for job {job.JobId} could not be found.");

        sourceParams.Append($"--batch-id={batch.Id.Id}");
        sourceParams.Append($"--grid-areas=['805','806']");
        sourceParams.Append($"--snapshot-datetime={}");
        sourceParams.Append($"--period-start-datetime={}"); // datetime.strptime('31/05/2022 22:00', '%d/%m/%Y %H:%M')
        sourceParams.Append($"--period-end-datetime={}"); // datetime.strptime('31/05/2022 22:00', '%d/%m/%Y %H:%M')

        return RunParameters.CreatePythonParams(sourceParams);
    }
}
