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

using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Security;
using Energinet.DataHub.Wholesale.Orchestrations.Extensions.Options;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation;

internal class CalculationTrigger
{
    private readonly IUserContext<FrontendUser> _userContext;
    private readonly CalculationOrchestrationMonitorOptions _orchestrationMonitorOptions;

    public CalculationTrigger(
        IUserContext<FrontendUser> userContext,
        IOptions<CalculationOrchestrationMonitorOptions> jobStatusMonitorOptions)
    {
        _userContext = userContext;
        _orchestrationMonitorOptions = jobStatusMonitorOptions.Value;
    }

    [Function(nameof(StartCalculation))]
    public async Task<IActionResult> StartCalculation(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest httpRequest,
        [FromBody] StartCalculationRequestDto startCalculationRequestDto,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger<CalculationTrigger>();

        var orchestrationInput = new CalculationOrchestrationInput(
            _orchestrationMonitorOptions,
            startCalculationRequestDto,
            _userContext.CurrentUser.UserId);

        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(CalculationOrchestration), orchestrationInput).ConfigureAwait(false);
        logger.LogInformation("Created new orchestration with instance ID = {instanceId}", instanceId);

        var orchestrationMetadata = await client.WaitForInstanceStartAsync(instanceId).ConfigureAwait(false);
        while (ReadCalculationId(orchestrationMetadata) == Guid.Empty)
        {
            await Task.Delay(200).ConfigureAwait(false);
            orchestrationMetadata = await client.GetInstanceAsync(instanceId, getInputsAndOutputs: true).ConfigureAwait(false);
        }

        var calculationId = ReadCalculationId(orchestrationMetadata);
        return new OkObjectResult(calculationId);
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
