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

using Energinet.DataHub.Wholesale.Application.Batches.Model;
using Energinet.DataHub.Wholesale.Application.CalculationJobs;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Application.Batches;

public class BatchApplicationService : IBatchApplicationService
{
    private readonly IBatchFactory _batchFactory;
    private readonly IBatchRepository _batchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICalculatorJobRunner _calculatorJobRunner;
    private readonly ICalculatorJobParametersFactory _calculatorJobParametersFactory;
    private readonly IBatchCompletedPublisher _batchCompletedPublisher;
    private readonly IBatchExecutionStateHandler _batchExecutionStateHandler;
    private readonly IBatchDtoMapper _batchDtoMapper;

    public BatchApplicationService(
        IBatchFactory batchFactory,
        IBatchRepository batchRepository,
        IUnitOfWork unitOfWork,
        ICalculatorJobRunner calculatorJobRunner,
        ICalculatorJobParametersFactory calculatorJobParametersFactory,
        IBatchCompletedPublisher batchCompletedPublisher,
        IBatchExecutionStateHandler batchExecutionStateHandler,
        IBatchDtoMapper batchDtoMapper)
    {
        _batchFactory = batchFactory;
        _batchRepository = batchRepository;
        _unitOfWork = unitOfWork;
        _calculatorJobRunner = calculatorJobRunner;
        _calculatorJobParametersFactory = calculatorJobParametersFactory;
        _batchCompletedPublisher = batchCompletedPublisher;
        _batchExecutionStateHandler = batchExecutionStateHandler;
        _batchDtoMapper = batchDtoMapper;
    }

    public async Task<Guid> CreateAsync(BatchRequestDto batchRequestDto)
    {
        var processType = batchRequestDto.ProcessType switch
        {
            ProcessType.BalanceFixing => Domain.ProcessAggregate.ProcessType.BalanceFixing,
            _ => throw new NotImplementedException($"Process type '{batchRequestDto.ProcessType}' not supported."),
        };
        var batch = _batchFactory.Create(processType, batchRequestDto.GridAreaCodes, batchRequestDto.StartDate, batchRequestDto.EndDate);
        await _batchRepository.AddAsync(batch).ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
        return batch.Id;
    }

    public async Task StartSubmittingAsync()
    {
        var batches = await _batchRepository.GetCreatedAsync().ConfigureAwait(false);

        foreach (var batch in batches)
        {
            var jobParameters = _calculatorJobParametersFactory.CreateParameters(batch);
            var jobRunId = await _calculatorJobRunner.SubmitJobAsync(jobParameters).ConfigureAwait(false);
            batch.MarkAsSubmitted(jobRunId);
            await _unitOfWork.CommitAsync().ConfigureAwait(false);
        }
    }

    public async Task UpdateExecutionStateAsync()
    {
        var completedBatches = await _batchExecutionStateHandler.UpdateExecutionStateAsync(_batchRepository, _calculatorJobRunner).ConfigureAwait(false);
        var batchCompletedEvents = completedBatches.Select(
            b => new BatchCompletedEventDto(b.Id, b.GridAreaCodes.Select(c => c.Code).ToList(), b.ProcessType, b.PeriodStart, b.PeriodEnd));

        await _unitOfWork.CommitAsync().ConfigureAwait(false);
        await _batchCompletedPublisher.PublishAsync(batchCompletedEvents).ConfigureAwait(false);
    }

    public async Task<IEnumerable<BatchDto>> SearchAsync(BatchSearchDto batchSearchDto)
    {
        var minExecutionTimeStart = Instant.FromDateTimeOffset(batchSearchDto.MinExecutionTime);
        var maxExecutionTimeStart = Instant.FromDateTimeOffset(batchSearchDto.MaxExecutionTime);
        var batches = await _batchRepository.GetAsync(minExecutionTimeStart, maxExecutionTimeStart)
            .ConfigureAwait(false);
        return batches.Select(_batchDtoMapper.Map);
    }

    public async Task<BatchDto> GetAsync(Guid batchId)
    {
        var batch = await _batchRepository.GetAsync(batchId).ConfigureAwait(false);
        return _batchDtoMapper.Map(batch);
    }
}
