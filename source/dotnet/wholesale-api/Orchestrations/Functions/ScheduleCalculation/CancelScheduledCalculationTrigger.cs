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

using Energinet.DataHub.Wholesale.Calculations.Interfaces.AuditLog;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.ScheduleCalculation;

internal class CancelScheduledCalculationTrigger(
    ILogger<CancelScheduledCalculationTrigger> logger,
    IAuditLogger auditLogger,
    CalculationSchedulerHandler calculationSchedulerHandler)
{
    private const string PermissionCalculationsManage = "calculations:manage";

    private readonly ILogger<CancelScheduledCalculationTrigger> _logger = logger;
    private readonly IAuditLogger _auditLogger = auditLogger;
    private readonly CalculationSchedulerHandler _calculationSchedulerHandler = calculationSchedulerHandler;

    [Function(nameof(CancelScheduledCalculation))]
    [Authorize(Roles = PermissionCalculationsManage)]
    public async Task<IActionResult> CancelScheduledCalculation(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest httpRequest,
        [DurableClient] DurableTaskClient durableTaskClient,
        [FromBody] CancelScheduledCalculationRequestDto cancelScheduledCalculationRequestDto,
        FunctionContext executionContext)
    {
        await _auditLogger.LogWithCommitAsync(
                AuditLogActivity.CancelScheduledCalculation,
                httpRequest.GetDisplayUrl(),
                cancelScheduledCalculationRequestDto,
                AuditLogEntityType.Calculation,
                cancelScheduledCalculationRequestDto.CalculationId)
            .ConfigureAwait(false);

        await _calculationSchedulerHandler.CancelScheduledCalculationAsync(
                durableTaskClient,
                new CalculationId(cancelScheduledCalculationRequestDto.CalculationId))
            .ConfigureAwait(false);

        return new OkResult();
    }
}
