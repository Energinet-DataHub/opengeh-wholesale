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
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Microsoft.Net.Http.Headers;
using WireMock.Models;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions;

/// <summary>
/// A collection of WireMock extensions for easy mock configuration of
/// Databricks REST API endpoints.
///
/// IMPORTANT developer tips:
///  - It's possible to start the WireMock server in Proxy mode, this means
///    that all requests are proxied to the real URL. And the mappings can be recorded and saved.
///    See https://github.com/WireMock-Net/WireMock.Net/wiki/Proxying
///  - WireMockInspector: https://github.com/WireMock-Net/WireMockInspector/blob/main/README.md
///  - WireMock.Net examples: https://github.com/WireMock-Net/WireMock.Net-examples
/// </summary>
public static class DatabricksApiWireMockExtensions
{
    public static WireMockServer MockJobsList(this WireMockServer server, long jobId)
    {
        var request = Request
            .Create()
            .WithPath("/api/2.1/jobs/list")
            .UsingGet();

        var response = Response
            .Create()
            .WithStatusCode(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, "application/json")
            .WithBody(BuildJobsListJson(jobId));

        server
            .Given(request)
            .RespondWith(response);

        return server;
    }

    public static WireMockServer MockJobsGet(this WireMockServer server, long jobId)
    {
        var request = Request
            .Create()
            .WithPath("/api/2.1/jobs/get")
            .WithParam("job_id", jobId.ToString())
            .UsingGet();

        var response = Response
            .Create()
            .WithStatusCode(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, "application/json")
            .WithBody(BuildJobsGetJson(jobId));

        server
            .Given(request)
            .RespondWith(response);

        return server;
    }

    public static WireMockServer MockJobsRunNow(this WireMockServer server, long runId)
    {
        var request = Request
            .Create()
            .WithPath("/api/2.1/jobs/run-now")
            .UsingPost();

        var response = Response
            .Create()
            .WithStatusCode(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, "application/json")
            .WithBody(BuildJobsRunNowJson(runId));

        server
            .Given(request)
            .RespondWith(response);

        return server;
    }

    public static WireMockServer MockJobsRunsGet(this WireMockServer server, long runId, string lifeCycleState, string resultState)
    {
        var request = Request
            .Create()
            .WithPath("/api/2.1/jobs/runs/get")
            .WithParam("run_id", runId.ToString())
            .UsingGet();

        var response = Response
            .Create()
            .WithStatusCode(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, "application/json")
            .WithBody(BuildJobsRunsGetJson(runId, lifeCycleState, resultState));

        server
            .Given(request)
            .RespondWith(response);

        return server;
    }

    public static WireMockServer MockJobsRunsGet(this WireMockServer server, long runId, Func<string?> getLifeCycleState)
    {
        var request = Request
            .Create()
            .WithPath("/api/2.1/jobs/runs/get")
            .WithParam("run_id", runId.ToString())
            .UsingGet();

        var response = Response
            .Create()
            .WithStatusCode(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, "application/json")
            .WithBody(async _ =>
            {
                await Awaiter.WaitUntilConditionAsync(
                    () => getLifeCycleState() != null,
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromMilliseconds(500));

                var lifeCycleState = getLifeCycleState() ??
                        throw new Exception("LifeCycleState is null, waiting for LifeCycleState state failed");

                var resultState = "EXCLUDED";
                if (lifeCycleState == "TERMINATED")
                    resultState = "SUCCESS";

                return BuildJobsRunsGetJson(runId, lifeCycleState, resultState);
            });

        server
            .Given(request)
            .RespondWith(response);

        return server;
    }

    /// <summary>
    /// JobStatusLifeCycle goes through "Pending" -> "Running" -> "Completed".
    /// </summary>
    public static WireMockServer MockJobsRunsGetLifeCycleScenario(this WireMockServer server, long runId)
    {
        var request = Request
            .Create()
            .WithPath("/api/2.1/jobs/runs/get")
            .WithParam("run_id", runId.ToString())
            .UsingGet();

        // Pending
        server
            .Given(request)
            .InScenario("JobStatusLifeCycle")
            .WillSetStateTo(Calculations.Application.Model.CalculationState.Pending.ToString())
            .RespondWith(Response
                .Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader(HeaderNames.ContentType, "application/json")
                .WithBody(BuildJobsRunsGetJson(runId, "PENDING", "EXCLUDED")));

        // Running
        server
            .Given(request)
            .InScenario("JobStatusLifeCycle")
            .WhenStateIs(Calculations.Application.Model.CalculationState.Pending.ToString())
            .WillSetStateTo(Calculations.Application.Model.CalculationState.Running.ToString())
            .RespondWith(Response
                .Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader(HeaderNames.ContentType, "application/json")
                .WithBody(BuildJobsRunsGetJson(runId, "RUNNING", "EXCLUDED")));

        // Completed
        server
            .Given(request)
            .InScenario("JobStatusLifeCycle")
            .WhenStateIs(Calculations.Application.Model.CalculationState.Running.ToString())
            .WillSetStateTo(Calculations.Application.Model.CalculationState.Completed.ToString())
            .RespondWith(Response
                .Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader(HeaderNames.ContentType, "application/json")
                .WithBody(BuildJobsRunsGetJson(runId, "TERMINATED", "SUCCESS")));

        return server;
    }

    public static WireMockServer MockEnergySqlStatements(this WireMockServer server, string statementId, int chunkIndex)
    {
        var request = Request
            .Create()
            .WithPath("/api/2.0/sql/statements")
            .UsingPost();

        var response = Response
            .Create()
            .WithStatusCode(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, "application/json")
            .WithBody(DatabricksEnergyStatementResponseMock(statementId, chunkIndex));

        server
            .Given(request)
            .RespondWith(response);

        return server;
    }

    public static WireMockServer MockEnergySqlStatementsResultChunks(this WireMockServer server, string statementId, int chunkIndex, string path)
    {
        var request = Request
            .Create()
            .WithPath($"/api/2.0/sql/statements/{statementId}/result/chunks/{chunkIndex}")
            .UsingGet();

        var response = Response
            .Create()
            .WithStatusCode(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, "application/json")
            .WithBody(DatabricksEnergyStatementExternalLinkResponseMock(chunkIndex, $"{server.Url}/{path}"));

        server
            .Given(request)
            .RespondWith(response);

        return server;
    }

    public static WireMockServer MockEnergySqlStatementsResultStream(this WireMockServer server, string path, Guid? calculationId = null)
    {
        var request = Request
            .Create()
            .WithPath($"/{path}")
            .UsingGet();

        var response = Response
            .Create()
            .WithStatusCode(HttpStatusCode.OK)
            .WithBody(Encoding.UTF8.GetBytes(DatabricksEnergyStatementRowMock(calculationId)));

        server
            .Given(request)
            .RespondWith(response);
        return server;
    }

    /// <summary>
    /// Mock databrick energy sql statement result. Waits for <paramref name="getCalculationId"/> to not return null
    /// before returning the response.
    /// </summary>
    /// <param name="server"></param>
    /// <param name="path"></param>
    /// <param name="getCalculationId">Will wait for the calculation id to have a value before returning a mocked response</param>
    public static WireMockServer MockEnergySqlStatementsResultStream(this WireMockServer server, string path, Func<Guid?> getCalculationId)
    {
        var request = Request
            .Create()
            .WithPath($"/{path}")
            .UsingGet();

        var response = Response
            .Create()
            .WithStatusCode(HttpStatusCode.OK)
            .WithBody(async (_) =>
            {
                await Awaiter.WaitUntilConditionAsync(
                    () => getCalculationId() != null,
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromMilliseconds(500));

                var calculationId = getCalculationId();
                return DatabricksEnergyStatementRowMock(calculationId!.Value);
            });

        server
            .Given(request)
            .RespondWith(response);
        return server;
    }

    /// <summary>
    /// Create a '/api/2.0/sql/statements' JSON response. With a single chunk, containing a single row.
    /// The rest is pretty much dummy data, and can be adjusted as one pleases.
    /// Note that the columns matches the 'EnergyResultColumnNames' enum. Which again matches the columns in the energy databricks table.
    /// </summary>
    public static string DatabricksEnergyStatementResponseMock(string statementId, int chunkIndex)
    {
        var json = """
               {
                 "statement_id": "{statementId}",
                 "status": {
                   "state": "SUCCEEDED"
                 },
                 "manifest": {
                   "format": "CSV",
                   "schema": {
                     "column_count": 1,
                     "columns": [
                       {columnArray}
                     ]
                   },
                   "total_chunk_count": 1,
                   "chunks": [
                     {
                       "chunk_index": {chunkIndex},
                       "row_offset": 0,
                       "row_count": 1
                     }
                   ],
                   "total_row_count": 1,
                   "total_byte_count": 293
                 },
                 "result": {
                   "external_links": [
                     {
                       "chunk_index": {chunkIndex},
                       "row_offset": 0,
                       "row_count": 100,
                       "byte_count": 293,
                       "external_link": "https://someplace.cloud-provider.com/very/long/path/...",
                       "expiration": "2023-01-30T22:23:23.140Z"
                     }
                   ]
                 }
               }
               """;

        // Make sure that the order of the names matches the order of the data defined in 'DatabricksEnergyStatementRowMock'
        var columns = string.Join(
            ",",
            EnergyResultColumnNames
                .GetAllNames()
                .Select(name => $" {{\"name\": \"{name}\" }}"));

        return json.Replace("{statementId}", statementId)
            .Replace("{chunkIndex}", chunkIndex.ToString())
            .Replace(
                "{columnArray}",
                columns);
    }

    /// <summary>
    /// Creates a '/api/2.0/sql/statements/{statementId}/result/chunks/{chunkIndex}' JSON response.
    /// Containing a list of 'external_links', which holds information about the rows one are fetching
    /// using the url defined in 'external_link', defined in the elements of 'external_links'.
    /// </summary>
    public static string DatabricksEnergyStatementExternalLinkResponseMock(int chunkIndex, string url)
    {
        var json = """
                   {
                   "external_links": [
                     {
                       "chunk_index": {chunkIndex},
                       "row_offset": 0,
                       "row_count": 1,
                       "byte_count": 246,
                       "external_link": "{url}",
                       "expiration": "2023-01-30T22:23:23.140Z"
                     }
                   ]
                   }
                   """;
        return json.Replace("{chunkIndex}", chunkIndex.ToString())
            .Replace("{url}", url);
    }

    /// <summary>
    /// Creates a JSON response of a single row in the energy databricks table.
    /// This is the data that is fetched from the 'external_link' defined in the 'DatabricksEnergyStatementExternalLinkResponseMock'.
    /// </summary>
    /// <remarks>
    /// Note that QuantityQualities is a string, containing a list of strings.
    /// </remarks>>
    private static string DatabricksEnergyStatementRowMock(Guid? calculationId = null)
    {
        // Make sure that the order of the data matches the order of the columns defined in 'DatabricksEnergyStatementResponseMock'
        var data = EnergyResultColumnNames.GetAllNames().Select(columnName => columnName switch
        {
            EnergyResultColumnNames.CalculationId => $"\"{calculationId ?? Guid.NewGuid()}\"",
            EnergyResultColumnNames.CalculationExecutionTimeStart => "\"2022-03-11T03:00:00.000Z\"",
            EnergyResultColumnNames.CalculationType => $"\"{DeltaTableCalculationType.BalanceFixing}\"",
            EnergyResultColumnNames.CalculationResultId => "\"aaaaaaaa-1111-1111-1c1c-08d3b12d4511\"",
            EnergyResultColumnNames.TimeSeriesType => $"\"{DeltaTableTimeSeriesType.Production}\"",
            EnergyResultColumnNames.GridArea => "\"805\"",
            EnergyResultColumnNames.FromGridArea => "\"900\"",
            EnergyResultColumnNames.BalanceResponsibleId => $"\"1236552000028\"",
            EnergyResultColumnNames.EnergySupplierId => "\"2236552000028\"",
            EnergyResultColumnNames.Time => "\"2022-05-16T03:00:00.000Z\"",
            EnergyResultColumnNames.Quantity => "\"1.123\"",
            EnergyResultColumnNames.QuantityQualities => "\"[\\\"missing\\\"]\"",
            EnergyResultColumnNames.AggregationLevel => "\"total_ga\"",
            EnergyResultColumnNames.MeteringPointId => "\"1234567\"",
            EnergyResultColumnNames.Resolution => "\"PT15M\"",
            _ => throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null),
        }).ToArray();
        var temp = $"""[[{string.Join(",", data)}]]""";
        return temp;
    }

    /// <summary>
    /// Creates a '/jobs/list' JSON response with exactly one 'CalculationJob'
    /// and the given job id.
    /// </summary>
    private static string BuildJobsListJson(long jobId)
    {
        var json = """
            {
              "jobs": [
                {
                  "job_id": {jobId},
                  "settings": {
                    "name": "CalculatorJob"
                  }
                }
              ],
              "has_more": false
            }
            """;

        return json.Replace("{jobId}", jobId.ToString());
    }

    /// <summary>
    /// Creates a '/jobs/get' JSON response with the given job id.
    /// </summary>
    private static string BuildJobsGetJson(long jobId)
    {
        var json = """
            {
              "job_id": {jobId},
              "settings": {
                "name": "CalculatorJob"
              }
            }
            """;

        return json.Replace("{jobId}", jobId.ToString());
    }

    /// <summary>
    /// Creates a '/jobs/run-now' JSON response with the given run id.
    /// </summary>
    private static string BuildJobsRunNowJson(long runId)
    {
        var json = """
            {
              "run_id": {runId}
            }
            """;

        return json.Replace("{runId}", runId.ToString());
    }

    /// <summary>
    /// Creates a '/jobs/runs/get' JSON response with the given run id.
    /// </summary>
    private static string BuildJobsRunsGetJson(long runId, string lifeCycleState, string resultState)
    {
        var json = """
            {
              "run_id": {runId},
              "state": {
                "life_cycle_state": "{lifeCycleState}",
                "result_state": "{resultState}"
              }
            }
            """;

        return json
            .Replace("{runId}", runId.ToString())
            .Replace("{lifeCycleState}", lifeCycleState)
            .Replace("{resultState}", resultState);
    }
}
