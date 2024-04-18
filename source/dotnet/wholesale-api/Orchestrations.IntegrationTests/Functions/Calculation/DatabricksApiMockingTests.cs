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
using Energinet.DataHub.Core.App.WebApp.Extensions.Options;
using Energinet.DataHub.Core.Databricks.Jobs.Abstractions;
using Energinet.DataHub.Core.Databricks.Jobs.Configuration;
using Energinet.DataHub.Core.Databricks.Jobs.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;
using FluentAssertions;
using FluentAssertions.Common;
using FluentAssertions.Execution;
using Microsoft.Azure.Databricks.Client.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Functions.Calculation;

/// <summary>
/// Tests used for playing with WireMock for mocking Databricks REST API.
/// </summary>
public class DatabricksApiMockingTests
{
    private readonly WireMockServer _server;
    private readonly IServiceCollection _services;

    public DatabricksApiMockingTests()
    {
        _server = new WiremockFixture([OrchestrationsAppFixture.LocalhostUrl]).Server;

        _services = new ServiceCollection();
        var configuration = CreateInMemoryConfigurations(new Dictionary<string, string?>()
        {
            [$"{nameof(DatabricksJobsOptions.WorkspaceUrl)}"] = OrchestrationsAppFixture.LocalhostUrl,
            [$"{nameof(DatabricksJobsOptions.WorkspaceToken)}"] = "notEmpty",
            [$"{nameof(DatabricksJobsOptions.WarehouseId)}"] = "notEmpty",
        });
        _services.AddDatabricksJobs(configuration);
        var serviceProvider = _services.BuildServiceProvider();
        JobApiClient = serviceProvider.GetRequiredService<IJobsApiClient>();
    }

    public IJobsApiClient JobApiClient { get; }

    [Fact]
    public async Task JobApiClient_WhenJobsList_CanDeserializeMockedResponse()
    {
        // Arrange
        var jobId = Random.Shared.Next(0, 1000);
        var jobsJson = GenerateMockedJobs(jobId);

        var jobsListRequest = Request
            .Create()
            .WithPath("/api/2.1/jobs/list")
            .UsingGet();

        var jobsListResponse = Response
            .Create()
            .WithStatusCode(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, "application/json")
            .WithBody(jobsJson);

        _server
            .Given(jobsListRequest)
            .RespondWith(jobsListResponse);

        // Act
        var actualJobList = await JobApiClient.Jobs.List();

        // Assert
        using var assertionScope = new AssertionScope();
        actualJobList.Jobs.Should().ContainSingle();

        var job = actualJobList.Jobs.First();
        job.JobId.Should().Be(jobId);
        job.Settings.Name.Should().Be("CalculatorJob");
    }

    private static string GenerateMockedJobs(long jobId)
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

    private static Job GenerateMockedJob(long jobId)
    {
        return new Job()
        {
            Settings = new JobSettings()
            {
                Name = "CalculatorJob",
            },
            JobId = jobId,
        };
    }

    private IConfiguration CreateInMemoryConfigurations(Dictionary<string, string?> configurations)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(configurations)
            .Build();
    }
}
