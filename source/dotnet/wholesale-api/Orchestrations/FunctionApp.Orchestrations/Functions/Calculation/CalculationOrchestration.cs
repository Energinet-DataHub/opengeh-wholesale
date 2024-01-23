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

using FunctionApp.Orchestrations.Functions.Calculation.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace FunctionApp.Orchestrations.Functions.Calculation
{
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
    internal class CalculationOrchestration
    {
        public CalculationOrchestration()
        {
        }

        [Function(nameof(StartCalculation))]
        public async Task<HttpResponseData> StartCalculation(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            [FromBody] BatchRequestDto batchRequestDto,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger<CalculationOrchestration>();

            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(Calculation), batchRequestDto);
            logger.LogInformation("Created new orchestration with instance ID = {instanceId}", instanceId);

            return client.CreateCheckStatusResponse(req, instanceId);
        }

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

            var calculationMeta = await context.CallActivityAsync<CalculationMeta>(nameof(CalculationActivities.CreateCalculationMetaActivity), batchRequestDto);
            calculationMeta.JobId = await context.CallActivityAsync<Guid>(nameof(CalculationActivities.StartCalculationActivity), calculationMeta.Id);

            // TODO: Adjust polling and expiry
            var pollingIntervalInSeconds = 5;
            var expiryTime = context.CurrentUtcDateTime.AddMinutes(1);

            while (context.CurrentUtcDateTime < expiryTime)
            {
                calculationMeta.JobStatus = await context.CallActivityAsync<string>(nameof(CalculationActivities.GetJobStatusActivity), calculationMeta.JobId);
                if (calculationMeta.JobStatus == "Completed")
                {
                    await context.CallActivityAsync(nameof(CalculationActivities.SendCalculationResultsActivity), calculationMeta.Id);
                    break;
                }

                // Wait for the next checkpoint
                var nextCheckpoint = context.CurrentUtcDateTime.AddSeconds(pollingIntervalInSeconds);
                await context.CreateTimer(nextCheckpoint, CancellationToken.None);
            }

            if (calculationMeta.JobStatus != "Completed")
            {
                return "Error: Job monitor expired.";
            }

            await context.CallActivityAsync(nameof(CalculationActivities.UpdateCalculationMetaActivity), calculationMeta);

            return "Success";
        }
    }
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
}
