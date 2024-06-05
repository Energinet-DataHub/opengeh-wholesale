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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Functions.Calculation;
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

    /// <summary>
    /// Test calling MockJobsRunsGet with a callback, so it is possible to set/update the job lifecycle state
    /// later in the test
    /// </summary>
    [Fact]
    public async Task MockJobsRunsGet_WhenCallingJobsRunsGetWithStateCallback_CanDeserializeResponseFromMock()
    {
        // Arrange
        var runId = Random.Shared.Next(0, 1000);
        var lifeCycleStateCallback = new CallbackValue<string?>(null);
        _fixture.MockServer
            .MockJobsRunsGet(runId, lifeCycleStateCallback.GetValue);

        // Set calculation job lifecycle state, so the API call can return a value
        lifeCycleStateCallback.SetValue("TERMINATED");

        // Act
        var actualRunTuple = await _fixture.JobApiClient.Jobs.RunsGet(runId);

        // Assert
        using var assertionScope = new AssertionScope();
        actualRunTuple.Item1.RunId.Should().Be(runId);
        actualRunTuple.Item1.State.LifeCycleState.Should().Be(RunLifeCycleState.TERMINATED);
        actualRunTuple.Item1.State.ResultState.Should().Be(RunResultState.SUCCESS);
    }

    [Fact]
    public async Task MockJobsRunsGet_WhenCallingJobsRunsGetLifeCycleScenario_CanDeserializeResponseFromMockForEachState()
    {
        // Arrange
        var runId = Random.Shared.Next(0, 1000);
        _fixture.MockServer
            .MockJobsRunsGetLifeCycleScenario(runId);

        // Act
        var firstRunTuple = await _fixture.JobApiClient.Jobs.RunsGet(runId);
        var secondRunTuple = await _fixture.JobApiClient.Jobs.RunsGet(runId);
        var thirdRunTuple = await _fixture.JobApiClient.Jobs.RunsGet(runId);

        // Assert
        using var assertionScope = new AssertionScope();
        firstRunTuple.Item1.RunId.Should().Be(runId);
        firstRunTuple.Item1.State.LifeCycleState.Should().Be(RunLifeCycleState.PENDING);

        secondRunTuple.Item1.RunId.Should().Be(runId);
        secondRunTuple.Item1.State.LifeCycleState.Should().Be(RunLifeCycleState.RUNNING);

        thirdRunTuple.Item1.RunId.Should().Be(runId);
        thirdRunTuple.Item1.State.LifeCycleState.Should().Be(RunLifeCycleState.TERMINATED);
        thirdRunTuple.Item1.State.ResultState.Should().Be(RunResultState.SUCCESS);
    }

    /// <summary>
    /// The mocked data we're testing goes through multiple deserializations.
    /// First we transform the json data to a "ExpandoObject" which will be mapped to a dictionary
    /// Which is what "actual" contains in the test below
    /// Afterwards we map this dictionary to an "EnergyTimeSeriesPoint", where we will deserialize the
    /// attribute "QuantityQuality".
    /// The first deserialization is being tested by calling "ExecuteStatementAsync"
    /// The second is tested by calling "CreateTimeSeriesPoint"
    /// </summary>
    [Fact]
    public async Task MockEnergySqlStatements_WhenQueryForData_CanDeserializeResponseFromMock()
    {
        // Arrange
        var statementId = "SomeIdMostLikelyGuid";
        var chunkIndex = 0;
        var path = "GetDatabricksDataPath";
        var calculationIdForEnergyResults = Guid.NewGuid();
        _fixture.MockServer
            .MockEnergySqlStatements(statementId, chunkIndex)
            .MockEnergySqlStatementsResultChunks(statementId, chunkIndex, path)
            .MockEnergySqlStatementsResultStream(path, calculationIdForEnergyResults);

        var query = new EnergyResultQueryStatement(
            Guid.Empty,
            new DeltaTableOptions() { SCHEMA_NAME = "empty", ENERGY_RESULTS_TABLE_NAME = "empty" });

        // Act
        var actual = _fixture.DatabricksExecutor.ExecuteStatementAsync(query, Format.JsonArray).ConfigureAwait(false);

        // Assert
        await foreach (var row in actual)
        {
            var databricksSqlNextRow = new DatabricksSqlRow(row);
            var timeSeriesPoint = EnergyTimeSeriesPointFactory.CreateTimeSeriesPoint(databricksSqlNextRow);
        }
    }

    /// <summary>
    /// Test calling MockEnergySqlStatements with a callback, so it is possible to set/update the calculation id later
    /// in a test (used because the calculation id is not known when the mock is initialized, only after the calculation
    /// has been started as a part of the test)
    /// </summary>
    [Fact]
    public async Task MockEnergySqlStatementsWithCalculationIdCallback_WhenQueryForData_CanDeserializeResponseFromMock()
    {
        // Arrange
        var statementId = "SomeIdMostLikelyGuid";
        var chunkIndex = 0;
        var path = "GetDatabricksDataPath";
        var calculationIdForEnergyResultsCallback = new CallbackValue<Guid?>(null);
        _fixture.MockServer
            .MockEnergySqlStatements(statementId, chunkIndex)
            .MockEnergySqlStatementsResultChunks(statementId, chunkIndex, path)
            .MockEnergySqlStatementsResultStream(path, calculationIdForEnergyResultsCallback.GetValue);

        var query = new EnergyResultQueryStatement(
            Guid.Empty,
            new DeltaTableOptions() { SCHEMA_NAME = "empty", ENERGY_RESULTS_TABLE_NAME = "empty" });

        // Act
        var actual = _fixture.DatabricksExecutor.ExecuteStatementAsync(query, Format.JsonArray).ConfigureAwait(false);

        // Set calculation id, so the statement can return
        calculationIdForEnergyResultsCallback.SetValue(Guid.NewGuid());

        // Assert
        await foreach (var row in actual)
        {
            var databricksSqlNextRow = new DatabricksSqlRow(row);
            var timeSeriesPoint = EnergyTimeSeriesPointFactory.CreateTimeSeriesPoint(databricksSqlNextRow);
        }
    }
}
