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

using Energinet.DataHub.Wholesale.Application.JobRunner;
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;

namespace Energinet.DataHub.Wholesale.Application.Batches;

public class BatchApplicationService : IBatchApplicationService
{
    private readonly IBatchRepository _batchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProcessCompletedPublisher _processCompletedPublisher;
    private readonly ICalculatorJobRunner _calculatorJobRunner;
    private readonly ICalculatorJobParametersFactory _calculatorJobParametersFactory;

    public BatchApplicationService(
        IBatchRepository batchRepository,
        IUnitOfWork unitOfWork,
        IProcessCompletedPublisher processCompletedPublisher,
        ICalculatorJobRunner calculatorJobRunner,
        ICalculatorJobParametersFactory calculatorJobParametersFactory)
    {
        _batchRepository = batchRepository;
        _unitOfWork = unitOfWork;
        _processCompletedPublisher = processCompletedPublisher;
        _calculatorJobRunner = calculatorJobRunner;
        _calculatorJobParametersFactory = calculatorJobParametersFactory;
    }

    public async Task CreateAsync(BatchRequestDto batchRequestDto)
    {
        var batch = CreateBatch(batchRequestDto);
        await _batchRepository.AddAsync(batch).ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }

    public async Task StartPendingAsync()
    {
        var batches = await _batchRepository.GetPendingAsync().ConfigureAwait(false);

        foreach (var batch in batches)
        {
            var jobParameters = _calculatorJobParametersFactory.CreateParameters(batch);
            var jobRunId = await _calculatorJobRunner.SubmitJobAsync(jobParameters).ConfigureAwait(false);
            batch.SetExecuting(jobRunId);
            await _unitOfWork.CommitAsync().ConfigureAwait(false);
        }
    }

    public async Task UpdateExecutionStateAsync()
    {
        var batches = await _batchRepository.GetExecutingAsync().ConfigureAwait(false);
        if (!batches.Any())
            return;

        var completedBatches = new List<Batch>();

        foreach (var batch in batches)
        {
            // The batch will have received a RunId when the batch have started.
            var runId = batch.RunId!;

            var state = await _calculatorJobRunner
                .GetJobStateAsync(runId)
                .ConfigureAwait(false);

            if (state == JobState.Completed)
            {
                batch.Complete();
                completedBatches.Add(batch);
            }
        }

        var completedProcesses = CreateProcessCompletedEvents(completedBatches);
        await _processCompletedPublisher.PublishAsync(completedProcesses).ConfigureAwait(false);

        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }

    private static Batch CreateBatch(BatchRequestDto batchRequestDto)
    {
        var gridAreaCodes = batchRequestDto.GridAreaCodes.Select(c => new GridAreaCode(c));
        var processType = batchRequestDto.ProcessType switch
        {
            WholesaleProcessType.BalanceFixing => ProcessType.BalanceFixing,
            _ => throw new NotImplementedException($"Process type '{batchRequestDto.ProcessType}' not supported."),
        };
        var batch = new Batch(processType, gridAreaCodes);
        return batch;
    }

    private List<ProcessCompletedEventDto> CreateProcessCompletedEvents(List<Batch> completedBatches)
    {
        return completedBatches
            .SelectMany(b => b.GridAreaCodes.Select(x => new { b.Id, x.Code }))
            .Select(c => new ProcessCompletedEventDto(c.Code, c.Id.Value))
            .ToList();
    }
}
