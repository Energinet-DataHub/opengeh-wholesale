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
using Energinet.DataHub.Wholesale.Orchestrations.Extensions.Options;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation;

internal class CalculationTrigger
{
    [Function(nameof(StartCalculation))]
    public async Task<HttpResponseData> StartCalculation(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
        [FromBody] StartCalculationRequestDto startCalculationRequestDto,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger<CalculationOrchestration>();

        var orchestrationInput = new CalculationOrchestrationInput(
            new CalculationJobStatusMonitorOptions(),
            startCalculationRequestDto,
            // TODO: Retrieve user id from token sent as part of http request
            Guid.Parse("3A3A90B7-C624-4844-B990-3221DEE54F04"));

        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(CalculationOrchestration.Calculation), orchestrationInput).ConfigureAwait(false);
        logger.LogInformation("Created new orchestration with instance ID = {instanceId}", instanceId);

        var orchestrationMetadata = await client.WaitForInstanceStartAsync(instanceId).ConfigureAwait(false);
        while (ReadCalculationId(orchestrationMetadata) == Guid.Empty)
        {
            await Task.Delay(200).ConfigureAwait(false);
            orchestrationMetadata = await client.GetInstanceAsync(instanceId, getInputsAndOutputs: true).ConfigureAwait(false);
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(ReadCalculationId(orchestrationMetadata)).ConfigureAwait(false);

        return response;
    }

    private static Guid ReadCalculationId(OrchestrationMetadata? orchestrationMetadata)
    {
        if (orchestrationMetadata == null || orchestrationMetadata.SerializedCustomStatus == null)
            return Guid.Empty;

        var calculationMetadata = orchestrationMetadata.ReadCustomStatusAs<CalculationMetadata>();
        return calculationMetadata == null || calculationMetadata.Id == Guid.Empty
            ? Guid.Empty
            : calculationMetadata.Id;
    }
}
