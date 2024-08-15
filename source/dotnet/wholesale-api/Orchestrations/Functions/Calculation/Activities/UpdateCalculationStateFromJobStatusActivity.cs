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
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Activities;

internal class UpdateCalculationStateFromJobStatusActivity(
    IClock clock,
    IUnitOfWork calculationUnitOfWork,
    ICalculationRepository calculationRepository,
    ILogger<UpdateCalculationStateFromJobStatusActivity> logger)
{
    private readonly IClock _clock = clock;
    private readonly IUnitOfWork _calculationUnitOfWork = calculationUnitOfWork;
    private readonly ICalculationRepository _calculationRepository = calculationRepository;
    private readonly ILogger<UpdateCalculationStateFromJobStatusActivity> _logger = logger;

    /// <summary>
    /// Update calculation status record in SQL database.
    /// </summary>
    [Function(nameof(UpdateCalculationStateFromJobStatusActivity))]
    public async Task<string> Run(
        [ActivityTrigger] CalculationMetadata calculationMetadata)
    {
        _logger.LogInformation(
            "Update calculation state from the calculation job status: {CalculationJobStatus}, calculation id: {CalculationId}",
            calculationMetadata.JobStatus,
            calculationMetadata.Id);
        var calculation = await _calculationRepository.GetAsync(calculationMetadata.Id).ConfigureAwait(false);

        var previousState = calculation.OrchestrationState;
        if (calculationMetadata.JobStatus == CalculationState.Canceled)
        {
            // Jobs may be cancelled in Databricks for various reasons. For example they can be cancelled due to migrations in CD
            // Setting calculation state back to "created" ensure they will be picked up and started again
            calculation.Reset();
        }
        else
        {
            switch (calculationMetadata.JobStatus)
            {
                case CalculationState.Pending:
                    calculation.MarkAsCalculationJobPending();
                    break;
                case CalculationState.Running:
                    calculation.MarkAsCalculating();
                    break;
                case CalculationState.Completed:
                    calculation.MarkAsCalculated(_clock.GetCurrentInstant());
                    break;
                case CalculationState.Failed:
                    calculation.MarkAsCalculationFailed();
                    break;

                // Application.Model.CalculationState.Canceled cannot be mapped, since it is a databricks thing unrelated to domain
                case CalculationState.Canceled:
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(calculationMetadata.JobStatus),
                        actualValue: calculationMetadata.JobStatus,
                        "Value cannot be used to update calculation state.");
            }
        }

        _logger.LogInformation(
            "Updated calculation state to: {NewOrchestrationState}, previous state: {PreviousOrchestrationState}, calculation id: {CalculationId}",
            calculation.OrchestrationState,
            previousState,
            calculationMetadata.Id);

        await _calculationUnitOfWork.CommitAsync().ConfigureAwait(false);
        return "Orchestration state updated to: " + calculation.OrchestrationState;
    }
}
