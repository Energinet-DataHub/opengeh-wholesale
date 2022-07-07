using Microsoft.Azure.Databricks.Client;

namespace DatabricksJobRunner
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            using var jobRunner = CreateJobRunner();

            var jobId = await jobRunner.SubmitJobAsync();

            await Task.Delay(1000);

            var state = await jobRunner.GetJobStateAsync(jobId);

            ;
        }

        private static IJobRunnerPort CreateJobRunner()
        {
            var client = DatabricksClient.CreateClient("https://adb-5870161604877074.14.azuredatabricks.net", "https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            return new DatabricksJobRunner(client);
        }
    }
}
