﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
internal class CalculationOrchestration
{
    [Function(nameof(Calculation))]
    public async Task<string> Calculation(
        [OrchestrationTrigger] TaskOrchestrationContext context,
        FunctionContext executionContext)
    {
        var batchRequestDto = context.GetInput<BatchRequestDto>();
        if (batchRequestDto == null)
        {
            return "Error: No input specified.";
        }

        // Replay safe logger, only logging when not replaying previous history
        var logger = context.CreateReplaySafeLogger<CalculationOrchestration>();
        logger.LogInformation($"{nameof(batchRequestDto)}: {batchRequestDto}.");

        // Create calculation (SQL)
        var calculationMetaData = await context.CallActivityAsync<CalculationMetadata>(nameof(CalculationActivities.CreateCalculationRecordActivity), batchRequestDto);
        calculationMetaData.OrchestrationProgress = "CalculationCreated";
        context.SetCustomStatus(calculationMetaData);

        // Start calculation (Databricks)
        calculationMetaData.JobId = await context.CallActivityAsync<CalculationJobId>(nameof(CalculationActivities.StartCalculationActivity), calculationMetaData.Id);
        calculationMetaData.OrchestrationProgress = "CalculationJobQueued";
        context.SetCustomStatus(calculationMetaData);

        // TODO: Adjust polling and expiry
        var pollingIntervalInSeconds = 60;
        var expiryTime = context.CurrentUtcDateTime.AddMinutes(30);

        while (context.CurrentUtcDateTime < expiryTime)
        {
            // Monitor calculation (Databricks)
            calculationMetaData.JobStatus = await context.CallActivityAsync<CalculationState>(nameof(CalculationActivities.GetJobStatusActivity), calculationMetaData.JobId);
            context.SetCustomStatus(calculationMetaData);

            if (calculationMetaData.JobStatus == CalculationState.Running
                || calculationMetaData.JobStatus == CalculationState.Pending)
            {
                // Update calculation execution status (SQL)
                await context.CallActivityAsync(nameof(CalculationActivities.UpdateCalculationExecutionStatusActivity), calculationMetaData);

                // Wait for the next checkpoint
                var nextCheckpoint = context.CurrentUtcDateTime.AddSeconds(pollingIntervalInSeconds);
                await context.CreateTimer(nextCheckpoint, CancellationToken.None);
            }
            else
            {
                break;
            }
        }

        // Update calculation execution status (SQL)
        await context.CallActivityAsync(nameof(CalculationActivities.UpdateCalculationExecutionStatusActivity), calculationMetaData);

        if (calculationMetaData.JobStatus == CalculationState.Completed)
        {
            calculationMetaData.OrchestrationProgress = "CalculationJobCompleted";
            context.SetCustomStatus(calculationMetaData);

            // OBSOLETE: Create calculation completed (SQL - Event database)
            await context.CallActivityAsync(nameof(CalculationActivities.CreateCompletedCalculationActivity), calculationMetaData.Id);

            //// TODO: Wait for warehouse to start (could use retry policy); could be done using fan-out/fan-in

            // Send calculation results (ServiceBus)
            await context.CallActivityAsync(nameof(CalculationActivities.SendCalculationResultsActivity), calculationMetaData.Id);
            calculationMetaData.OrchestrationProgress = "CalculationResultsSend";
            context.SetCustomStatus(calculationMetaData);
        }
        else
        {
            calculationMetaData.OrchestrationProgress = "CalculationJobFailed";
            context.SetCustomStatus(calculationMetaData);
            return $"Error: Job status '{calculationMetaData.JobStatus}'.";
        }

        // TODO: Could wait for an event to notiy us that messages are ready for customer in EDI
        return "Success";
    }
}
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
