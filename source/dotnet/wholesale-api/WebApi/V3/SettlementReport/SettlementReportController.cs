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

using Asp.Versioning;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Common.Interfaces.Security;
using Energinet.DataHub.Wholesale.WebApi.V3.Calculation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime.Extensions;
using CalculationState = Energinet.DataHub.Wholesale.Calculations.Interfaces.Models.CalculationState;

namespace Energinet.DataHub.Wholesale.WebApi.V3.SettlementReport;

[ApiController]
[Route("v3/[controller]")]
public class SettlementReportController : V3ControllerBase
{
    private readonly ICalculationsClient _calculationsClient;
    private readonly IGridAreaOwnershipClient _gridAreaOwnershipClient;
    private readonly IUserContext<FrontendUser> _userContext;

    public SettlementReportController(
        ICalculationsClient calculationsClient,
        IGridAreaOwnershipClient gridAreaOwnershipClient,
        IUserContext<FrontendUser> userContext)
    {
        _calculationsClient = calculationsClient;
        _gridAreaOwnershipClient = gridAreaOwnershipClient;
        _userContext = userContext;
    }

    /// <summary>
    /// Returns a subset of calculations that are valid for use with settlement reports.
    /// Settlement reports must access only a subset of data about calculations, as settlement reports are used by actors.
    /// </summary>
    [HttpGet(Name = "GetApplicableCalculations")]
    [MapToApiVersion(Version)]
    [Produces("application/json", Type = typeof(List<SettlementReportApplicableCalculationDto>))]
    [Authorize(Roles = Permissions.SettlementReportsManage)]
    public async Task<IActionResult> GetApplicableCalculationsAsync(
        [FromQuery] CalculationType calculationType,
        [FromQuery] string[] gridAreaCodes,
        [FromQuery] DateTimeOffset periodStart,
        [FromQuery] DateTimeOffset periodEnd)
    {
        if (_userContext.CurrentUser.Actor.HasMarketRole(FrontendActorMarketRole.GridAccessProvider))
        {
            var ownedGridAreas = await _gridAreaOwnershipClient
                .GetOwnedByAsync(_userContext.CurrentUser.Actor.ActorNumber)
                .ConfigureAwait(false);

            if (gridAreaCodes.Any(code => !ownedGridAreas.Contains(code)))
            {
                return Forbid();
            }
        }

        var calculations = await _calculationsClient
            .SearchAsync(
                gridAreaCodes,
                CalculationState.Completed,
                periodStart.ToInstant(),
                periodEnd.ToInstant(),
                CalculationTypeMapper.Map(calculationType))
            .ConfigureAwait(false);

        var calculationsForSettlementReports =
            from calculation in calculations
            from gridAreaCode in calculation.GridAreaCodes
            where gridAreaCodes.Contains(gridAreaCode)
            select new SettlementReportApplicableCalculationDto(
                calculation.CalculationId,
                calculation.ExecutionTimeStart!.Value,
                calculation.PeriodStart,
                calculation.PeriodEnd,
                gridAreaCode);

        return Ok(calculationsForSettlementReports.ToList());
    }
}
