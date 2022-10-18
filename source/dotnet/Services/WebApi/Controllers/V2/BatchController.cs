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

    public BatchController(IBatchApplicationService batchApplicationService, IBatchDtoV2Mapper batchDtoV2Mapper)
    {
        _batchApplicationService = batchApplicationService;
        _batchDtoV2Mapper = batchDtoV2Mapper;
    }

    /// <summary>
    /// Create a batch.
    /// </summary>
    /// <returns>Always 200 OK</returns>
    [HttpPost]
    [MapToApiVersion(Version)]
    public async Task<IActionResult> CreateAsync([FromBody] BatchRequestDto batchRequestDto)
    {
        await _batchApplicationService.CreateAsync(batchRequestDto).ConfigureAwait(false);
        return Ok();
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
}
