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

using Energinet.DataHub.Core.Databricks.Jobs.Abstractions;
using Energinet.DataHub.Core.Databricks.Jobs.Configuration;
using Energinet.DataHub.Core.Databricks.Jobs.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WireMock.Server;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions;

/// <summary>
/// Tests used for playing with WireMock for mocking Databricks REST API.
/// </summary>
public class DatabricksApiWireMockExtensionsTests
{
    private readonly WireMockServer _server;
    private readonly IServiceCollection _services;

    public DatabricksApiWireMockExtensionsTests()
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
    public async Task StubJobsList_WhenCallingJobsList_CanDeserializeStubbedResponse()
    {
        // Arrange
        var jobId = Random.Shared.Next(0, 1000);
        _server.StubJobsList(jobId);

        // Act
        var actualJobList = await JobApiClient.Jobs.List();

        // Assert
        using var assertionScope = new AssertionScope();
        actualJobList.Jobs.Should().ContainSingle();

        var job = actualJobList.Jobs.First();
        job.JobId.Should().Be(jobId);
        job.Settings.Name.Should().Be("CalculatorJob");
    }

    private IConfiguration CreateInMemoryConfigurations(Dictionary<string, string?> configurations)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(configurations)
            .Build();
    }
}
