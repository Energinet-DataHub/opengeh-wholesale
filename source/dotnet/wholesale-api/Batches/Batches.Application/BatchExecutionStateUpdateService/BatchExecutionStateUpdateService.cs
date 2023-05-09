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

using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Wholesale.Batches.Interfaces;

namespace Energinet.DataHub.Wholesale.Batches.Application.BatchExecutionStateUpdateService;

public class BatchExecutionStateUpdateService : IBatchExecutionStateUpdateService
{
    private readonly ICorrelationContext _correlationContext;
    private readonly IBatchApplicationService _batchApplicationService;

    public BatchExecutionStateUpdateService(ICorrelationContext correlationContext, IBatchApplicationService batchApplicationService)
    {
        _correlationContext = correlationContext;
        _batchApplicationService = batchApplicationService;
    }

    public async Task UpdateBatchExecutionStatesAsync()
    {
        // CorrelationIdMiddleware does not currently support timer triggered functions,
        // so we need to add a correlation ID ourselves
        _correlationContext.SetId(Guid.NewGuid().ToString());

        await _batchApplicationService.UpdateExecutionStateAsync().ConfigureAwait(false);
    }
}
