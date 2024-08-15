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

using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Security;
using Energinet.DataHub.Wholesale.Orchestrations.Extensions.Options;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation;

internal class CalculationTrigger
{
    private const string PermissionCalculationsManage = "calculations:manage";

    private readonly IUserContext<FrontendUser> _userContext;
    private readonly ICalculationsClient _calculationsClient;
    private readonly ILogger<CalculationTrigger> _logger;
    private readonly CalculationOrchestrationMonitorOptions _orchestrationMonitorOptions;

    public CalculationTrigger(
        IUserContext<FrontendUser> userContext,
        ICalculationsClient calculationsClient,
        ILogger<CalculationTrigger> logger,
        IOptions<CalculationOrchestrationMonitorOptions> jobStatusMonitorOptions)
    {
        _userContext = userContext;
        _calculationsClient = calculationsClient;
        _logger = logger;
        _orchestrationMonitorOptions = jobStatusMonitorOptions.Value;
    }

    [Function(nameof(StartCalculation))]
    [Authorize(Roles = PermissionCalculationsManage)]
    public async Task<IActionResult> StartCalculation(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest httpRequest,
        [FromBody] StartCalculationRequestDto startCalculationRequestDto,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger<CalculationTrigger>();
        var calculationsClient = executionContext.InstanceServices.GetRequiredService<ICalculationsClient>();

        var calculationId = await calculationsClient.CreateAndCommitAsync(
            startCalculationRequestDto.CalculationType,
            startCalculationRequestDto.GridAreaCodes,
            startCalculationRequestDto.StartDate,
            startCalculationRequestDto.EndDate,
            startCalculationRequestDto.ScheduledAt,
            _userContext.CurrentUser.UserId).ConfigureAwait(false);

        logger.LogInformation("Calculation created with id {calculationId}", calculationId);

        var orchestrationInput = new CalculationOrchestrationInput(
            _orchestrationMonitorOptions,
            calculationId);

        // TODO: Move starting orchestration to a new TimeTrigger in a new PR.
        // Since this will happen soon, we don't handle the case where the orchestration fails to start, but
        // the calculation is already added to the database.
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(CalculationOrchestration), orchestrationInput).ConfigureAwait(false);
        logger.LogInformation("Started new orchestration for calculation ID = {calculationId} with instance ID = {instanceId}", calculationId, instanceId);

        var orchestrationMetadata = await client.WaitForInstanceStartAsync(instanceId).ConfigureAwait(false);
        while (ReadCalculationId(orchestrationMetadata) == Guid.Empty)
        {
            await Task.Delay(200).ConfigureAwait(false);
            orchestrationMetadata = await client.GetInstanceAsync(instanceId, getInputsAndOutputs: true).ConfigureAwait(false);
        }

        return new OkObjectResult(calculationId.Id);
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
