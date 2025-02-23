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
// limitations under the License.

using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Activities;

internal class SetCalculationAsStartedActivity(
    ICalculationRepository calculationRepository,
    IUnitOfWork unitOfWork)
{
    private readonly ICalculationRepository _calculationRepository = calculationRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    /// Set calculation orchestration id on calculation entity
    /// </summary>
    [Function(nameof(SetCalculationAsStartedActivity))]
    public async Task Run(
        [ActivityTrigger] SetCalculationAsStartedInput input)
    {
        var calculation = await _calculationRepository.GetAsync(input.CalculationId).ConfigureAwait(false);

        if (calculation.OrchestrationInstanceId.Id != input.OrchestrationInstanceId)
            throw new InvalidOperationException($"Cannot set calculation {calculation.Id} as started. Orchestration instance id does not match the given orchestration instance id ({input.OrchestrationInstanceId}).");

        calculation.MarkAsStarted();
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }
}
