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
using Microsoft.Net.Http.Headers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;

/// <summary>
/// A collection of WireMock extensions for easy mock configuration of
/// Databricks REST API endpoints.
/// </summary>
public static class DatabricksApiMockExtensions
{
    public static WireMockServer StubJobsList(this WireMockServer server, long jobId)
    {
        var jobsListRequest = Request
            .Create()
            .WithPath("/api/2.1/jobs/list")
            .UsingGet();

        var jobsListResponse = Response
            .Create()
            .WithStatusCode(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, "application/json")
            .WithBody(BuildJobsListJson(jobId));

        server
            .Given(jobsListRequest)
            .RespondWith(jobsListResponse);

        return server;
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
}
