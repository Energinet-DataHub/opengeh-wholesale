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

using System.Net;
using System.Text;
using Energinet.DataHub.Wholesale.Infrastructure.DatabricksClient;
using Microsoft.Azure.Databricks.Client;
using Newtonsoft.Json;

namespace DatabricksClientManager;

/// <summary>
/// There is currently no Databricks environment for integration testing.
/// This class functions as a web server for Databricks REST API, mocking the calls.
/// The API exposes a single CalculatorJob and allows for triggering runs using its job id.
/// </summary>
public sealed class DatabricksHttpListener : IDisposable
{
    // https://github.com/Azure/azure-databricks-client/blob/master/csharp/Microsoft.Azure.Databricks.Client/JobsApiClient.cs
    private const int JobId = 42;
    private readonly HttpListener _listener;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly List<RunIdentifier> _runs = new();

    public DatabricksHttpListener(string prefix)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(prefix);
    }

    public async Task BeginListenAsync()
    {
        _listener.Start();

        var requests = new HashSet<Task> { _listener.GetContextAsync() };

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            var t = await Task.WhenAny(requests).ConfigureAwait(false);
            requests.Remove(t);

            if (t is Task<HttpListenerContext> taskRequest)
            {
                requests.Add(ProcessRequestAsync(await taskRequest.ConfigureAwait(false)));
                requests.Add(_listener.GetContextAsync());
            }
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        ((IDisposable)_listener).Dispose();
        ((IDisposable)_cancellationTokenSource).Dispose();
    }

    private Task ProcessRequestAsync(HttpListenerContext context)
    {
        if (context.Request.RawUrl == null)
        {
            context.Response.StatusCode = 500;
            context.Response.Close();
            return Task.CompletedTask;
        }

        if (context.Request.RawUrl.Contains("jobs/list"))
        {
            HandleJobListRequest(context);
        }
        else if (context.Request.RawUrl.Contains("jobs/get?"))
        {
            HandleJobGetRequest(context);
        }
        else if (context.Request.RawUrl.Contains("jobs/run-now"))
        {
            HandleJobRunNowRequest(context);
        }
        else if (context.Request.RawUrl.Contains("jobs/runs/get?"))
        {
            HandleJobRunGetRequest(context);
        }
        else
        {
            // request is unknown
            context.Response.StatusCode = 500;
            context.Response.Close();
        }

        return Task.CompletedTask;
    }

    private void HandleJobGetRequest(HttpListenerContext context)
    {
        var id = long.Parse(context.Request.QueryString["job_id"] ?? string.Empty);

        if (VerifyJobId(context, id))
            return;

        var calculatorJob = CreateCalculatorJob();
        var serialized = JsonConvert.SerializeObject(calculatorJob);

        context.Response.StatusCode = 200;
        context.Response.Close(Encoding.UTF8.GetBytes(serialized), true);
    }

    private void HandleJobRunGetRequest(HttpListenerContext context)
    {
        var id = long.Parse(context.Request.QueryString["run_id"] ?? string.Empty);

        if (VerifyRunId(context, id))
            return;

        var run = new Run { RunId = id, State = new RunState { ResultState = RunResultState.SUCCESS } };

        var serialized = JsonConvert.SerializeObject(run);

        context.Response.StatusCode = 200;
        context.Response.Close(Encoding.UTF8.GetBytes(serialized), true);
    }

    private static void HandleJobListRequest(HttpListenerContext context)
    {
        var calculatorJob = CreateCalculatorJob();
        var serialized = JsonConvert.SerializeObject(new { jobs = new[] { calculatorJob } });

        context.Response.StatusCode = 200;
        context.Response.Close(Encoding.UTF8.GetBytes(serialized), true);
    }

    private static WheelJob CreateCalculatorJob()
    {
        var calculatorJob = new WheelJob
        {
            JobId = JobId,
            Settings =
                new WheelJobSettings
                {
                    Name = "CalculatorJob",
                    PythonWheelTask = new PythonWheelTask { Parameters = new List<string>() },
                },
        };
        return calculatorJob;
    }

    private bool VerifyJobId(HttpListenerContext context, long id)
    {
        if (JobId != id)
        {
            FakeServerErrorAndClose(context);
            return true;
        }

        return false;
    }

    private bool VerifyRunId(HttpListenerContext context, long id)
    {
        if (_runs.All(x => x.RunId != id))
        {
            FakeServerErrorAndClose(context);
            return true;
        }

        return false;
    }

    private void HandleJobRunNowRequest(HttpListenerContext context)
    {
        if (VerifyJobRequest(context))
            return;

        var runIdentifier = new RunIdentifier { RunId = Random.Shared.NextInt64() };
        _runs.Add(runIdentifier);

        var serialized = JsonConvert.SerializeObject(runIdentifier);

        context.Response.StatusCode = 200;
        context.Response.Close(Encoding.UTF8.GetBytes(serialized), true);
    }

    private static bool VerifyJobRequest(HttpListenerContext context)
    {
        var reader = new StreamReader(context.Request.InputStream);
        var settings = reader.ReadToEnd();
        var actualSettings = JsonConvert.DeserializeObject<RunNowSettings>(settings);
        if (actualSettings!.JobId != JobId)
        {
            FakeServerErrorAndClose(context);
            return true;
        }

        return false;
    }

    private static void FakeServerErrorAndClose(HttpListenerContext context)
    {
        context.Response.StatusCode = 500;
        context.Response.Close();
    }
}
