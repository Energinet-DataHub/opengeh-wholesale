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
using Energinet.DataHub.Wholesale.Application.SettlementReport;
using Energinet.DataHub.Wholesale.Application.SettlementReport.Model;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace Energinet.DataHub.Wholesale.WebApi.V2;

/// <summary>
/// Handle process batches.
/// </summary>
[ApiController]
[ApiVersion(Version)]
[Produces("application/json")]
[Route("v{version:apiVersion}/[controller]")]
public class BatchController : ControllerBase
{
    private const string Version = "2.0";
    private readonly IBatchApplicationService _batchApplicationService;
    private readonly IBatchDtoV2Mapper _batchDtoV2Mapper;
    private readonly ISettlementReportApplicationService _settlementReportApplicationService;
    private readonly DateTimeZone _dateTimeZone;
    private readonly IMediator _mediator;

    public BatchController(
        IBatchApplicationService batchApplicationService,
        IBatchDtoV2Mapper batchDtoV2Mapper,
        ISettlementReportApplicationService settlementReportApplicationService,
        DateTimeZone dateTimeZone,
        IMediator mediator)
    {
        _batchApplicationService = batchApplicationService;
        _batchDtoV2Mapper = batchDtoV2Mapper;
        _settlementReportApplicationService = settlementReportApplicationService;
        _dateTimeZone = dateTimeZone;
        _mediator = mediator;
    }

    /// <summary>
    /// Create a batch.
    /// Period end must be exactly 1 ms before midnight.
    /// </summary>
    /// <returns>200 Ok with The batch id, or a 400 with an errormessage</returns>
    [HttpPost]
    [MapToApiVersion(Version)]
    [Produces("application/json", Type = typeof(Guid))]
    public async Task<Guid> CreateAsync([FromBody] BatchRequestDto batchRequestDto)
    {
        batchRequestDto = batchRequestDto with { EndDate = batchRequestDto.EndDate.AddMilliseconds(1) };
        var periodEnd = Instant.FromDateTimeOffset(batchRequestDto.EndDate);
        if (new ZonedDateTime(periodEnd, _dateTimeZone).TimeOfDay != LocalTime.Midnight)
            throw new BusinessValidationException($"The period end '{periodEnd.ToString()}' must be 1 ms before midnight.");

        return await _mediator.Send(new CreateBatchCommand(
            batchRequestDto.ProcessType,
            batchRequestDto.GridAreaCodes,
            batchRequestDto.StartDate,
            batchRequestDto.EndDate)).ConfigureAwait(false);
    }

    /// <summary>
    /// Get batches that matches the criteria specified in <paramref name="batchSearchDto"/>
    /// Period ends are 1 ms before midnight of the last day of the period.
    /// </summary>
    /// <param name="batchSearchDto">Search criteria</param>
    /// <returns>Batches that matches the search criteria. Always 200 OK</returns>
    [HttpPost("Search")]
    [MapToApiVersion(Version)]
    [Produces("application/json", Type = typeof(List<BatchDtoV2>))]
    public async Task<IActionResult> SearchAsync([FromBody] BatchSearchDto batchSearchDto)
    {
        var batchesDto = await _batchApplicationService.SearchAsync(
            Enumerable.Empty<string>(),
            null,
            batchSearchDto.MinExecutionTime,
            batchSearchDto.MaxExecutionTime,
            null,
            null).ConfigureAwait(false);

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
    [Produces("application/json", Type = typeof(Stream))]
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
    [Produces("application/json", Type = typeof(BatchDtoV2))]
    public async Task<IActionResult> GetAsync(Guid batchId)
    {
        var batchDto = await _batchApplicationService.GetAsync(batchId).ConfigureAwait(false);
        var batch = _batchDtoV2Mapper.Map(batchDto);

        // Subtract 1 ms from period end as it is currently the expectation of the API
        batch = batch with { PeriodEnd = batch.PeriodEnd.AddMilliseconds(-1) };

        return Ok(batch);
    }
}
