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
using Energinet.DataHub.Wholesale.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.Wholesale.WebApi.V2;

/// <summary>
/// Handle process batches.
/// </summary>
[ApiController]
[ApiVersion(Version)]
[Route("v{version:apiVersion}/Batch")]
public class BatchControllerV21 : ControllerBase
{
    private const string Version = "2.1";
    private readonly IBatchApplicationService _batchApplicationService;
    private readonly IBatchDtoV2Mapper _batchDtoV2Mapper;

    public BatchControllerV21(
        IBatchApplicationService batchApplicationService,
        IBatchDtoV2Mapper batchDtoV2Mapper)
    {
        _batchApplicationService = batchApplicationService;
        _batchDtoV2Mapper = batchDtoV2Mapper;
    }

    /// <summary>
    /// Get batches that matches the criteria specified in <paramref name="batchSearchDto"/>
    /// Period ends are 1 ms before midnight of the last day of the period.
    /// </summary>
    /// <param name="batchSearchDto">Search criteria</param>
    /// <returns>Batches that matches the search criteria. Always 200 OK</returns>
    [HttpPost("Search")]
    [MapToApiVersion(Version)]
    [AllowAnonymous]
    [Produces("application/json", Type = typeof(List<BatchDtoV2>))]
    public async Task<IActionResult> SearchAsync([FromBody] BatchSearchDtoV2 batchSearchDto)
    {
        var batchesDto = await _batchApplicationService.SearchAsync(
            batchSearchDto.FilterByGridAreaCodes,
            batchSearchDto.FilterByExecutionState,
            batchSearchDto.MinExecutionTime,
            batchSearchDto.MaxExecutionTime,
            batchSearchDto.PeriodStart,
            batchSearchDto.PeriodEnd).ConfigureAwait(false);

        var batches = batchesDto.Select(_batchDtoV2Mapper.Map).ToList();

        // Subtract 1 ms from period end as it is currently the expectation of the API
        for (var i = 0; i < batches.Count(); i++)
        {
            var batch = batches[i];
            batches[i] = batch with { PeriodEnd = batch.PeriodEnd.AddMilliseconds(-1) };
        }

        return Ok(batches);
    }
}
