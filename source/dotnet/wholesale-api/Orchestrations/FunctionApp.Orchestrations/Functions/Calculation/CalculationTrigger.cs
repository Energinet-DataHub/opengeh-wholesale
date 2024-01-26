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
using FunctionApp.Orchestrations.Functions.Calculation.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace FunctionApp.Orchestrations.Functions.Calculation
{
    internal class CalculationTrigger
    {
        [Function(nameof(StartCalculation))]
        public async Task<HttpResponseData> StartCalculation(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
            [FromBody] BatchRequestDto batchRequestDto,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger<CalculationOrchestration>();

            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(CalculationOrchestration.Calculation), batchRequestDto).ConfigureAwait(false);
            logger.LogInformation("Created new orchestration with instance ID = {instanceId}", instanceId);

            var orchestrationMetaData = await client.WaitForInstanceStartAsync(instanceId).ConfigureAwait(false);
            while (ReadCalculationId(orchestrationMetaData) == Guid.Empty)
            {
                await Task.Delay(200).ConfigureAwait(false);
                orchestrationMetaData = await client.GetInstanceAsync(instanceId, getInputsAndOutputs: true).ConfigureAwait(false);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(ReadCalculationId(orchestrationMetaData)).ConfigureAwait(false);

            return response;
        }

        // TODO: For demo purposes, can be deleted later.
        [Function(nameof(StartCalculationForDemo))]
        public async Task<HttpResponseData> StartCalculationForDemo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            [FromBody] BatchRequestDto batchRequestDto,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger<CalculationOrchestration>();

            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(Calculation), batchRequestDto).ConfigureAwait(false);
            logger.LogInformation("Created new orchestration with instance ID = {instanceId}", instanceId);

            return client.CreateCheckStatusResponse(req, instanceId);
        }

        private static Guid ReadCalculationId(OrchestrationMetadata? orchestrationMetaData)
        {
            if (orchestrationMetaData == null || orchestrationMetaData.SerializedCustomStatus == null)
                return Guid.Empty;

            var calculationMetaData = orchestrationMetaData.ReadCustomStatusAs<CalculationMetaData>();
            return calculationMetaData == null || calculationMetaData.Id == Guid.Empty
                ? Guid.Empty
                : calculationMetaData.Id;
        }
    }
}
