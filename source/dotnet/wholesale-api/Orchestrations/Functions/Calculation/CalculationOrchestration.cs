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

using Energinet.DataHub.EnergySupplying.RequestResponse.InboxEvents;
using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Activities;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation;

internal class CalculationOrchestration
{
    [Function(nameof(CalculationOrchestration))]
    public async Task<string> Run(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var input = context.GetInput<CalculationOrchestrationInput>();
        if (input == null)
        {
            return "Error: No input specified.";
        }

        // Create calculation (SQL)
        var calculationMetadata = await context.CallActivityAsync<CalculationMetadata>(
            nameof(CreateCalculationRecordActivity),
            input);
        calculationMetadata.OrchestrationProgress = "CalculationCreated";
        context.SetCustomStatus(calculationMetadata);

        // Start calculation (Databricks)
        calculationMetadata.JobId = await context.CallActivityAsync<CalculationJobId>(
            nameof(StartCalculationActivity),
            calculationMetadata.Id);
        calculationMetadata.OrchestrationProgress = "CalculationJobQueued";
        context.SetCustomStatus(calculationMetadata);

        var expiryTime = context.CurrentUtcDateTime.AddSeconds(input.JobStatusMonitorOptions.ExpiryTimeInSeconds);
        while (context.CurrentUtcDateTime < expiryTime)
        {
            // Monitor calculation (Databricks)
            calculationMetadata.JobStatus = await context.CallActivityAsync<CalculationState>(
                nameof(GetJobStatusActivity),
                calculationMetadata.JobId);
            context.SetCustomStatus(calculationMetadata);

            if (calculationMetadata.JobStatus is CalculationState.Running
                or CalculationState.Pending
                or CalculationState.Canceled)
            {
                // Update calculation execution status (SQL)
                await context.CallActivityAsync(
                    nameof(UpdateCalculationStatusActivity),
                    calculationMetadata);

                if (calculationMetadata.JobStatus is CalculationState.Canceled)
                {
                    // (Re) Start calculation (Databricks)
                    calculationMetadata.JobId = await context.CallActivityAsync<CalculationJobId>(
                        nameof(StartCalculationActivity),
                        calculationMetadata.Id);
                    calculationMetadata.OrchestrationProgress = "CalculationJobQueuedAgain";
                    context.SetCustomStatus(calculationMetadata);
                }

                // Wait for the next checkpoint
                var nextCheckpoint = context.CurrentUtcDateTime.AddSeconds(input.JobStatusMonitorOptions.PollingIntervalInSeconds);
                await context.CreateTimer(nextCheckpoint, CancellationToken.None);
            }
            else
            {
                break;
            }
        }

        // Update calculation execution status (SQL)
        await context.CallActivityAsync(
            nameof(UpdateCalculationStatusActivity),
            calculationMetadata);

        if (calculationMetadata.JobStatus == CalculationState.Completed)
        {
            calculationMetadata.OrchestrationProgress = "CalculationJobCompleted";
            context.SetCustomStatus(calculationMetadata);

            // OBSOLETE: Create calculation completed (SQL - Event database)
            await context.CallActivityAsync(
                nameof(CreateCompletedCalculationActivity),
                new CreateCompletedCalculationInput(calculationMetadata.Id, context.InstanceId));

            //// TODO: Wait for warehouse to start (could use retry policy); could be done using fan-out/fan-in

            // Send calculation results (ServiceBus)
            await context.CallActivityAsync(
                nameof(SendCalculationResultsActivity),
                calculationMetadata.Id);
            calculationMetadata.OrchestrationProgress = "ActorMessagesEnqueuing";

            context.SetCustomStatus(calculationMetadata);
        }
        else
        {
            calculationMetadata.OrchestrationProgress = "CalculationJobFailed";
            context.SetCustomStatus(calculationMetadata);
            return $"Error: Job status '{calculationMetadata.JobStatus}'";
        }

        // Wait for an ActorMessagesEnqueued event to notify us that messages are ready to be consumed by actors
        // Pattern #5: Human interaction - https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview?tabs=isolated-process%2Cnodejs-v3%2Cv1-model&pivots=csharp#human
        var waitForActorMessagesEnqueuedEventResult = await WaitForActorMessagesEnqueuedEvent(context, calculationMetadata.Id);
        if (!waitForActorMessagesEnqueuedEventResult.IsSuccess)
        {
            calculationMetadata.OrchestrationProgress = waitForActorMessagesEnqueuedEventResult.ErrorSubject ?? "UnknownWaitForActorMessagesEnqueuedEventError";
            context.SetCustomStatus(calculationMetadata);
            return $"Error: {waitForActorMessagesEnqueuedEventResult.ErrorDescription ?? "Unknown error waiting for actor messages enqueued event"}";
        }

        calculationMetadata.OrchestrationProgress = "ActorMessagesEnqueued";
        context.SetCustomStatus(calculationMetadata);

        // Update calculation state to ActorMessagesEnqueued in database
        await context.CallActivityAsync(
            nameof(SetCalculationOrchestrationStateActivity),
            new SetCalculationOrchestrationStateInput(calculationMetadata.Id, CalculationOrchestrationState.ActorMessagesEnqueued));

        calculationMetadata.OrchestrationProgress = "Completed";
        context.SetCustomStatus(calculationMetadata);
        // Set calculation orchestration status to completed
        await context.CallActivityAsync(
            nameof(SetCalculationOrchestrationStateActivity),
            new SetCalculationOrchestrationStateInput(calculationMetadata.Id, CalculationOrchestrationState.Completed));

        return "Success";
    }

    private static async Task<OrchestrationResult> WaitForActorMessagesEnqueuedEvent(
        TaskOrchestrationContext context,
        Guid calculationId)
    {
        using (var timeoutCts = new CancellationTokenSource())
        {
            // TODO: Get timeout from config
            var timeoutAt = context.CurrentUtcDateTime.AddHours(1);

            var waitForTimeoutTask = context.CreateTimer(timeoutAt, timeoutCts.Token);

            // ReSharper disable once MethodSupportsCancellation
            // Cancellation is handled by the waitForTimeoutTask, so the cancellation token shouldn't be passed to waiting for the actual event
            var waitForMessagesEnqueuedEventTask = context.WaitForExternalEvent<MessagesEnqueuedV1>(MessagesEnqueuedV1.EventName);

            var finishedTask = await Task.WhenAny(waitForMessagesEnqueuedEventTask, waitForTimeoutTask);
            if (finishedTask == waitForMessagesEnqueuedEventTask)
            {
                var messagesEnqueuedEvent = waitForMessagesEnqueuedEventTask.Result;
                var canParseCalculationId = Guid.TryParse(messagesEnqueuedEvent.CalculationId, out var messagesEnqueuedCalculationId);
                if (!canParseCalculationId || messagesEnqueuedCalculationId != calculationId)
                {
                    return OrchestrationResult.Error("ActorMessagesEnqueuedCalculationIdMismatch", $"Calculation id mismatch for actor messages enqueued event (expected: {calculationId}, actual: {messagesEnqueuedEvent.CalculationId})");
                }
            }
            else
            {
                return OrchestrationResult.Error("ActorMessagesEnqueuingTimeout", "Timeout while waiting for actor messages enqueued event");
            }

            // ReSharper disable once MethodHasAsyncOverload -- Do not use .CanceAsync() since it is not a durable task and will cause the the durable function to fail
            timeoutCts.Cancel();
        }

        return OrchestrationResult.Success();
    }
}
