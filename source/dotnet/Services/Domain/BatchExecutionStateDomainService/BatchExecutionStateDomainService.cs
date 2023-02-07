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
// limitations under the License.using Energinet.DataHub.Wholesale.Application.JobRunner;

using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.CalculationDomainService;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Domain.BatchExecutionStateDomainService;

public class BatchExecutionStateDomainService : IBatchExecutionStateDomainService
{
    private readonly IBatchRepository _batchRepository;
    private readonly ICalculationDomainService _calculationDomainService;
    private readonly ILogger _logger;
    private readonly IClock _clock;
    private readonly IDomainEventPublisher _domainEventPublisher;

    public BatchExecutionStateDomainService(
        IBatchRepository batchRepository,
        ICalculationDomainService calculationDomainService,
        ILogger<BatchExecutionStateDomainService> logger,
        IClock clock,
        IDomainEventPublisher domainEventPublisher)
    {
        _batchRepository = batchRepository;
        _calculationDomainService = calculationDomainService;
        _logger = logger;
        _clock = clock;
        _domainEventPublisher = domainEventPublisher;
    }

    /// <summary>
    /// Update the execution states in the batch repository by mapping the job states from the runs <see cref="ICalculationDomainService"/>
    /// </summary>
    /// <returns>Batches that have been completed</returns>
    public async Task UpdateExecutionStateAsync()
    {
        var completedBatches = new List<Batch>();
        var states = new List<BatchExecutionState>
        {
            BatchExecutionState.Submitted, BatchExecutionState.Pending, BatchExecutionState.Executing,
        };
        var activeBatches = await _batchRepository.GetByStatesAsync(states).ConfigureAwait(false);
        foreach (var batch in activeBatches)
        {
            try
            {
                var jobState = await _calculationDomainService
                    .GetStatusAsync(batch.CalculationId!)
                    .ConfigureAwait(false);

                var executionState = MapState(jobState);
                if (executionState != batch.ExecutionState)
                {
                    HandleNewState(executionState, batch, completedBatches);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception caught while trying to update execution state for run ID {BatchRunId}", batch.CalculationId);
            }
        }

        var batchCompletedEvents = completedBatches
            .Select(b => new BatchCompletedEventDto(b.Id, b.GridAreaCodes.Select(c => c.Code).ToList(), b.ProcessType, b.PeriodStart, b.PeriodEnd))
            .ToList();
        await _domainEventPublisher.PublishAsync(batchCompletedEvents).ConfigureAwait(false);
    }

    private static BatchExecutionState MapState(CalculationState calculationState)
    {
        return calculationState switch
        {
            CalculationState.Pending => BatchExecutionState.Pending,
            CalculationState.Running => BatchExecutionState.Executing,
            CalculationState.Completed => BatchExecutionState.Completed,
            CalculationState.Canceled => BatchExecutionState.Canceled,
            CalculationState.Failed => BatchExecutionState.Failed,
            _ => throw new ArgumentOutOfRangeException(nameof(calculationState), calculationState, "Unexpected JobState."),
        };
    }

    private void HandleNewState(BatchExecutionState state, Batch batch, ICollection<Batch> completedBatches)
    {
        switch (state)
        {
            case BatchExecutionState.Pending:
                batch.MarkAsPending();
                break;
            case BatchExecutionState.Executing:
                batch.MarkAsExecuting();
                break;
            case BatchExecutionState.Completed:
                batch.MarkAsCompleted(_clock.GetCurrentInstant());
                completedBatches.Add(batch);
                break;
            case BatchExecutionState.Failed:
                batch.MarkAsFailed();
                break;
            case BatchExecutionState.Canceled:
                // Jobs may be cancelled in Databricks for various reasons. For example they can be cancelled due to migrations in CD
                // Setting batch state back to "created" ensure they will be picked up and started again
                batch.Reset();
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unexpected execution state: {state.ToString()}.");
        }
    }
}
