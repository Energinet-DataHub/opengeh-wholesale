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

namespace Energinet.DataHub.Wholesale.Infrastructure.JobRunner;

using Microsoft.Azure.Databricks.Client;

public sealed class DatabricksJobRunner : IJobRunner
{
    private readonly DatabricksClient _client;

    public DatabricksJobRunner(DatabricksClient client)
    {
        _client = client;
    }

    public async Task<JobRunId> SubmitJobAsync(Batch batch)
    {
        var knownJobs = await _client.Jobs.List().ConfigureAwait(false);
        var calculatorJob = knownJobs.Single(j => j.Settings.Name == "CalculatorJob");

        var runParameters = RunParameters.CreatePythonParams(new[]
        {
            "param1",
        });

        var runId = await _client.Jobs.RunNow(calculatorJob.JobId, runParameters).ConfigureAwait(false);
        return new JobRunId(runId.RunId);
    }

    public async Task<JobState> GetJobStateAsync(JobRunId jobRunId)
    {
        var runState = await _client.Jobs.RunsGet(jobRunId.Id).ConfigureAwait(false);

        switch (runState.State.ResultState)
        {
            case RunResultState.SUCCESS:
                return JobState.Completed;
            case RunResultState.FAILED:
            case RunResultState.TIMEDOUT:
            case RunResultState.CANCELED:
                return JobState.Failed;
            case null:
                return JobState.Running;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
