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

using System.Net;
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Application.Batches.Model;
using Energinet.DataHub.Wholesale.Application.SettlementReport;
using Energinet.DataHub.Wholesale.Contracts;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

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
    private readonly DateTimeZone _dateTimeZone;

    public BatchController(
        IBatchApplicationService batchApplicationService,
        IBatchDtoV2Mapper batchDtoV2Mapper,
        ISettlementReportApplicationService settlementReportApplicationService,
        IBatchRequestDtoValidator batchRequestDtoValidator,
        DateTimeZone dateTimeZone)
    {
        _batchApplicationService = batchApplicationService;
        _batchDtoV2Mapper = batchDtoV2Mapper;
        _settlementReportApplicationService = settlementReportApplicationService;
        _batchRequestDtoValidator = batchRequestDtoValidator;
        _dateTimeZone = dateTimeZone;
    }

    /// <summary>
    /// Create a batch.
    /// Period end must be exactly 1 ms before midnight.
    /// </summary>
    /// <returns>200 Ok with The batch id, or a 400 with an errormessage</returns>
    [HttpPost]
    [MapToApiVersion(Version)]
    public async Task<IActionResult> CreateAsync([FromBody] BatchRequestDto batchRequestDto)
    {
        batchRequestDto = batchRequestDto with { EndDate = batchRequestDto.EndDate.AddMilliseconds(1) };
        var periodEnd = Instant.FromDateTimeOffset(batchRequestDto.EndDate);
        if (new ZonedDateTime(periodEnd, _dateTimeZone).TimeOfDay != LocalTime.Midnight)
            return StatusCode((int)HttpStatusCode.BadRequest, $"The period end '{periodEnd.ToString()}' must be 1 ms before midnight.");

        if (!_batchRequestDtoValidator.IsValid(batchRequestDto, out var errorMessages))
            return StatusCode((int)HttpStatusCode.BadRequest, string.Join(" ", errorMessages));

        var batchId = await _batchApplicationService.CreateAsync(batchRequestDto).ConfigureAwait(false);
        return Ok(batchId);
    }

    /// <summary>
    /// Get batches that matches the criteria specified in <paramref name="batchSearchDto"/>
    /// Period ends are 1 ms before midnight of the last day of the period.
    /// </summary>
    /// <param name="batchSearchDto">Search criteria</param>
    /// <returns>Batches that matches the search criteria. Always 200 OK</returns>
    [HttpPost("Search")]
    [MapToApiVersion(Version)]
    public async Task<IActionResult> SearchAsync([FromBody] BatchSearchDto batchSearchDto)
    {
        var batchesDto = await _batchApplicationService.SearchAsync(batchSearchDto).ConfigureAwait(false);
        var batches = batchesDto.Select(_batchDtoV2Mapper.Map).ToList();

        // Subtract 1 ms from period end as it is currently the expectation of the API
        for (var i = 0; i < batches.Count(); i++)
        {
            var batch = batches[i];
            batches[i] = batch with { PeriodEnd = batch.PeriodEnd.AddMilliseconds(-1) };
        }

        return Ok(batches);
    }

    /// <summary>
    /// Returns a stream containing the settlement report for a batch matching <paramref name="batchId"/>
    /// </summary>
    /// <param name="batchId">BatchId</param>
    [Obsolete("Use HTTP GET /v2_3/settlementreport")]
    [HttpPost("ZippedBasisDataStream")]
    [MapToApiVersion(Version)]
    public async Task<IActionResult> GetSettlementReportAsync([FromBody] Guid batchId)
    {
        var report = await _settlementReportApplicationService.GetSettlementReportAsync(batchId).ConfigureAwait(false);
        return Ok(report.Stream);
    }

    /// <summary>
    /// Returns a batch matching <paramref name="batchId"/>.
    /// Period ends are 1 ms before midnight of the last day of the period.
    /// </summary>
    /// <param name="batchId">BatchId</param>
    [HttpGet]
    [MapToApiVersion(Version)]
    public async Task<IActionResult> GetAsync(Guid batchId)
    {
        var batchDto = await _batchApplicationService.GetAsync(batchId).ConfigureAwait(false);
        var batch = _batchDtoV2Mapper.Map(batchDto);

        // Subtract 1 ms from period end as it is currently the expectation of the API
        batch = batch with { PeriodEnd = batch.PeriodEnd.AddMilliseconds(-1) };

        return Ok(batch);
    }
}
