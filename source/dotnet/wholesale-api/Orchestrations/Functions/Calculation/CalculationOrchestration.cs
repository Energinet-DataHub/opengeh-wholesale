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

using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
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
                or CalculationState.Pending)
            {
                // Update calculation execution status (SQL)
                await context.CallActivityAsync(
                    nameof(UpdateCalculationStatusActivity),
                    calculationMetadata);

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
            calculationMetadata.OrchestrationProgress = "CalculationResultsSend";

            context.SetCustomStatus(calculationMetadata);
        }
        else
        {
            calculationMetadata.OrchestrationProgress = "CalculationJobFailed";
            context.SetCustomStatus(calculationMetadata);
            return $"Error: Job status '{calculationMetadata.JobStatus}'.";
        }

        // TODO: Wait for an event to notify us that messages are ready for customer in EDI, and update orchestration status
        return "Success";
    }
}
