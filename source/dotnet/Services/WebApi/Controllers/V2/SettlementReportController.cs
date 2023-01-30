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

using Energinet.DataHub.Wholesale.Application.SettlementReport;
using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.Wholesale.WebApi.Controllers.V2;

[ApiController]
[ApiVersion(Version)]
[Route("v{version:apiVersion}/[controller]")]
public class SettlementReportController : ControllerBase
{
    private const string Version = "2.3";
    private readonly ISettlementReportApplicationService _settlementReportApplicationService;

    public SettlementReportController(ISettlementReportApplicationService settlementReportApplicationService)
    {
        _settlementReportApplicationService = settlementReportApplicationService;
    }

    /// <summary>
    /// Returns a stream containing the settlement report for batch with <paramref name="batchId"/>.
    /// </summary>
    /// <param name="batchId">BatchId</param>
    [HttpGet]
    [MapToApiVersion(Version)]
    public async Task<IActionResult> GetAsync(Guid batchId)
    {
        var report = await _settlementReportApplicationService.GetSettlementReportAsync(batchId).ConfigureAwait(false);
        return Ok(report.Stream);
    }
}
