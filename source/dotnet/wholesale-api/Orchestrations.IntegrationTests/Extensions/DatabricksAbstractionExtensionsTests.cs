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
public class DatabricksAbstractionExtensionsTests : IClassFixture<WireMockExtensionsFixture>
{
    private readonly WireMockExtensionsFixture _fixture;

    public DatabricksAbstractionExtensionsTests(WireMockExtensionsFixture fixture)
    {
        _fixture = fixture;

        // Clear mappings etc. before each test
        _fixture.MockServer.Reset();
    }

    [Fact]
    public async Task MockCalculationJobStatusResponse_WhenCallingJobsRunsGet_CanDeserializeResponseFromMock()
    {
        // Arrange
        var runId = Random.Shared.Next(0, 1000);
        _fixture.MockServer
            .MockCalculationJobStatusResponse(RunLifeCycleState.TERMINATED, runId);

        // Act
        var actualRunTupleTask = _fixture.JobApiClient.Jobs.RunsGet(runId);
        var actualRunTuple = await actualRunTupleTask;

        // Assert
        using var assertionScope = new AssertionScope();
        actualRunTuple.Item1.RunId.Should().Be(runId);
        actualRunTuple.Item1.State.LifeCycleState.Should().Be(RunLifeCycleState.TERMINATED);
        actualRunTuple.Item1.State.ResultState.Should().Be(RunResultState.SUCCESS);
    }

    [Fact]
    public async Task MockCalculationJobStatusResponseWithStateCallback_WhenCallingJobsRunsGet_CanDeserializeResponseFromMock()
    {
        // Arrange
        var runId = Random.Shared.Next(0, 1000);
        var lifeCycleStateCallback = new CallbackValue<RunLifeCycleState?>(null);
        _fixture.MockServer
            .MockCalculationJobStatusResponse(lifeCycleStateCallback.GetValue, runId);

        // Act
        var actualRunTupleTask = _fixture.JobApiClient.Jobs.RunsGet(runId);
        lifeCycleStateCallback.SetValue(RunLifeCycleState.TERMINATED);
        var actualRunTuple = await actualRunTupleTask;

        // Assert
        using var assertionScope = new AssertionScope();
        actualRunTuple.Item1.RunId.Should().Be(runId);
        actualRunTuple.Item1.State.LifeCycleState.Should().Be(RunLifeCycleState.TERMINATED);
        actualRunTuple.Item1.State.ResultState.Should().Be(RunResultState.SUCCESS);
    }

    [Fact]
    public async Task MockCalculationJobStatusResponsesWithLifecycle_WhenCallingJobsRunsGet_CanDeserializeResponseFromMockForEachState()
    {
        // Arrange
        var runId = Random.Shared.Next(0, 1000);
        _fixture.MockServer
            .MockCalculationJobStatusResponsesWithLifecycle(runId);

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
}
