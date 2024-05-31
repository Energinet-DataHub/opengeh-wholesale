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
using System.Net.Http.Json;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using Energinet.DataHub.EnergySupplying.RequestResponse.InboxEvents;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.DurableTask;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;
using FluentAssertions;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Functions.Calculation;

[Collection(nameof(OrchestrationsAppCollectionFixture))]
public class CalculationOrchestrationStateTests : IAsyncLifetime
{
    public CalculationOrchestrationStateTests(
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

        Fixture.ServiceBusListenerMock.ResetMessageHandlersAndReceivedMessages();

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
    ///  - The calculation state is updated as expected.
    ///  - The orchestrator completes with the expected output.
    ///  - A service bus message is sent as expected.
    /// </summary>
    [Fact]
    public async Task GivenExpectedCalculationFlow_WhenHandlingCalculationOrchestration_OrchestrationCompletesWithExpectedStateUpdatesAndServiceBusMessage()
    {
        // Arrange
        var dbContext = Fixture.DatabaseManager.CreateDbContext();

        // => Databricks Jobs API
        var jobId = Random.Shared.Next(1, 1000);
        var runId = Random.Shared.Next(1000, 2000);

        // The current databrick calculation state. Can be null, "PENDING", "RUNNING", "TERMINATED" (success)
        // The mock response will wait for the value to not be null before returning
        string? currentCalculationJobState = null;
        Fixture.MockServer
            .MockJobsList(jobId)
            .MockJobsGet(jobId)
            .MockJobsRunNow(runId)
            // ReSharper disable once AccessToModifiedClosure -- We need to modify calculation job state in outer scope
            .MockJobsRunsGet(runId, () => currentCalculationJobState);

        // => Databricks SQL Statement API
        var chunkIndex = 0;
        var statementId = Guid.NewGuid().ToString();
        var path = "GetDatabricksDataPath";

        // This is the calculationId returned in the energyResult from the mocked databricks.
        // It should match the ID returned by the http client calling 'api/StartCalculation'.
        // The mocked response waits for this to not be null before responding, so it must be updated
        // when we have the actual id.
        Guid? calculationId = null;

        Fixture.MockServer
            .MockEnergySqlStatements(statementId, chunkIndex)
            .MockEnergySqlStatementsResultChunks(statementId, chunkIndex, path)
            // ReSharper disable once AccessToModifiedClosure -- We need to modify calculation id in outer scope
            // when we get a response from 'api/StartCalculation'
            .MockEnergySqlStatementsResultStream(path, () => calculationId);

        // Act
        var beforeOrchestrationCreated = DateTime.UtcNow;
        using var startCalculationResponse = await Fixture.StartCalculation();

        // Assert
        // => Verify endpoint response
        startCalculationResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        calculationId = await startCalculationResponse.Content.ReadFromJsonAsync<Guid>();

        // => Verify expected behaviour by searching the orchestration history
        var orchestrationStatus = await Fixture.DurableClient.FindOrchestationStatusAsync(createdTimeFrom: beforeOrchestrationCreated);

        // => Function has the expected calculation id
        var calculationMetadata = orchestrationStatus.CustomStatus.ToObject<CalculationMetadata>();
        calculationMetadata!.Id.Should().Be(calculationId.Value);

        // => Calculation job hasn't started yet, state should be Scheduled
        var isScheduledState = await dbContext.WaitForCalculationWithState(calculationId.Value, CalculationOrchestrationState.Scheduled, Fixture.TestLogger);
        isScheduledState.ActualState.Should().Be(CalculationOrchestrationState.Scheduled);

        // => Calculation job is "PENDING", state should be Calculating
        currentCalculationJobState = "PENDING";
        var isStillScheduledCalculatingState = await dbContext.WaitForCalculationWithState(calculationId.Value, CalculationOrchestrationState.Scheduled, Fixture.TestLogger);
        isStillScheduledCalculatingState.ActualState.Should().Be(CalculationOrchestrationState.Scheduled);

        // => Calculation job is "RUNNING", state should be Calculating
        currentCalculationJobState = "RUNNING";
        var isCalculatingState = await dbContext.WaitForCalculationWithState(calculationId.Value, CalculationOrchestrationState.Calculating, Fixture.TestLogger);
        isCalculatingState.ActualState.Should().Be(CalculationOrchestrationState.Calculating);

        // => Calculation job is "TERMINATED" (success), state should be Calculated or ActorMessagesEnqueuing
        // The state changes from Calculated to ActorMessagesEnqueuing immediately, so we need to check for both states.
        currentCalculationJobState = "TERMINATED";
        var isCalculatedState = await dbContext.WaitForCalculationWithOneOfStates(
            calculationId.Value,
            [CalculationOrchestrationState.Calculated, CalculationOrchestrationState.ActorMessagesEnqueuing],
            Fixture.TestLogger);
        isCalculatedState.ActualState.Should().BeOneOf(
            CalculationOrchestrationState.Calculated,
            CalculationOrchestrationState.ActorMessagesEnqueuing);

        // => When the calculation result is complete, state should be ActorMessagesEnqueuing
        // We need to wait for the state change from Calculated to ActorMessagesEnqueuing if it hasn't already
        // happened in previous step
        var isActorMessagesEnqueuingState = await dbContext.WaitForCalculationWithState(calculationId.Value, CalculationOrchestrationState.ActorMessagesEnqueuing, Fixture.TestLogger);
        isActorMessagesEnqueuingState.ActualState.Should().Be(CalculationOrchestrationState.ActorMessagesEnqueuing);

        // => Raise "ActorMessagesEnqueued" event to the orchestrator
        await Fixture.DurableClient.RaiseEventAsync(
            orchestrationStatus.InstanceId,
            MessagesEnqueuedV1.EventName,
            new MessagesEnqueuedV1
            {
                CalculationId = calculationId.ToString(),
                OrchestrationInstanceId = orchestrationStatus.InstanceId,
            });

        // => Orchestration is "ActorMessagesEnqueued" or "Completed", state should be ActorMessagesEnqueued or Completed
        // The state changes from ActorMessagesEnqueued to Completed immediately, so we need to check for both states.
        var isActorMessagesEnqueuedState = await dbContext.WaitForCalculationWithOneOfStates(
            calculationId.Value,
            [CalculationOrchestrationState.ActorMessagesEnqueued, CalculationOrchestrationState.Completed],
            Fixture.TestLogger);
        isActorMessagesEnqueuedState.ActualState.Should().BeOneOf(
            CalculationOrchestrationState.ActorMessagesEnqueued,
            CalculationOrchestrationState.Completed);

        // => Orchestration is completed, state should be Completed and orchestration output should be success
        // We need to wait for the orchestration to complete if it hasn't already happened in previous step
        var completeOrchestrationStatus = await Fixture.DurableClient.WaitForInstanceCompletedAsync(
            orchestrationStatus.InstanceId,
            TimeSpan.FromMinutes(3));
        var isCompletedState = await dbContext.WaitForCalculationWithState(calculationId.Value, CalculationOrchestrationState.Completed, Fixture.TestLogger);
        isCompletedState.ActualState.Should().Be(CalculationOrchestrationState.Completed);
        completeOrchestrationStatus.Output.ToObject<string>().Should().Be("Success");

        // => Verify that the expected message was sent on the ServiceBus
        var verifyServiceBusMessages = await Fixture.ServiceBusListenerMock
            .When(msg =>
            {
                if (msg.Subject != EnergyResultProducedV2.EventName)
                {
                    return false;
                }

                var erp = EnergyResultProducedV2.Parser.ParseFrom(msg.Body);

                // This should be the calculationId in "actualResponse".
                // But the current implementation takes the calculationId from the databricks row,
                // which is mocked in this scenario. Giving us a "false" comparison here.
                return erp.CalculationId == calculationId.Value.ToString();
            })
            .VerifyCountAsync(1);

        var wait = verifyServiceBusMessages.Wait(TimeSpan.FromMinutes(1));
        wait.Should().BeTrue("We did not send the expected message on the ServiceBus");
    }
}
