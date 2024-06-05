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

using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using Energinet.DataHub.EnergySupplying.RequestResponse.InboxEvents;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.DurableTask;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Azure.Databricks.Client.Models;
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
        // The current databrick calculation state. Can be null, "PENDING", "RUNNING", "TERMINATED" (success)
        // The mock response will wait for the value to not be null before returning
        var calculationJobStateCallback = new CallbackValue<RunLifeCycleState?>(null);
        Fixture.MockServer.MockCalculationJobStatusResponse(calculationJobStateCallback.GetValue);

        // => Databricks SQL Statement API
        // This is the calculationId returned in the energyResult from the mocked databricks.
        // It should be set to the ID returned by the http client calling 'api/StartCalculation'.
        // The mocked response waits for this to not be null before responding, so it must be updated
        // when we have the actual id.
        var calculationIdCallback = new CallbackValue<Guid?>(null);
        Fixture.MockServer.MockEnergyResultsResponse(calculationIdCallback.GetValue);

        // Act
        var beforeOrchestrationCreated = DateTime.UtcNow;
        var calculationId = await Fixture.AppHostManager.StartCalculationAsync();
        calculationIdCallback.SetValue(calculationId);

        // Assert
        // => Verify expected behaviour by searching the orchestration history
        var orchestrationStatus = await Fixture.DurableClient.FindOrchestationStatusAsync(createdTimeFrom: beforeOrchestrationCreated);

        // => Function has the expected calculation id
        var calculationMetadata = orchestrationStatus.CustomStatus.ToObject<CalculationMetadata>();
        calculationMetadata!.Id.Should().Be(calculationId);

        // => Calculation job hasn't started yet, state should be Scheduled
        var isScheduledState = await dbContext.WaitForCalculationWithState(calculationId, CalculationOrchestrationState.Scheduled, Fixture.TestLogger);
        isScheduledState.ActualState.Should().Be(CalculationOrchestrationState.Scheduled);

        // => Calculation job is "PENDING", state should be Calculating
        calculationJobStateCallback.SetValue(RunLifeCycleState.PENDING);
        var isStillScheduledCalculatingState = await dbContext.WaitForCalculationWithState(calculationId, CalculationOrchestrationState.Scheduled, Fixture.TestLogger);
        isStillScheduledCalculatingState.ActualState.Should().Be(CalculationOrchestrationState.Scheduled);

        // => Calculation job is "RUNNING", state should be Calculating
        calculationJobStateCallback.SetValue(RunLifeCycleState.RUNNING);
        var isCalculatingState = await dbContext.WaitForCalculationWithState(calculationId, CalculationOrchestrationState.Calculating, Fixture.TestLogger);
        isCalculatingState.ActualState.Should().Be(CalculationOrchestrationState.Calculating);

        // => Calculation job is "TERMINATED" (success), state should be Calculated or ActorMessagesEnqueuing
        // The state changes from Calculated to ActorMessagesEnqueuing immediately, so we need to check for both states.
        calculationJobStateCallback.SetValue(RunLifeCycleState.TERMINATED);
        var isCalculatedState = await dbContext.WaitForCalculationWithOneOfStates(
            calculationId,
            [CalculationOrchestrationState.Calculated, CalculationOrchestrationState.ActorMessagesEnqueuing],
            Fixture.TestLogger);
        isCalculatedState.ActualState.Should().BeOneOf(
            CalculationOrchestrationState.Calculated,
            CalculationOrchestrationState.ActorMessagesEnqueuing);

        // => When the calculation result is complete, state should be ActorMessagesEnqueuing
        // We need to wait for the state change from Calculated to ActorMessagesEnqueuing if it hasn't already
        // happened in previous step
        var isActorMessagesEnqueuingState = await dbContext.WaitForCalculationWithState(calculationId, CalculationOrchestrationState.ActorMessagesEnqueuing, Fixture.TestLogger);
        isActorMessagesEnqueuingState.ActualState.Should().Be(CalculationOrchestrationState.ActorMessagesEnqueuing);

        // => Raise "ActorMessagesEnqueued" event to the orchestrator
        await Fixture.DurableClient.RaiseEventAsync(
            orchestrationStatus.InstanceId,
            ActorMessagesEnqueuedV1.EventName,
            new ActorMessagesEnqueuedV1
            {
                CalculationId = calculationId.ToString(),
                OrchestrationInstanceId = orchestrationStatus.InstanceId,
                Success = true,
            });

        // => Orchestration is "ActorMessagesEnqueued" or "Completed", state should be ActorMessagesEnqueued or Completed
        // The state changes from ActorMessagesEnqueued to Completed immediately, so we need to check for both states.
        var isActorMessagesEnqueuedState = await dbContext.WaitForCalculationWithOneOfStates(
            calculationId,
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
        var isCompletedState = await dbContext.WaitForCalculationWithState(calculationId, CalculationOrchestrationState.Completed, Fixture.TestLogger);
        isCompletedState.ActualState.Should().Be(CalculationOrchestrationState.Completed);
        completeOrchestrationStatus.Output.ToObject<string>().Should().Be("Success");

        // => Verify that the expected message was sent on the ServiceBus
        var verifyServiceBusMessages = await Fixture.ServiceBusListenerMock
            .When(msg =>
            {
                if (msg.Subject != CalculationCompletedV1.EventName)
                {
                    return false;
                }

                var calculationCompleted = CalculationCompletedV1.Parser.ParseFrom(msg.Body);

                // The current implementation takes the calculationId from the databricks row,
                // which is mocked in this scenario. Giving us a "false" comparison here.
                return calculationCompleted.CalculationId == calculationId.ToString();
            })
            .VerifyCountAsync(1);

        var wait = verifyServiceBusMessages.Wait(TimeSpan.FromMinutes(1));
        wait.Should().BeTrue("We did not send the expected message on the ServiceBus");
    }

    /// <summary>
    /// Verifies that:
    ///  - The calculation state is set to ActorMessagesEnqueuingFailed.
    ///  - The orchestration can complete a full run.
    ///  - The orchestrator completes with the expected error status.
    /// </summary>
    [Fact]
    public async Task GivenActorMessagesEnqueuingFailed_WhenHandlingCalculationOrchestration_OrchestrationCompletesWithActorMessagesEnqueuingFailed()
    {
        // Arrange
        var dbContext = Fixture.DatabaseManager.CreateDbContext();

        // => Databricks Jobs API, mock calculation job run as terminated (success)
        Fixture.MockServer.MockCalculationJobStatusResponse(RunLifeCycleState.TERMINATED);

        // The calculation id is a callback since we can only to set it after the calculation is started
        // (we get the calculation id from the /api/StartCalculation response)
        var calculationIdCallback = new CallbackValue<Guid?>(null);

        // => Databricks SQL Statement API
        Fixture.MockServer.MockEnergyResultsResponse(calculationIdCallback.GetValue);

        // Act
        var beforeOrchestrationCreated = DateTime.UtcNow;
        var calculationId = await Fixture.AppHostManager.StartCalculationAsync();
        calculationIdCallback.SetValue(calculationId);

        // Assert
        // => Get orchestration status for started orchestration
        var orchestrationStatus = await Fixture.DurableClient.FindOrchestationStatusAsync(createdTimeFrom: beforeOrchestrationCreated);

        // => Wait for ActorMessagesEnqueuing state
        await Fixture.DurableClient.WaitForCustomStatusAsync<CalculationMetadata>(
                orchestrationStatus.InstanceId,
                s => s.OrchestrationProgress == "ActorMessagesEnqueuing");

        // => Raise "ActorMessagesEnqueued" failed event to the orchestrator
        await Fixture.DurableClient.RaiseEventAsync(
            orchestrationStatus.InstanceId,
            ActorMessagesEnqueuedV1.EventName,
            new ActorMessagesEnqueuedV1
            {
                CalculationId = calculationId.ToString(),
                OrchestrationInstanceId = orchestrationStatus.InstanceId,
                Success = false,
            });

        // => Wait for ActorMessagesEnqueuingFailed state
        var isActorMessagesEnqueuingFailedState = await dbContext.WaitForCalculationWithState(
            calculationId,
            CalculationOrchestrationState.ActorMessagesEnqueuingFailed,
            Fixture.TestLogger);
        isActorMessagesEnqueuingFailedState.ActualState.Should().Be(CalculationOrchestrationState.ActorMessagesEnqueuingFailed);

        // => Orchestration is completed, state should still be ActorMessagesEnqueuingFailed and orchestration output should be error
        var completeOrchestrationStatus = await Fixture.DurableClient.WaitForInstanceCompletedAsync(
            orchestrationStatus.InstanceId,
            TimeSpan.FromMinutes(3));
        var isStillActorMessagesEnqueuingFailedState = await dbContext.WaitForCalculationWithState(
                calculationId,
                CalculationOrchestrationState.ActorMessagesEnqueuingFailed,
                Fixture.TestLogger);
        isStillActorMessagesEnqueuingFailedState.ActualState.Should().Be(CalculationOrchestrationState.ActorMessagesEnqueuingFailed);
        completeOrchestrationStatus.Output.ToObject<string>().Should().Be("Error: ActorMessagesEnqueuedV1 event failed");
    }
}
