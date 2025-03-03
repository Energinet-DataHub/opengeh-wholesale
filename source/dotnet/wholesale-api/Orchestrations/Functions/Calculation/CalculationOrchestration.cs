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
    public Task<string> Run(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        throw new Exception("Calculations should no longer run using Wholesale, they should run in the Process Manager subsystem instead");
        // var input = context.GetInput<CalculationOrchestrationInput>();
        // if (input == null)
        // {
        //     return "Error: No input specified.";
        // }
        //
        // var defaultRetryOptions = CreateDefaultRetryOptions();
        //
        // // Set instance id on calculation
        // await context.CallActivityAsync(
        //     nameof(SetCalculationAsStartedActivity),
        //     new SetCalculationAsStartedInput(input.CalculationId.Id, context.InstanceId),
        //     defaultRetryOptions);
        //
        // // Set custom calculation. This is being waited for in the ScheduledCalculationTrigger to ensure that the calculation is started.
        // var calculationMetadata = new CalculationMetadata
        // {
        //     IsStarted = true,
        //     Id = input.CalculationId.Id,
        //     Input = input,
        //     OrchestrationProgress = "OrchestrationStarted",
        // };
        // context.SetCustomStatus(calculationMetadata);
        //
        // // Start calculation (Databricks)
        // calculationMetadata.JobId = await context.CallActivityAsync<CalculationJobId>(
        //     nameof(StartCalculationActivity),
        //     calculationMetadata.Id,
        //     defaultRetryOptions);
        // calculationMetadata.OrchestrationProgress = "CalculationJobQueued";
        // context.SetCustomStatus(calculationMetadata);
        //
        // var expiryTime =
        //     context.CurrentUtcDateTime.AddSeconds(input.OrchestrationMonitorOptions
        //         .CalculationJobStatusExpiryTimeInSeconds);
        // while (context.CurrentUtcDateTime < expiryTime)
        // {
        //     // Monitor calculation (Databricks)
        //     calculationMetadata.JobStatus = await context.CallActivityAsync<CalculationState>(
        //         nameof(GetJobStatusActivity),
        //         calculationMetadata.JobId,
        //         defaultRetryOptions);
        //     context.SetCustomStatus(calculationMetadata);
        //
        //     if (calculationMetadata.JobStatus is CalculationState.Running
        //         or CalculationState.Pending
        //         or CalculationState.Canceled)
        //     {
        //         // Update calculation execution status (SQL)
        //         await context.CallActivityAsync(
        //             nameof(UpdateCalculationStateFromJobStatusActivity),
        //             calculationMetadata,
        //             defaultRetryOptions);
        //
        //         if (calculationMetadata.JobStatus is CalculationState.Canceled)
        //         {
        //             // (Re) Start calculation (Databricks)
        //             calculationMetadata.JobId = await context.CallActivityAsync<CalculationJobId>(
        //                 nameof(StartCalculationActivity),
        //                 calculationMetadata.Id,
        //                 defaultRetryOptions);
        //             calculationMetadata.OrchestrationProgress = "CalculationJobQueuedAgain";
        //             context.SetCustomStatus(calculationMetadata);
        //         }
        //
        //         // Wait for the next checkpoint
        //         var nextCheckpoint = context.CurrentUtcDateTime.AddSeconds(input.OrchestrationMonitorOptions
        //             .CalculationJobStatusPollingIntervalInSeconds);
        //         await context.CreateTimer(nextCheckpoint, CancellationToken.None);
        //     }
        //     else
        //     {
        //         break;
        //     }
        // }
        //
        // // Update calculation execution status (SQL)
        // await context.CallActivityAsync(
        //     nameof(UpdateCalculationStateFromJobStatusActivity),
        //     calculationMetadata,
        //     defaultRetryOptions);
        //
        // if (calculationMetadata.JobStatus == CalculationState.Completed)
        // {
        //     calculationMetadata.OrchestrationProgress = "CalculationJobCompleted";
        //     context.SetCustomStatus(calculationMetadata);
        //
        //     // If the calculation is internal, we don't want to send the calculation results event.
        //     if (!input.IsInternalCalculation)
        //     {
        //         // Send calculation results (ServiceBus)
        //         await context.CallActivityAsync(
        //             nameof(SendCalculationResultsActivity),
        //             new SendCalculationResultsInput(calculationMetadata.Id, context.InstanceId),
        //             defaultRetryOptions);
        //
        //         await UpdateCalculationOrchestrationStateAsync(
        //             context,
        //             calculationMetadata.Id,
        //             CalculationOrchestrationState.ActorMessagesEnqueuing,
        //             defaultRetryOptions);
        //
        //         calculationMetadata.OrchestrationProgress = "ActorMessagesEnqueuing";
        //
        //         context.SetCustomStatus(calculationMetadata);
        //     }
        // }
        // else
        // {
        //     calculationMetadata.OrchestrationProgress = "CalculationJobFailed";
        //     context.SetCustomStatus(calculationMetadata);
        //     return $"Error: Job status '{calculationMetadata.JobStatus}'";
        // }
        //
        // // If the calculation is internal, we are not enqueueing messages to the actors
        // if (!input.IsInternalCalculation)
        // {
        //     // Wait for an ActorMessagesEnqueued event to notify us that messages are ready to be consumed by actors
        //     var waitForActorMessagesEnqueuedEventResult = await WaitForActorMessagesEnqueuedEventAsync(
        //         context,
        //         calculationMetadata.Id,
        //         input.OrchestrationMonitorOptions.MessagesEnqueuingExpiryTimeInSeconds);
        //     if (!waitForActorMessagesEnqueuedEventResult.IsSuccess)
        //     {
        //         calculationMetadata.OrchestrationProgress = waitForActorMessagesEnqueuedEventResult.ErrorSubject ??
        //                                                     "UnknownWaitForActorMessagesEnqueuedEventError";
        //         context.SetCustomStatus(calculationMetadata);
        //         await UpdateCalculationOrchestrationStateAsync(
        //             context,
        //             calculationMetadata.Id,
        //             CalculationOrchestrationState.ActorMessagesEnqueuingFailed,
        //             defaultRetryOptions);
        //         return
        //             $"Error: {waitForActorMessagesEnqueuedEventResult.ErrorDescription ?? "Unknown error waiting for actor messages enqueued event"}";
        //     }
        //
        //     // Update state to ActorMessagesEnqueued
        //     calculationMetadata.OrchestrationProgress = "ActorMessagesEnqueued";
        //     context.SetCustomStatus(calculationMetadata);
        //     await UpdateCalculationOrchestrationStateAsync(
        //         context,
        //         calculationMetadata.Id,
        //         CalculationOrchestrationState.ActorMessagesEnqueued,
        //         defaultRetryOptions);
        // }
        //
        // // Update state to Completed
        // calculationMetadata.OrchestrationProgress = "Completed";
        // context.SetCustomStatus(calculationMetadata);
        // await UpdateCalculationOrchestrationStateAsync(
        //     context,
        //     calculationMetadata.Id,
        //     CalculationOrchestrationState.Completed,
        //     defaultRetryOptions);
        //
        // return "Success";
    }

    private static async Task UpdateCalculationOrchestrationStateAsync(
        TaskOrchestrationContext context,
        Guid calculationId,
        CalculationOrchestrationState newState,
        TaskOptions retryOptions)
    {
        await context.CallActivityAsync(
            nameof(UpdateCalculationOrchestrationStateActivity),
            new UpdateCalculationOrchestrationStateInput(calculationId, newState),
            retryOptions);
    }

#pragma warning disable CS1570 // XML comment has badly formed XML -- XML doesn't like links
    /// <summary>
    /// Pattern #5: Human interaction - https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview?tabs=isolated-process%2Cnodejs-v3%2Cv1-model&pivots=csharp#human
    /// </summary>
    private static async Task<OrchestrationResult> WaitForActorMessagesEnqueuedEventAsync(
        TaskOrchestrationContext context,
        Guid calculationId,
        int messagesEnqueuingExpiryTimeInSeconds)
    {
        ActorMessagesEnqueuedV1 messagesEnqueuedEvent;

        var timeoutLimit = TimeSpan.FromSeconds(messagesEnqueuingExpiryTimeInSeconds);
        try
        {
            messagesEnqueuedEvent = await context.WaitForExternalEvent<ActorMessagesEnqueuedV1>(
                ActorMessagesEnqueuedV1.EventName,
                timeout: timeoutLimit);
        }
        catch (TaskCanceledException taskCanceledException)
        {
            return OrchestrationResult.Error(
                "ActorMessagesEnqueuingTimeout",
                $"Timeout while waiting for actor messages enqueued event. Timeout limit: {timeoutLimit}, exception: {taskCanceledException}");
        }

        var canParseCalculationId =
            Guid.TryParse(messagesEnqueuedEvent.CalculationId, out var messagesEnqueuedCalculationId);
        if (!canParseCalculationId || messagesEnqueuedCalculationId != calculationId)
        {
            return OrchestrationResult.Error(
                "ActorMessagesEnqueuedCalculationIdMismatch",
                $"Calculation id mismatch for actor messages enqueued event (expected: {calculationId}, actual: {messagesEnqueuedEvent.CalculationId})");
        }

        if (!messagesEnqueuedEvent.Success)
        {
            return OrchestrationResult.Error(
                "ActorMessagesEnqueuingFailed",
                "ActorMessagesEnqueuedV1 event was not success");
        }

        return OrchestrationResult.Success();
    }
#pragma warning restore CS1570 // XML comment has badly formed XML

    private static TaskOptions CreateDefaultRetryOptions()
    {
        return TaskOptions.FromRetryPolicy(new RetryPolicy(
            maxNumberOfAttempts: 5,
            firstRetryInterval: TimeSpan.FromSeconds(30),
            backoffCoefficient: 2.0));
    }
}
