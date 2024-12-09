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

using Energinet.DataHub.Core.DurableFunctionApp.TestCommon.DurableTask;
using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Azure.Databricks.Client.Models;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Functions.Calculation;

[Collection(nameof(OrchestrationsAppCollectionFixture))]
public class CalculationOrchestrationActivitiesTests : IAsyncLifetime
{
    public CalculationOrchestrationActivitiesTests(
        OrchestrationsAppFixture fixture,
        ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.SetTestOutputHelper(testOutputHelper);
    }

    private OrchestrationsAppFixture Fixture { get; }

    public Task InitializeAsync()
    {
        Fixture.EnsureAppHostUsesMockedDatabricksJobs();
        Fixture.AppHostManager.ClearHostLog();

        // Clear mappings etc. before each test
        Fixture.MockServer.Reset();

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Fixture.SetTestOutputHelper(null!);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Verifies that:
    ///  - The orchestration can complete a full run.
    ///  - Every activity is executed once and in correct order.
    /// </summary>
    [Fact]
    public async Task MockExternalDependencies_WhenCallingDurableFunctionEndPoint_OrchestrationCompletes()
    {
        // Arrange
        // => Databricks Jobs API
        Fixture.MockServer.MockCalculationJobStatusResponse(RunLifeCycleState.TERMINATED); // Terminated is success

        // => Databricks SQL Statement API
        // This is the calculationId returned in the energyResult from the mocked databricks.
        // It should be set to the ID returned by the http client calling 'api/StartCalculation'.
        // The mocked response waits for this to not be null before responding, so it must be updated
        // when we have the actual id.
        var calculationIdCallback = new CallbackValue<Guid?>(null);
        Fixture.MockServer.MockEnergyResultsResponse(calculationIdCallback.GetValue);

        // Act
        var beforeOrchestrationCreated = DateTime.UtcNow;
        var calculationId = await Fixture.StartCalculationAsync();
        calculationIdCallback.SetValue(calculationId);

        // Assert
        // => Verify expected behaviour by searching the orchestration history
        var orchestrationStatus = await Fixture.DurableClient.WaitForOrchestationStartedAsync(createdTimeFrom: beforeOrchestrationCreated);

        // => Function has the expected calculation id
        var calculationMetadata = orchestrationStatus.CustomStatus.ToObject<CalculationMetadata>();
        calculationMetadata!.Id.Should().Be(calculationId);

        // => Wait for the orchestration to reach the "ActorMessagesEnqueuing" state
        await Fixture.DurableClient.WaitForCustomStatusAsync<CalculationMetadata>(orchestrationStatus.InstanceId, (status) => status.OrchestrationProgress == "ActorMessagesEnqueuing");

        // => Send "ActorMessagesEnqueued" event to Wholesale inbox
        await Fixture.WholesaleInboxQueue.SendActorMessagesEnqueuedAsync(calculationId, orchestrationStatus.InstanceId);

        // => Wait for completion, this should be fairly quick, since we have mocked databricks
        var completeOrchestrationStatus = await Fixture.DurableClient.WaitForOrchestrationCompletedAsync(
            orchestrationStatus.InstanceId,
            TimeSpan.FromMinutes(3));

        // => Expect history
        using var assertionScope = new AssertionScope();

        var activities = completeOrchestrationStatus.History
            .OrderBy(item => item["Timestamp"])
            .Select(item => item.ToObject<OrchestrationHistoryItem>())
            .ToList();

        activities.Should().NotBeNull().And.Equal(
        [
            new OrchestrationHistoryItem("ExecutionStarted", FunctionName: "CalculationOrchestration"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "SetCalculationAsStartedActivity"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "StartCalculationActivity"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "GetJobStatusActivity"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "UpdateCalculationStateFromJobStatusActivity"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "SendCalculationResultsActivity"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "UpdateCalculationOrchestrationStateActivity"),
            new OrchestrationHistoryItem("TimerCreated"), // Wait for raised event (ActorMessagesEnqueued)
            new OrchestrationHistoryItem("EventRaised", Name: "ActorMessagesEnqueuedV1"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "UpdateCalculationOrchestrationStateActivity"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "UpdateCalculationOrchestrationStateActivity"),
            new OrchestrationHistoryItem("ExecutionCompleted"),
        ]);

        // => Verify that the durable function completed successfully
        var last = completeOrchestrationStatus.History
            .OrderBy(item => item["Timestamp"])
            .Last();
        last.Value<string>("EventType").Should().Be("ExecutionCompleted");
        last.Value<string>("Result").Should().Be("Success");
    }

    /// <summary>
    /// Verifies that:
    ///  - The orchestration can complete a full run for an internal calculation.
    ///  - Every expected activity is executed once and in correct order.
    /// </summary>
    [Fact]
    public async Task MockExternalDependencies_WhenCallingDurableFunctionEndPointWithAnInternalCalculationRequest_OrchestrationCompletes()
    {
        // Arrange
        // => Databricks Jobs API
        Fixture.MockServer.MockCalculationJobStatusResponse(RunLifeCycleState.TERMINATED); // Terminated is success

        // => Databricks SQL Statement API
        // This is the calculationId returned in the energyResult from the mocked databricks.
        // It should be set to the ID returned by the http client calling 'api/StartCalculation'.
        // The mocked response waits for this to not be null before responding, so it must be updated
        // when we have the actual id.
        var calculationIdCallback = new CallbackValue<Guid?>(null);
        Fixture.MockServer.MockEnergyResultsResponse(calculationIdCallback.GetValue);

        // Act
        var beforeOrchestrationCreated = DateTime.UtcNow;
        var calculationId = await Fixture.StartCalculationAsync(isInternalCalculation: true);
        calculationIdCallback.SetValue(calculationId);

        // Assert
        // => Verify expected behaviour by searching the orchestration history
        var orchestrationStatus = await Fixture.DurableClient.WaitForOrchestationStartedAsync(createdTimeFrom: beforeOrchestrationCreated);

        // => Function has the expected calculation id
        var calculationMetadata = orchestrationStatus.CustomStatus.ToObject<CalculationMetadata>();
        calculationMetadata!.Id.Should().Be(calculationId);

        // => Wait for completion, this should be fairly quick, since we have mocked databricks
        var completeOrchestrationStatus = await Fixture.DurableClient.WaitForOrchestrationCompletedAsync(
            orchestrationStatus.InstanceId,
            TimeSpan.FromMinutes(3));

        // => Expect history
        using var assertionScope = new AssertionScope();

        var activities = completeOrchestrationStatus.History
            .OrderBy(item => item["Timestamp"])
            .Select(item => item.ToObject<OrchestrationHistoryItem>())
            .ToList();

        activities.Should().NotBeNull().And.Equal(
        [
            new OrchestrationHistoryItem("ExecutionStarted", FunctionName: "CalculationOrchestration"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "SetCalculationAsStartedActivity"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "StartCalculationActivity"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "GetJobStatusActivity"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "UpdateCalculationStateFromJobStatusActivity"),
            new OrchestrationHistoryItem("TaskCompleted", FunctionName: "UpdateCalculationOrchestrationStateActivity"),
            new OrchestrationHistoryItem("ExecutionCompleted"),
        ]);

        // => Verify that the durable function completed successfully
        var last = completeOrchestrationStatus.History
            .OrderBy(item => item["Timestamp"])
            .Last();
        last.Value<string>("EventType").Should().Be("ExecutionCompleted");
        last.Value<string>("Result").Should().Be("Success");
    }

    /// <summary>
    /// Verify the job status monitor (loop) is working with the expected job status state changes.
    /// </summary>
    [Fact]
    public async Task MockJobsRunsGetLifeCycleScenario_WhenCallingStartCalculationEndPoint_CalculationJobCompletesWithExpectedGetJobStatusActivity()
    {
        // Arrange
        // => Databricks Jobs API
        Fixture.MockServer.MockCalculationJobStatusResponsesWithLifecycle();

        // => Databricks SQL Statement API
        Fixture.MockServer.MockEnergyResultsResponse();

        // Act
        var beforeOrchestrationCreated = DateTime.UtcNow;
        var calculationId = await Fixture.StartCalculationAsync();

        // Assert
        // => Verify expected behaviour by searching the orchestration history
        var orchestrationStatus = await Fixture.DurableClient.WaitForOrchestationStartedAsync(createdTimeFrom: beforeOrchestrationCreated);

        // => Expect calculation id
        var calculationMetadata = orchestrationStatus.CustomStatus.ToObject<CalculationMetadata>();
        calculationMetadata!.Id.Should().Be(calculationId);

        // => Wait for calculation job to be completed
        var completeOrchestrationStatus = await Fixture.DurableClient.WaitForCustomStatusAsync<CalculationMetadata>(
            orchestrationStatus.InstanceId,
            status => status.JobStatus == CalculationState.Completed,
            TimeSpan.FromMinutes(1)); // We will loop at least twice to get job status

        // => Expect history
        using var assertionScope = new AssertionScope();
        var first = completeOrchestrationStatus.History.First();
        first.Value<string>("FunctionName").Should().Be("CalculationOrchestration");

        // => Job status (loop)
        var getJobStatusResults = completeOrchestrationStatus.History
            .Where(item => item.Value<string>("FunctionName") == "GetJobStatusActivity")
            .OrderBy(item => item["Timestamp"])
            .Select(item => item.Value<string>("Result"))
            .ToList();

        getJobStatusResults.Should().NotBeNull().And.Equal(
        [
            ((int)CalculationState.Pending).ToString(),
            ((int)CalculationState.Running).ToString(),
            ((int)CalculationState.Completed).ToString(),
        ]);
    }

    /// <summary>
    /// Verify the job status monitor (loop) breaks if we reach expiry time.
    /// </summary>
    [Fact]
    public async Task MockJobsRunsGetAsRunning_WhenCallingStartCalculationEndpointAndCalculationTimeoutIsExceeded_OrchestrationCompletesWithExpectedGetJobStatusActivity()
    {
        // Arrange
        // => Databricks Jobs API
        Fixture.MockServer.MockCalculationJobStatusResponse(RunLifeCycleState.RUNNING);

        // Act
        var beforeOrchestrationCreated = DateTime.UtcNow;
        var calculationId = await Fixture.StartCalculationAsync();

        // Assert
        // => Verify expected behaviour by searching the orchestration history
        var orchestrationStatus = await Fixture.DurableClient.WaitForOrchestationStartedAsync(createdTimeFrom: beforeOrchestrationCreated);

        // => Expect calculation id
        var calculationMetadata = orchestrationStatus.CustomStatus.ToObject<CalculationMetadata>();
        calculationMetadata!.Id.Should().Be(calculationId);

        // => Wait for completion
        var completeOrchestrationStatus = await Fixture.DurableClient.WaitForOrchestrationCompletedAsync(
            orchestrationStatus.InstanceId,
            TimeSpan.FromMinutes(1)); // We will loop at least until expiry time has been reached

        // => Expect history
        using var assertionScope = new AssertionScope();
        var first = completeOrchestrationStatus.History.First();
        first.Value<string>("FunctionName").Should().Be("CalculationOrchestration");

        var last = completeOrchestrationStatus.History.Last();
        var lastResult = new { EventType = last.Value<string>("EventType"), Result = last.Value<string>("Result") };
        lastResult.EventType.Should().Be("ExecutionCompleted");
        lastResult.Result.Should().Be("Error: Job status 'Running'");
    }
}
