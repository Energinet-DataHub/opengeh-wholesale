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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Azure.Databricks.Client.Models;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions;

/// <summary>
/// Tests used for playing with WireMock for mocking Databricks REST API.
/// </summary>
public class DatabricksApiWireMockExtensionsTests : IClassFixture<WireMockExtensionsFixture>
{
    private readonly WireMockExtensionsFixture _fixture;

    public DatabricksApiWireMockExtensionsTests(WireMockExtensionsFixture fixture)
    {
        _fixture = fixture;

        // Clear mappings etc. before each test
        _fixture.MockServer.Reset();
    }

    [Fact]
    public async Task CatchAll_WhenCallingJobsList_ReturnsNotImplemented()
    {
        // Arrange
        _fixture.MockServer
            .CatchAll();

        // Act
        var act = async () => await _fixture.JobApiClient.Jobs.List();

        // Assert
        await act.Should()
            .ThrowAsync<Microsoft.Azure.Databricks.Client.ClientApiException>()
            .Where(ex =>
                ex.StatusCode == System.Net.HttpStatusCode.NotImplemented
                && ex.Message.Contains("Request not mapped"));
    }

    [Fact]
    public async Task CombinedCatchAllWithMockJobsList_WhenCallingJobsList_DoesNotFallIntoCatchAll()
    {
        // Arrange
        var jobId = Random.Shared.Next(0, 1000);
        _fixture.MockServer
            .CatchAll()
            .MockJobsList(jobId);

        // Act
        var actualJobList = await _fixture.JobApiClient.Jobs.List();

        // Assert
        actualJobList.Jobs.Should().ContainSingle();
    }

    [Fact]
    public async Task MockJobsList_WhenCallingJobsList_CanDeserializeResponseFromMock()
    {
        // Arrange
        var jobId = Random.Shared.Next(0, 1000);
        _fixture.MockServer
            .MockJobsList(jobId);

        // Act
        var actualJobList = await _fixture.JobApiClient.Jobs.List();

        // Assert
        using var assertionScope = new AssertionScope();
        actualJobList.Jobs.Should().ContainSingle();

        var job = actualJobList.Jobs.First();
        job.JobId.Should().Be(jobId);
        job.Settings.Name.Should().Be("CalculatorJob");
    }

    [Fact]
    public async Task MockJobsGet_WhenCallingJobsGet_CanDeserializeResponseFromMock()
    {
        // Arrange
        var jobId = Random.Shared.Next(0, 1000);
        _fixture.MockServer
            .MockJobsGet(jobId);

        // Act
        var actualJob = await _fixture.JobApiClient.Jobs.Get(jobId);

        // Assert
        actualJob.JobId.Should().Be(jobId);
    }

    [Fact]
    public async Task MockJobsRunNow_WhenCallingJobsRunNow_CanDeserializeResponseFromMock()
    {
        // Arrange
        var anonymousJobId = 1;
        var runId = Random.Shared.Next(0, 1000);
        _fixture.MockServer
            .MockJobsRunNow(runId);

        // Act
        var actualRunId = await _fixture.JobApiClient.Jobs.RunNow(anonymousJobId);

        // Assert
        actualRunId.Should().Be(runId);
    }

    [Fact]
    public async Task MockJobsRunsGet_WhenCallingJobsRunsGet_CanDeserializeResponseFromMock()
    {
        // Arrange
        var runId = Random.Shared.Next(0, 1000);
        var lifeCycleState = "TERMINATED";
        var resultState = "SUCCESS";
        _fixture.MockServer
            .MockJobsRunsGet(runId, lifeCycleState, resultState);

        // Act
        var actualRunTuple = await _fixture.JobApiClient.Jobs.RunsGet(runId);

        // Assert
        using var assertionScope = new AssertionScope();
        actualRunTuple.Item1.RunId.Should().Be(runId);
        actualRunTuple.Item1.State.LifeCycleState.Should().Be(RunLifeCycleState.TERMINATED);
        actualRunTuple.Item1.State.ResultState.Should().Be(RunResultState.SUCCESS);
    }

    [Fact]
    public async Task MockedDataBrickSql_WhenQueringForData_CanDeserializeResponseFromMock()
    {
        // Arrange
        var statementId = "pony";
        var chunkIndex = 0;
        var path = "flamingo";
        _fixture.MockServer
            .CatchAll()
            .MockSqlStatements(statementId, chunkIndex)
            .MockSqlStatementsResultChunks(statementId, chunkIndex, path)
            .MockSqlStatementsResultStream(path);

        var query = new EnergyResultQueryStatement(
            Guid.Empty,
            new DeltaTableOptions() { SCHEMA_NAME = "empty", ENERGY_RESULTS_TABLE_NAME = "empty" });

        // Act
        var hej = await _fixture.DatabricksExecutor.ExecuteStatementAsync(query, Format.JsonArray).ToListAsync();

        var logs = _fixture.MockServer.LogEntries;
        hej.Should().NotBeNull().And.NotBeEmpty();
    }
}
