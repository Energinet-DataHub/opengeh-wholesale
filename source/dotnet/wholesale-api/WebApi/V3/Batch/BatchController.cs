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

using System.ComponentModel.DataAnnotations;
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace Energinet.DataHub.Wholesale.WebApi.V3.Batch;

/// <summary>
/// Energy suppliers for which batch results have been calculated.
/// </summary>
[Route("/v3/batch")]
public class BatchController : V3ControllerBase
{
    private readonly DateTimeZone _dateTimeZone;
    private readonly IBatchApplicationService _batchApplicationService;

    public BatchController(DateTimeZone dateTimeZone, IBatchApplicationService batchApplicationService)
    {
        _dateTimeZone = dateTimeZone;
        _batchApplicationService = batchApplicationService;
    }

    /// <summary>
    /// Create a batch.
    /// Period end must be exactly 1 ms before midnight.
    /// </summary>
    /// <returns>200 Ok with The batch id, or a 400 with an errormessage</returns>
    [HttpPost(Name = "CreateBatch")]
    [MapToApiVersion(Version)]
    [Produces("application/json", Type = typeof(Guid))]
    public async Task<Guid> CreateAsync([FromBody][Required] BatchRequestDto batchRequestDto)
    {
        batchRequestDto = batchRequestDto with { EndDate = batchRequestDto.EndDate.AddMilliseconds(1) };
        var periodEnd = Instant.FromDateTimeOffset(batchRequestDto.EndDate);
        if (new ZonedDateTime(periodEnd, _dateTimeZone).TimeOfDay != LocalTime.Midnight)
            throw new BusinessValidationException($"The period end '{periodEnd.ToString()}' must be 1 ms before midnight.");

        var batchId = await _batchApplicationService.CreateAsync(batchRequestDto).ConfigureAwait(false);
        return batchId;
    }
}
