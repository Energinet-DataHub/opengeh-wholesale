using Microsoft.Azure.Databricks.Client;

namespace DatabricksJobRunner;

internal sealed class DatabricksJobRunner : IJobRunnerPort
{
    private readonly DatabricksClient _client;

    public DatabricksJobRunner(DatabricksClient client)
    {
        _client = client;
    }

    public async Task<JobRunId> SubmitJobAsync()
    {
        var knownJobs = await _client.Jobs.List();
        var calculatorJob = knownJobs.Single(j => j.Settings.Name == "CalculatorJob");

        var runParameters = RunParameters.CreatePythonParams(new[]
        {
            "param1"
        });

        var runId = await _client.Jobs.RunNow(calculatorJob.JobId, runParameters);
        return new JobRunId(runId.RunId);
    }

    public async Task<JobState> GetJobStateAsync(JobRunId jobRunId)
    {
        var runState = await _client.Jobs.RunsGet(jobRunId.Id);

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
