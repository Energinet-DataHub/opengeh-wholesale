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
using Energinet.DataHub.Wholesale.Application.SettlementReport;
using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.Wholesale.WebApi.V3.SettlementReport;

[ApiController]
[Route("v3/[controller]")]
public class SettlementReportController : V3ControllerBase
{
    private readonly ISettlementReportApplicationService _settlementReportApplicationService;

    public SettlementReportController(ISettlementReportApplicationService settlementReportApplicationService)
    {
        _settlementReportApplicationService = settlementReportApplicationService;
    }

    /// <summary>
    /// Returns a stream containing the settlement report for batch with <paramref name="batchId" /> and <paramref name="gridAreaCode" />.
    /// </summary>
    /// <param name="batchId">BatchId</param>
    /// <param name="gridAreaCode">GridAreaCode</param>
    [HttpGet(Name = "GetSettlementReportAsStreamAsync")]
    [MapToApiVersion(Version)]
    [BinaryContent]
    public async Task GetAsync([Required]Guid batchId, [Required]string gridAreaCode)
    {
        var outputStream = Response.BodyWriter.AsStream();

        await using (outputStream.ConfigureAwait(false))
        {
            await _settlementReportApplicationService
                .GetSettlementReportAsync(batchId, gridAreaCode, outputStream)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Returns a stream containing the settlement report for a batch matching <paramref name="batchId"/>
    /// </summary>
    /// <param name="batchId">BatchId</param>
    [HttpPost("ZippedBasisDataStream")]
    [MapToApiVersion(Version)]
    [BinaryContent]
    public async Task<IActionResult> GetSettlementReportAsync([Required] Guid batchId)
    {
        var report = await _settlementReportApplicationService.GetSettlementReportAsync(batchId).ConfigureAwait(false);
        return Ok(report.Stream);
    }
}
