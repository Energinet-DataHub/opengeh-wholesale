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
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Application.SettlementReport;
using Energinet.DataHub.Wholesale.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.Wholesale.WebApi.Controllers.V2;

/// <summary>
/// Handle process batches.
/// </summary>
[ApiController]
[ApiVersion(Version)]
[Route("v{version:apiVersion}/[controller]")]
public class BatchController : ControllerBase
{
    private const string Version = "2.0";
    private readonly IBatchApplicationService _batchApplicationService;
    private readonly IBatchDtoV2Mapper _batchDtoV2Mapper;
    private readonly ISettlementReportApplicationService _settlementReportApplicationService;
    private readonly IBatchRequestDtoValidator _batchRequestDtoValidator;

    public BatchController(
        IBatchApplicationService batchApplicationService,
        IBatchDtoV2Mapper batchDtoV2Mapper,
        ISettlementReportApplicationService settlementReportApplicationService,
        IBatchRequestDtoValidator batchRequestDtoValidator)
    {
        _batchApplicationService = batchApplicationService;
        _batchDtoV2Mapper = batchDtoV2Mapper;
        _settlementReportApplicationService = settlementReportApplicationService;
        _batchRequestDtoValidator = batchRequestDtoValidator;
    }

    /// <summary>
    /// Create a batch.
    /// </summary>
    /// <returns>200 Ok with The batch id, or a 400 with an errormessage</returns>
    [HttpPost]
    [MapToApiVersion(Version)]
    public async Task<IActionResult> CreateAsync([FromBody] BatchRequestDto batchRequestDto)
    {
        if (!_batchRequestDtoValidator.IsValid(batchRequestDto, out var errorMessages))
            return StatusCode(400, string.Join(" ", errorMessages));

        var batchId = await _batchApplicationService.CreateAsync(batchRequestDto).ConfigureAwait(false);
        return Ok(batchId);
    }

    /// <summary>
    /// Get batches that matches the criteria specified in <paramref name="batchSearchDto"/>
    /// </summary>
    /// <param name="batchSearchDto">Search criteria</param>
    /// <returns>Batches that matches the search criteria. Always 200 OK</returns>
    [HttpPost("Search")]
    [MapToApiVersion(Version)]
    public async Task<IActionResult> SearchAsync([FromBody] BatchSearchDto batchSearchDto)
    {
        var batchesDto = await _batchApplicationService.SearchAsync(batchSearchDto).ConfigureAwait(false);
        var batchesDtoV2 = batchesDto.Select(_batchDtoV2Mapper.Map);
        return Ok(batchesDtoV2);
    }

    /// <summary>
    /// Returns a stream containing the settlement report for a batch matching <paramref name="batchId"/>
    /// </summary>
    /// <param name="batchId">BatchId</param>
    [HttpPost("ZippedBasisDataStream")]
    [MapToApiVersion(Version)]
    public async Task<IActionResult> GetSettlementReportAsync([FromBody] Guid batchId)
    {
        var stream = await _settlementReportApplicationService.GetSettlementReportAsync(batchId).ConfigureAwait(false);
        return Ok(stream);
    }

    /// <summary>
    /// Returns a batch matching <paramref name="batchId"/>
    /// </summary>
    /// <param name="batchId">BatchId</param>
    [HttpGet]
    [MapToApiVersion(Version)]
    public async Task<IActionResult> GetAsync(Guid batchId)
    {
        var batchDto = await _batchApplicationService.GetAsync(batchId).ConfigureAwait(false);
        var batchDtoV2 = _batchDtoV2Mapper.Map(batchDto);
        return Ok(batchDtoV2);
    }

    /// <summary>
    /// Returns a list of actors
    /// </summary>
    /// <param name="batchActorsRequestDto">BatchActorsRequestDto</param>
    [HttpPost("Actors")]
    [MapToApiVersion(Version)]
    public async Task<IActionResult> GetActorsAsync([FromBody] BatchActorsRequestDto batchActorsRequestDto)
    {
        await Task.Delay(1000).ConfigureAwait(true);
        // var batchDto = await _batchApplicationService.GetAsync(batchId).ConfigureAwait(false);
        // var batchDtoV2 = _batchDtoV2Mapper.Map(batchDto);
        var actorList = new BatchActorDto[] { new BatchActorDto("1234"), new BatchActorDto("5678") };
        return Ok(actorList);
    }
}
