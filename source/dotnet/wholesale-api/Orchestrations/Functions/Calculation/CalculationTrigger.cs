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
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.AuditLog;
using Energinet.DataHub.Wholesale.Common.Interfaces.Security;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation;

internal class CalculationTrigger
{
    private const string PermissionCalculationsManage = "calculations:manage";

    private readonly IUserContext<FrontendUser> _userContext;
    private readonly ICalculationsClient _calculationsClient;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<CalculationTrigger> _logger;

    public CalculationTrigger(
        IUserContext<FrontendUser> userContext,
        ICalculationsClient calculationsClient,
        IAuditLogger auditLogger,
        ILogger<CalculationTrigger> logger)
    {
        _userContext = userContext;
        _calculationsClient = calculationsClient;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    [Function(nameof(StartCalculation))]
    [Authorize(Roles = PermissionCalculationsManage)]
    public async Task<IActionResult> StartCalculation(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest httpRequest,
        [FromBody] StartCalculationRequestDto startCalculationRequestDto,
        FunctionContext executionContext)
    {
        await _auditLogger.LogWithCommitAsync(
                AuditLogActivity.StartNewCalculation,
                httpRequest.GetDisplayUrl(),
                startCalculationRequestDto,
                AuditLogEntityType.Calculation,
                null)
            .ConfigureAwait(false);

        var calculationId = await _calculationsClient.CreateAndCommitAsync(
            startCalculationRequestDto.CalculationType,
            startCalculationRequestDto.GridAreaCodes,
            startCalculationRequestDto.StartDate,
            startCalculationRequestDto.EndDate,
            startCalculationRequestDto.ScheduledAt,
            _userContext.CurrentUser.UserId,
            startCalculationRequestDto.IsInternalCalculation).ConfigureAwait(false);

        _logger.LogInformation("Calculation created with id {calculationId}", calculationId);

        return new OkObjectResult(calculationId.Id);
    }
}
