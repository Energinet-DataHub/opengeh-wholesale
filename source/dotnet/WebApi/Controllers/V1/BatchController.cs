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

using Energinet.DataHub.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.Wholesale.WebApi.Controllers.V1;

/// <summary>
/// Handle process batches.
/// </summary>
[ApiController]
[ApiVersion(Version)]
[Route("v{version:apiVersion}/[controller]")]
public class BatchController : ControllerBase
{
    private readonly IBatchApplicationService _batchApplicationService;

    public BatchController(IBatchApplicationService batchApplicationService)
    {
        _batchApplicationService = batchApplicationService;
    }

    private const string Version = "1.0";

    /// <summary>
    /// Create a batch.
    /// </summary>
    /// <returns>Always 200 OK</returns>
    [HttpPost]
    [MapToApiVersion(Version)]
    public async Task<IActionResult> CreateAsync(
        WholesaleProcessType processType,
        [FromQuery]List<Guid> gridAreas)
    {
        await _batchApplicationService.CreateAsync(processType, gridAreas.Select(id => new GridAreaId(id)));
        return Ok();
    }
}
