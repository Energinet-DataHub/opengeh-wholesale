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
// limitations under the License.using Energinet.DataHub.Wholesale.Common.JobRunner;

using Energinet.DataHub.Wholesale.Batches.Application;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Calculations;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Batches.Infrastructure.CalculationState;

public class CalculationExecutionStateInfrastructureService : ICalculationExecutionStateInfrastructureService
{
    private readonly ICalculationRepository _calculationRepository;
    private readonly ICalculationInfrastructureService _calculationInfrastructureService;
    private readonly IClock _clock;
    private readonly ILogger _logger;

    public CalculationExecutionStateInfrastructureService(
        ICalculationRepository calculationRepository,
        ICalculationInfrastructureService calculationInfrastructureService,
        ILogger<CalculationExecutionStateInfrastructureService> logger,
        IClock clock)
    {
        _calculationRepository = calculationRepository;
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
        var completedBatches = new List<Calculation>();
        var states = new List<CalculationExecutionState>
        {
            CalculationExecutionState.Submitted, CalculationExecutionState.Pending, CalculationExecutionState.Executing,
        };
        var activeBatches = await _calculationRepository.GetByStatesAsync(states).ConfigureAwait(false);
        foreach (var batch in activeBatches)
        {
            try
            {
                var jobState = await _calculationInfrastructureService
                    .GetStatusAsync(batch.CalculationId!)
                    .ConfigureAwait(false);

                var executionState = CalculationStateMapper.MapState(jobState);
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

    private void HandleNewState(CalculationExecutionState state, Calculation calculation, ICollection<Calculation> completedBatches)
    {
        switch (state)
        {
            case CalculationExecutionState.Pending:
                calculation.MarkAsPending();
                break;
            case CalculationExecutionState.Executing:
                calculation.MarkAsExecuting();
                break;
            case CalculationExecutionState.Completed:
                calculation.MarkAsCompleted(_clock.GetCurrentInstant());
                completedBatches.Add(calculation);
                break;
            case CalculationExecutionState.Failed:
                calculation.MarkAsFailed();
                break;
            case CalculationExecutionState.Canceled:
                // Jobs may be cancelled in Databricks for various reasons. For example they can be cancelled due to migrations in CD
                // Setting batch state back to "created" ensure they will be picked up and started again
                calculation.Reset();
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unexpected execution state: {state.ToString()}.");
        }
    }
}
