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

using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.CalculationState;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.Azure.Functions.Worker;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Activities;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
internal class UpdateCalculationStatusActivity(
    IClock clock,
    IUnitOfWork calculationUnitOfWork,
    ICalculationRepository calculationRepository)
{
    private readonly IClock _clock = clock;
    private readonly IUnitOfWork _calculationUnitOfWork = calculationUnitOfWork;
    private readonly ICalculationRepository _calculationRepository = calculationRepository;

    /// <summary>
    /// Update calculation status record in SQL database.
    /// </summary>
    [Function(nameof(UpdateCalculationStatusActivity))]
    public async Task Run(
        [ActivityTrigger] CalculationMetadata calculationMetadata)
    {
        var calculation = await _calculationRepository.GetAsync(calculationMetadata.Id);

        if (calculationMetadata.JobStatus == CalculationState.Canceled)
        {
            // Jobs may be cancelled in Databricks for various reasons. For example they can be cancelled due to migrations in CD
            // Setting calculation state back to "created" ensure they will be picked up and started again
            // TODO: Is this still handled after moving to durable function?
            calculation.Reset();
        }
        else
        {
            var newState = CalculationStateMapper.MapState(calculationMetadata.JobStatus);

            // If state wasn't changed, do nothing
            if (calculation.OrchestrationState == newState)
                return;

            UpdateState(calculation, newState);
        }

        await _calculationUnitOfWork.CommitAsync();
    }

    private void UpdateState(Calculations.Application.Model.Calculations.Calculation calculation, CalculationOrchestrationState newState)
    {
        switch (newState)
        {
            case CalculationOrchestrationState.Scheduled:
                calculation.MarkAsScheduled();
                break;
            case CalculationOrchestrationState.Calculating:
                calculation.MarkAsCalculating();
                break;
            case CalculationOrchestrationState.Calculated:
                calculation.MarkAsCalculated(_clock.GetCurrentInstant());
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
                throw new ArgumentOutOfRangeException(nameof(newState), newState, $"Unexpected orchestration state: {newState}.");
        }
    }
}
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
