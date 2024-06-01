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
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Activities;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
internal class UpdateCalculationStatusActivity
{
    private readonly IClock _clock;
    private readonly IUnitOfWork _calculationUnitOfWork;
    private readonly ICalculationRepository _calculationRepository;
    private readonly ILogger<UpdateCalculationStatusActivity> _logger;

    public UpdateCalculationStatusActivity(
        IClock clock,
        IUnitOfWork calculationUnitOfWork,
        ICalculationRepository calculationRepository,
        ILogger<UpdateCalculationStatusActivity> logger)
    {
        _clock = clock;
        _calculationUnitOfWork = calculationUnitOfWork;
        _calculationRepository = calculationRepository;
        _logger = logger;
    }

    /// <summary>
    /// Update calculation status record in SQL database.
    /// </summary>
    [Function(nameof(UpdateCalculationStatusActivity))]
    public async Task<string> Run(
        [ActivityTrigger] CalculationMetadata calculationMetadata)
    {
        _logger.LogInformation(
            "Update calculation state from the calculation job status: {CalculationJobStatus}, calculation id: {CalculationId}",
            calculationMetadata.JobStatus,
            calculationMetadata.Id);

        var calculation = await _calculationRepository.GetAsync(calculationMetadata.Id);

        if (calculationMetadata.JobStatus == CalculationState.Canceled)
        {
            // Jobs may be cancelled in Databricks for various reasons. For example they can be cancelled due to migrations in CD
            // Setting calculation state back to "created" ensure they will be picked up and started again
            calculation.Reset();
        }
        else
        {
            var newState = CalculationStateMapper.MapState(calculationMetadata.JobStatus);

            // If state wasn't changed, do nothing
            if (calculation.OrchestrationState == newState)
            {
                _logger.LogInformation(
                    "Did not update calculation state since it didn't change. Current state: {CurrentOrchestrationState}, calculation id: {CalculationId}",
                    calculation.OrchestrationState,
                    calculationMetadata.Id);
                return "State didn't change from: " + calculation.OrchestrationState;
            }

            _logger.LogInformation(
                "Set new calculation state to: {NewOrchestrationState}, current state: {CurrentOrchestrationState}, calculation id: {CalculationId}",
                newState,
                calculation.OrchestrationState,
                calculationMetadata.Id);
            calculation.UpdateState(newState, _clock);
        }

        await _calculationUnitOfWork.CommitAsync();
        return "Orchestration state updated to: " + calculation.OrchestrationState;
    }
}
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
