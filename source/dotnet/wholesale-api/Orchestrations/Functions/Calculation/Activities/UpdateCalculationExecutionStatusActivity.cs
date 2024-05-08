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
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.CalculationState;
using Energinet.DataHub.Wholesale.Common.Application;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.Azure.Functions.Worker;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Activities;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
internal class UpdateCalculationExecutionStatusActivity(
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
    [Function(nameof(UpdateCalculationExecutionStatusActivity))]
    public async Task Run(
        [ActivityTrigger] CalculationMetadata calculationMetadata)
    {
        var calculation = await _calculationRepository.GetAsync(calculationMetadata.Id);
        var newExecutionState = CalculationStateMapper.MapState(calculationMetadata.JobStatus);

        if (calculation.ExecutionState != newExecutionState)
        {
            switch (newExecutionState)
            {
                case CalculationExecutionState.Pending:
                    calculation.MarkAsPending();
                    break;
                case CalculationExecutionState.Executing:
                    calculation.MarkAsExecuting();
                    break;
                case CalculationExecutionState.Completed:
                    calculation.MarkAsCompleted(_clock.GetCurrentInstant());
                    break;
                case CalculationExecutionState.Failed:
                    calculation.MarkAsFailed();
                    break;
                case CalculationExecutionState.Canceled:
                    // Jobs may be cancelled in Databricks for various reasons. For example they can be cancelled due to migrations in CD
                    // Setting calculation state back to "created" ensure they will be picked up and started again
                    calculation.Reset();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unexpected execution state: {newExecutionState}.");
            }

            await _calculationUnitOfWork.CommitAsync();
        }
    }
}
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
