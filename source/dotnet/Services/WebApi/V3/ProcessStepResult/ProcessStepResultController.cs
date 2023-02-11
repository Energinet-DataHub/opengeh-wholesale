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
using Energinet.DataHub.Wholesale.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.Wholesale.WebApi.V3.ProcessStepResult;

/// <summary>
/// Actor resource for actors for whom batch results have been calculated.
/// </summary>
[ApiController]
[Route("/v3/batches/{batchId}/processes/{gridAreaCode}/time-series-types/{timeSeriesType}/actors/{gln}")]
public class ProcessStepResultController : ControllerBase
{
    private readonly IProcessStepApplicationService _processStepApplicationService;
    private readonly IProcessStepResultFactory _processStepResultFactory;
    private readonly IBatchApplicationService _batchApplicationService;

    public ProcessStepResultController(IProcessStepApplicationService processStepApplicationService, IProcessStepResultFactory processStepResultFactory, IBatchApplicationService batchApplicationService)
    {
        _processStepApplicationService = processStepApplicationService;
        _processStepResultFactory = processStepResultFactory;
        _batchApplicationService = batchApplicationService;
    }

    [AllowAnonymous] // TODO: Temporary hack to enable EDI integration while awaiting architects decision
    [HttpGet]
    public async Task<ProcessStepResultDto> GetAsync([FromRoute] Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType, string gln)
    {
        var request = new ProcessStepResultRequestDtoV2(batchId, gridAreaCode, timeSeriesType, gln);
        var stepResult = await _processStepApplicationService.GetResultAsync(request).ConfigureAwait(false);
        var batch = await _batchApplicationService.GetAsync(batchId).ConfigureAwait(false);

        return _processStepResultFactory.Create(stepResult, batch);
    }
}
