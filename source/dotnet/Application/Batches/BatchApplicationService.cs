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

using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Domain;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;

namespace Energinet.DataHub.Wholesale.Application.Batches;

public class BatchApplicationService : IBatchApplicationService
{
    private readonly IBatchRepository _batchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBatchRunner _batchRunner;
    private readonly IProcessCompletedPublisher _processCompletedPublisher;

    public BatchApplicationService(IBatchRepository batchRepository, IUnitOfWork unitOfWork, IBatchRunner batchRunner, IProcessCompletedPublisher processCompletedPublisher)
    {
        _batchRepository = batchRepository;
        _unitOfWork = unitOfWork;
        _batchRunner = batchRunner;
        _processCompletedPublisher = processCompletedPublisher;
    }

    public async Task CreateAsync(BatchRequestDto batchRequestDto)
    {
        var gridAreaCodes = batchRequestDto.GridAreaCodes.Select(c => new GridAreaCode(c));
        var batch = new Batch(batchRequestDto.ProcessType, gridAreaCodes);

        await _batchRepository.AddAsync(batch).ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }

    public async Task StartRequestedAsync()
    {
        var requestedBatches = await _batchRepository.GetRequestedAsync().ConfigureAwait(false);
        await _batchRunner.BeginExecuteAsync(requestedBatches).ConfigureAwait(false);

        requestedBatches.ForEach(b => b.SetExecuting());
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }

    public async Task UpdateBatchStatesAsync()
    {
        var executingBatches = await _batchRepository.GetExecutingAsync().ConfigureAwait(false);
        var completedBatches = await _batchRunner.GetCompletedAsync(executingBatches).ConfigureAwait(false);
        completedBatches.ForEach(b => b.Complete());

        var completedProcesses = CreateProcessCompletedEvents(completedBatches);
        await _processCompletedPublisher.PublishAsync(completedProcesses).ConfigureAwait(false);

        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }

    private List<ProcessCompletedEventDto> CreateProcessCompletedEvents(List<Batch> completedBatches)
    {
        return completedBatches
            .SelectMany(b => b.GridAreaCodes)
            .Select(c => new ProcessCompletedEventDto(c.Code))
            .ToList();
    }
}
