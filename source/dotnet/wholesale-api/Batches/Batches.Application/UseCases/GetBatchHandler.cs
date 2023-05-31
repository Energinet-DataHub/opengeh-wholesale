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

namespace Energinet.DataHub.Wholesale.Batches.Application.UseCases;

public class GetBatchHandler : IGetBatchHandler
{
    private readonly IBatchRepository _batchRepository;
    private readonly IBatchDtoMapper _batchDtoMapper;

    public GetBatchHandler(IBatchRepository batchRepository, IBatchDtoMapper batchDtoMapper)
    {
        _batchRepository = batchRepository;
        _batchDtoMapper = batchDtoMapper;
    }

    public async Task<BatchDto> GetAsync(Guid batchId)
    {
        var batch = await _batchRepository.GetAsync(batchId).ConfigureAwait(false);
        return _batchDtoMapper.Map(batch);
    }
}
