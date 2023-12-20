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
using Asp.Versioning;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.Wholesale.WebApi.V3.Batch;

/// <summary>
/// Energy suppliers for which batch results have been calculated.
/// </summary>
[Route("/v3/batches")]
public class BatchController : V3ControllerBase
{
    private readonly IBatchesClient _batchesClient;
    private readonly ICreateBatchHandler _createBatchHandler;
    private readonly IUserContext<FrontendUser> _userContext;

    public BatchController(
        IBatchesClient batchesClient,
        ICreateBatchHandler createBatchHandler,
        IUserContext<FrontendUser> userContext)
    {
        _batchesClient = batchesClient;
        _createBatchHandler = createBatchHandler;
        _userContext = userContext;
    }

    /// <summary>
    /// Create a batch.
    /// </summary>
    /// <returns>200 Ok with The batch id, or a 400 with an errormessage</returns>
    [HttpPost(Name = "CreateBatch")]
    [MapToApiVersion(Version)]
    [Produces("application/json", Type = typeof(Guid))]
    [Authorize(Roles = Permissions.CalculationsManage)]
    public async Task<Guid> CreateAsync([FromBody][Required] BatchRequestDto batchRequestDto)
    {
        return await _createBatchHandler.HandleAsync(new CreateBatchCommand(
            ProcessTypeMapper.Map(batchRequestDto.ProcessType),
            batchRequestDto.GridAreaCodes,
            batchRequestDto.StartDate,
            batchRequestDto.EndDate,
            _userContext.CurrentUser.UserId)).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns a batch matching <paramref name="batchId"/>.
    /// </summary>
    /// <param name="batchId">BatchId</param>
    [HttpGet("{batchId}", Name = "GetBatch")]
    [MapToApiVersion(Version)]
    [Produces("application/json", Type = typeof(BatchDto))]
    [Authorize(Roles = Permissions.CalculationsManage)]
    public async Task<IActionResult> GetAsync([FromRoute] Guid batchId)
    {
        return Ok(await _batchesClient.GetAsync(batchId).ConfigureAwait(false));
    }

    /// <summary>
    /// Get batches that matches the criteria specified
    /// </summary>
    /// <param name="gridAreaCodes"></param>
    /// <param name="executionState"></param>
    /// <param name="minExecutionTime"></param>
    /// <param name="maxExecutionTime"></param>
    /// <param name="periodStart"></param>
    /// <param name="periodEnd"></param>
    /// <returns>Batches that matches the search criteria. Always 200 OK</returns>
    [HttpGet(Name = "SearchBatches")]
    [MapToApiVersion(Version)]
    [Produces("application/json", Type = typeof(List<BatchDto>))]
    [Authorize(Roles = Permissions.CalculationsManage)]
    public async Task<IActionResult> SearchAsync(
        [FromQuery] string[]? gridAreaCodes,
        [FromQuery] BatchState? executionState,
        [FromQuery] DateTimeOffset? minExecutionTime,
        [FromQuery] DateTimeOffset? maxExecutionTime,
        [FromQuery] DateTimeOffset? periodStart,
        [FromQuery] DateTimeOffset? periodEnd)
    {
        var batches = await _batchesClient.SearchAsync(
            gridAreaCodes ?? Array.Empty<string>(),
            BatchStateMapper.MapState(executionState),
            minExecutionTime,
            maxExecutionTime,
            periodStart,
            periodEnd).ConfigureAwait(false);

        return Ok(batches);
    }
}
