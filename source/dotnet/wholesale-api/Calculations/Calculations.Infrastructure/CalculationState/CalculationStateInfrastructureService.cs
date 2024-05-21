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

using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Calculations.Infrastructure.CalculationState;

public class CalculationStateInfrastructureService : ICalculationStateInfrastructureService
{
    private readonly ICalculationRepository _calculationRepository;
    private readonly ICalculationInfrastructureService _calculationInfrastructureService;
    private readonly IClock _clock;
    private readonly ILogger _logger;

    public CalculationStateInfrastructureService(
        ICalculationRepository calculationRepository,
        ICalculationInfrastructureService calculationInfrastructureService,
        ILogger<CalculationStateInfrastructureService> logger,
        IClock clock)
    {
        _calculationRepository = calculationRepository;
        _calculationInfrastructureService = calculationInfrastructureService;
        _logger = logger;
        _clock = clock;
    }

    /// <summary>
    /// Update the states in the calculation repository by mapping the job states from the runs <see cref="ICalculationInfrastructureService"/>
    /// </summary>
    /// <returns>Calculations that have been completed</returns>
    public async Task UpdateStateAsync()
    {
        var completedCalculations = new List<Calculation>();
        var states = new List<CalculationExecutionState>
        {
            CalculationExecutionState.Submitted, CalculationExecutionState.Pending, CalculationExecutionState.Executing,
        };
        var activeCalculations = await _calculationRepository.GetByStatesAsync(states).ConfigureAwait(false);
        foreach (var calculation in activeCalculations)
        {
            try
            {
                var jobState = await _calculationInfrastructureService
                    .GetStatusAsync(calculation.CalculationJobId!)
                    .ConfigureAwait(false);

                if (jobState == Application.Model.CalculationState.Canceled)
                {
                    // Jobs may be cancelled in Databricks for various reasons. For example they can be cancelled due to migrations in CD
                    // Setting calculation state back to "created" ensure they will be picked up and started again
                    calculation.Reset();
                    continue;
                }

                var state = CalculationStateMapper.MapState(jobState);
                if (state != calculation.OrchestrationState)
                {
                    HandleNewState(state, calculation, completedCalculations);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception caught while trying to update execution state for run ID {calculation_id}", calculation.CalculationJobId);
            }
        }
    }

    private void HandleNewState(CalculationOrchestrationState state, Calculation calculation, ICollection<Calculation> completedCalculations)
    {
        switch (state)
        {
            case CalculationOrchestrationState.Scheduled:
                calculation.MarkAsPending();
                break;
            case CalculationOrchestrationState.Calculating:
                calculation.MarkAsCalculating();
                break;
            case CalculationOrchestrationState.Calculated:
                calculation.MarkAsCalculated(_clock.GetCurrentInstant());
                completedCalculations.Add(calculation);
                break;
            case CalculationOrchestrationState.CalculationFailed:
                calculation.MarkAsCalculationFailed();
                break;
            case CalculationOrchestrationState.MessagesEnqueuing:
                calculation.MarkAsMessagesEnqueuing(_clock.GetCurrentInstant());
                break;
            case CalculationOrchestrationState.MessagesEnqueued:
                calculation.MarkAsMessagesEnqueued(_clock.GetCurrentInstant());
                break;
            case CalculationOrchestrationState.MessagesEnqueuingFailed:
                calculation.MarkAsMessagesEnqueuingFailed();
                break;
            case CalculationOrchestrationState.Completed:
                calculation.MarkAsCompleted(_clock.GetCurrentInstant());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, "Unhandled CalculationOrchestrationState when changing state");
        }
    }
}
