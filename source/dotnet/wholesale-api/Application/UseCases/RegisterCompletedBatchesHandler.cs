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

using Energinet.DataHub.Wholesale.Application.UseCases.Mappers;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Domain;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;

namespace Energinet.DataHub.Wholesale.Application.UseCases;

public class RegisterCompletedBatchesHandler : IRegisterCompletedBatchesHandler
{
    private readonly IBatchesClient _batchesClient;
    private readonly ICompletedBatchRepository _completedBatchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompletedBatchMapper _completedBatchMapper;

    public RegisterCompletedBatchesHandler(IBatchesClient batchesClient, ICompletedBatchRepository completedBatchRepository, IUnitOfWork unitOfWork, ICompletedBatchMapper completedBatchMapper)
    {
        _batchesClient = batchesClient;
        _completedBatchRepository = completedBatchRepository;
        _unitOfWork = unitOfWork;
        _completedBatchMapper = completedBatchMapper;
    }

    public async Task RegisterCompletedBatchesAsync()
    {
        var newCompletedBatches = await GetNewCompletedBatchesAsync().ConfigureAwait(false);
        await _completedBatchRepository.AddAsync(newCompletedBatches).ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }

    private async Task<IEnumerable<CompletedBatch>> GetNewCompletedBatchesAsync()
    {
        var lastKnownCompletedBatch = await _completedBatchRepository.GetLastCompletedOrNullAsync().ConfigureAwait(false);
        var completedBatchDtos = await _batchesClient.GetBatchesCompletedAfterAsync(lastKnownCompletedBatch?.CompletedTime).ConfigureAwait(false);
        return _completedBatchMapper.Map(completedBatchDtos);
    }
}
