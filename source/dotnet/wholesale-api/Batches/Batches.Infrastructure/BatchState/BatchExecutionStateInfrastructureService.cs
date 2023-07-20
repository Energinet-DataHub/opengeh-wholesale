﻿// Copyright 2020 Energinet DataHub A/S
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
// limitations under the License.using Energinet.DataHub.Wholesale.Common.JobRunner;

using Energinet.DataHub.Wholesale.Batches.Application;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Batches;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Batches.Infrastructure.BatchState;

public class BatchExecutionStateInfrastructureService : IBatchExecutionStateInfrastructureService
{
    private readonly IBatchRepository _batchRepository;
    private readonly ICalculationInfrastructureService _calculationInfrastructureService;
    private readonly IClock _clock;
    private readonly ILogger _logger;

    public BatchExecutionStateInfrastructureService(
        IBatchRepository batchRepository,
        ICalculationInfrastructureService calculationInfrastructureService,
        ILogger<BatchExecutionStateInfrastructureService> logger,
        IClock clock)
    {
        _batchRepository = batchRepository;
        _calculationInfrastructureService = calculationInfrastructureService;
        _logger = logger;
        _clock = clock;
    }

    /// <summary>
    /// Update the execution states in the batch repository by mapping the job states from the runs <see cref="ICalculationInfrastructureService"/>
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
                var jobState = await _calculationInfrastructureService
                    .GetStatusAsync(batch.CalculationId!)
                    .ConfigureAwait(false);

                var executionState = BatchStateMapper.MapState(jobState);
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
