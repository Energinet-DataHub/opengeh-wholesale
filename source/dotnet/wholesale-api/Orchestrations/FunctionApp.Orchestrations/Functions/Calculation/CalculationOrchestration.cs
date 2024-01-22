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
    internal static class CalculationOrchestration
    {
        [Function(nameof(StartCalculation))]
        public static async Task<HttpResponseData> StartCalculation(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            [FromBody] BatchRequestDto batchRequestDto,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(nameof(StartCalculation));

            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(Calculation), batchRequestDto);
            logger.LogInformation("Created new orchestration with instance ID = {instanceId}", instanceId);

            return client.CreateCheckStatusResponse(req, instanceId);
        }

        [Function(nameof(Calculation))]
        public static async Task<string> Calculation(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var batchRequestDto = context.GetInput<BatchRequestDto>();
            if (batchRequestDto == null)
            {
                return "Error";
            }

            var result = string.Empty;

            result += await context.CallActivityAsync<string>(nameof(CalculationActivities.CreateCalculationMetaActivity), batchRequestDto.ProcessType.ToString()) + " ";
            result += await context.CallActivityAsync<string>(nameof(CalculationActivities.StartCalculationActivity), batchRequestDto.StartDate.ToString()) + " ";
            result += await context.CallActivityAsync<string>(nameof(CalculationActivities.UpdateCalculationMetaActivity), batchRequestDto.EndDate.ToString());
            return result;
        }
    }
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
}
