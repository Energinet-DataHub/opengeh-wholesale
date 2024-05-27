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
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.Azure.Functions.Worker;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Activities;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
internal class SetCalculationOrchestrationStateActivity(
    IClock clock,
    IUnitOfWork calculationUnitOfWork,
    ICalculationRepository calculationRepository)
{
    /// <summary>
    /// Update calculation status record in SQL database.
    /// </summary>
    [Function(nameof(SetCalculationOrchestrationStateActivity))]
    public async Task Run(
        [ActivityTrigger] SetCalculationOrchestrationStateInput input)
    {
        var calculation = await calculationRepository.GetAsync(input.CalculationId);

        UpdateState(calculation, input.State);

        await calculationUnitOfWork.CommitAsync();
    }

    private void UpdateState(Calculations.Application.Model.Calculations.Calculation calculation, CalculationOrchestrationState newState)
    {
        calculation.UpdateState(newState, clock.GetCurrentInstant());
    }
}
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
