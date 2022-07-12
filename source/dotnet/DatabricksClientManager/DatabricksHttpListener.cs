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
using Microsoft.Azure.Databricks.Client;
using Newtonsoft.Json;

namespace DatabricksClientManager;

public sealed class DatabricksHttpListener : IDisposable
{
    // https://github.com/Azure/azure-databricks-client/blob/master/csharp/Microsoft.Azure.Databricks.Client/JobsApiClient.cs
    //public static async Task Main(string[] args)
    //{
    //    await Listen("http://localhost:9000/", 1, CancellationToken.None);
    //}
    private readonly HttpListener _listener;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public DatabricksHttpListener(string prefix)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(prefix);
    }

    public async Task BeginListenAsync()
    {
        _listener.Start();

        var requests = new HashSet<Task>();
        requests.Add(_listener.GetContextAsync());

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

    private static Task ProcessRequestAsync(HttpListenerContext context)
    {
        if (context.Request.RawUrl == null)
        {
            context.Response.StatusCode = 500;
            context.Response.Close();
            return Task.CompletedTask;
        }

        if (context.Request.RawUrl.Contains("jobs/list"))
        {
            HandleJoblistRequest(context);
        }
        else if (context.Request.RawUrl.Contains("jobs/run-now"))
        {
            HandleJobRunNowRequest(context);
        }
        else if (context.Request.RawUrl.Contains("jobs/runs/get?"))
        {
            HandleJobGetRequest(context);
        }
        else
        {
            // request is unknown
            context.Response.StatusCode = 500;
            context.Response.Close();
        }

        return Task.CompletedTask;
    }

    private static void HandleJobGetRequest(HttpListenerContext context)
    {
        var run = new Run { RunId = 33, State = new RunState { ResultState = RunResultState.SUCCESS } };

        var serialized = JsonConvert.SerializeObject(run);

        context.Response.StatusCode = 200;
        context.Response.Close(Encoding.UTF8.GetBytes(serialized), true);
    }

    private static void HandleJobRunNowRequest(HttpListenerContext context)
    {
        var runIdentifier = new RunIdentifier { RunId = 33 };
        var serialized = JsonConvert.SerializeObject(runIdentifier);

        context.Response.StatusCode = 200;
        context.Response.Close(Encoding.UTF8.GetBytes(serialized), true);
    }

    private static void HandleJoblistRequest(HttpListenerContext context)
    {
        var calculatorJob = new Job { JobId = 42, Settings = new JobSettings { Name = "CalculatorJob" } };

        var serialized = JsonConvert.SerializeObject(new { jobs = new[] { calculatorJob } });

        context.Response.StatusCode = 200;
        context.Response.Close(Encoding.UTF8.GetBytes(serialized), true);
    }
}
