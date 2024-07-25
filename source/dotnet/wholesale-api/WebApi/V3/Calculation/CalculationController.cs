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

using Asp.Versioning;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.Wholesale.WebApi.V3.Calculation;

/// <summary>
/// Energy suppliers for which calculation results have been calculated.
/// </summary>
[Route("/v3/calculations")]
public class CalculationController : V3ControllerBase
{
    private readonly ICalculationsClient _calculationsClient;

    public CalculationController(
        ICalculationsClient calculationsClient)
    {
        _calculationsClient = calculationsClient;
    }

    /// <summary>
    /// Returns a calculation matching <paramref name="calculationId"/>.
    /// </summary>
    /// <param name="calculationId">CalculationId</param>
    [HttpGet("{calculationId}", Name = "GetCalculation")]
    [MapToApiVersion(Version)]
    [Produces("application/json", Type = typeof(CalculationDto))]
    [Authorize(Roles = Permissions.CalculationsManage)]
    public async Task<IActionResult> GetAsync([FromRoute] Guid calculationId)
    {
        return Ok(await _calculationsClient.GetAsync(calculationId).ConfigureAwait(false));
    }

    /// <summary>
    /// Get calculations that matches the criteria specified
    /// </summary>
    /// <param name="gridAreaCodes"></param>
    /// <param name="executionState"></param>
    /// <param name="minExecutionTime"></param>
    /// <param name="maxExecutionTime"></param>
    /// <param name="periodStart"></param>
    /// <param name="periodEnd"></param>
    /// <returns>Calculations that matches the search criteria. Always 200 OK</returns>
    [HttpGet(Name = "SearchCalculations")]
    [MapToApiVersion(Version)]
    [Produces("application/json", Type = typeof(List<CalculationDto>))]
    [Authorize(Roles = Permissions.CalculationsManage)]
    public async Task<IActionResult> SearchAsync(
        [FromQuery] string[]? gridAreaCodes,
        [FromQuery] CalculationState? executionState,
        [FromQuery] DateTimeOffset? minExecutionTime,
        [FromQuery] DateTimeOffset? maxExecutionTime,
        [FromQuery] DateTimeOffset? periodStart,
        [FromQuery] DateTimeOffset? periodEnd)
    {
        var calculations = await _calculationsClient.SearchAsync(
            gridAreaCodes ?? Array.Empty<string>(),
            CalculationStateMapper.MapState(executionState),
            minExecutionTime,
            maxExecutionTime,
            periodStart,
            periodEnd).ConfigureAwait(false);

        return Ok(calculations);
    }
}
