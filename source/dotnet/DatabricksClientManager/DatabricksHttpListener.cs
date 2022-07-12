using System.Net;
using System.Text;
using Microsoft.Azure.Databricks.Client;
using Newtonsoft.Json;

namespace DatabricksApiFixture;

public static class DatabricksHttpListener
{
    // https://github.com/Azure/azure-databricks-client/blob/master/csharp/Microsoft.Azure.Databricks.Client/JobsApiClient.cs
    //public static async Task Main(string[] args)
    //{
    //    await Listen("http://localhost:9000/", 1, CancellationToken.None);
    //}
    public static async Task Listen(string prefix, int maxConcurrentRequests, CancellationToken token)
    {
        var listener = new HttpListener();
        listener.Prefixes.Add(prefix);
        listener.Start();

        var requests = new HashSet<Task>();
        for (var i = 0; i < maxConcurrentRequests; i++)
            requests.Add(listener.GetContextAsync());

        while (!token.IsCancellationRequested)
        {
            var t = await Task.WhenAny(requests);
            requests.Remove(t);

            if (t is Task<HttpListenerContext> taskRequest)
            {
                requests.Add(ProcessRequestAsync(taskRequest.Result));
                requests.Add(listener.GetContextAsync());
            }
        }
    }

    public static async Task ProcessRequestAsync(HttpListenerContext context)
    {
        if (context.Request.RawUrl.Contains("jobs/list"))
        {
            Job calculatorJob = new Job();
            calculatorJob.JobId = 42;
            calculatorJob.Settings = new JobSettings();
            calculatorJob.Settings.Name = "CalculatorJob";

            var serialized = JsonConvert.SerializeObject(new { jobs = new[] { calculatorJob } });

            context.Response.StatusCode = 200;
            context.Response.Close(Encoding.UTF8.GetBytes(serialized), true);
        }

        if (context.Request.RawUrl.Contains("jobs/run-now"))
        {
            var streamReader = new StreamReader(context.Request.InputStream);
            var jobSettings = JsonConvert.DeserializeObject<JobSettings>(streamReader.ReadToEnd());

            var runIndentifier = new RunIdentifier();
            runIndentifier.RunId = 33;

            var serialized = JsonConvert.SerializeObject(runIndentifier);

            context.Response.StatusCode = 200;
            context.Response.Close(Encoding.UTF8.GetBytes(serialized), true);
        }

        if (context.Request.RawUrl.Contains("jobs/runs/get?"))
        {
            var run = new Run();
            run.RunId = 33;
            run.State = new RunState { ResultState = RunResultState.SUCCESS };

            var serialized = JsonConvert.SerializeObject(run);

            context.Response.StatusCode = 200;
            context.Response.Close(Encoding.UTF8.GetBytes(serialized), true);
        }

        context.Response.StatusCode = 500;
        context.Response.Close();
    }
}
