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

using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Application.ProcessStep;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.ProcessStep;
using Energinet.DataHub.Wholesale.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.Wholesale.WebApi.V3.ProcessStepResult;

/// <summary>
/// Calculated result.
/// </summary>
[Route("/v3/batches/{batchId}/processes/{gridAreaCode}/time-series-types/{timeSeriesType}")]
public class ProcessStepResultController : V3ControllerBase
{
    private readonly IProcessStepApplicationService _processStepApplicationService;
    private readonly IProcessStepResultFactory _processStepResultFactory;
    private readonly IBatchApplicationService _batchApplicationService;

    public ProcessStepResultController(
        IProcessStepApplicationService processStepApplicationService,
        IProcessStepResultFactory processStepResultFactory,
        IBatchApplicationService batchApplicationService)
    {
        _processStepApplicationService = processStepApplicationService;
        _processStepResultFactory = processStepResultFactory;
        _batchApplicationService = batchApplicationService;
    }

    /// <summary>
    /// Calculation results provided by the following method:
    /// When only 'energySupplierGln' is provided, a result is returned for a energy supplier for the requested grid area, for the specified time series type.
    /// if only a 'balanceResponsiblePartyGln' is provided, a result is returned for a balance responsible party for the requested grid area, for the specified time series type.
    /// if both 'balanceResponsiblePartyGln' and 'energySupplierGln' is provided, a result is returned for the balance responsible party's energy supplier for requested grid area, for the specified time series type.
    /// if no 'balanceResponsiblePartyGln' and 'energySupplierGln' is provided, a result is returned for the requested grid area, for the specified time series type.
    /// </summary>
    /// <param name="batchId">The id to identify the batch the request is for</param>
    /// <param name="gridAreaCode">The grid area the requested result is in</param>
    /// <param name="timeSeriesType">The time series type the result has</param>
    /// <param name="energySupplierGln">The GLN for the energy supplier the requested result</param>
    /// <param name="balanceResponsiblePartyGln">The GLN for the balance responsible party the requested result</param>
    [HttpGet(Name = "GetProcessStepResult")]
    [Produces("application/json", Type = typeof(ProcessStepResultDto))]
    public async Task<ProcessStepResultDto> GetResultAsync(
        [FromRoute] Guid batchId,
        [FromRoute] string gridAreaCode,
        [FromRoute] TimeSeriesType timeSeriesType,
        [FromQuery] string? energySupplierGln,
        [FromQuery] string? balanceResponsiblePartyGln)
    {
        var stepResult = await _processStepApplicationService.GetResultAsync(
            batchId,
            gridAreaCode,
            timeSeriesType,
            energySupplierGln,
            balanceResponsiblePartyGln).ConfigureAwait(false);

        var batch = await _batchApplicationService.GetAsync(batchId).ConfigureAwait(false);

        return _processStepResultFactory.Create(stepResult, batch);
    }
}
