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

using Energinet.DataHub.Wholesale.Batches.Application.Model.Batches;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Batches.Application;

public class BatchesClient : IBatchesClient
{
    private readonly IBatchRepository _batchRepository;
    private readonly IBatchDtoMapper _batchDtoMapper;

    public BatchesClient(IBatchRepository batchRepository, IBatchDtoMapper batchDtoMapper)
    {
        _batchRepository = batchRepository;
        _batchDtoMapper = batchDtoMapper;
    }

    public async Task<IEnumerable<BatchDto>> GetBatchesCompletedAfterAsync(Instant? completedTime)
    {
        var batches = await _batchRepository.GetCompletedAfterAsync(completedTime).ConfigureAwait(false);
        return batches.Select(_batchDtoMapper.Map);
    }
}
