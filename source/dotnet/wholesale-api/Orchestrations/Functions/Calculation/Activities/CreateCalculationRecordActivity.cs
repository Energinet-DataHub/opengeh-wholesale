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

using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Activities;

internal class CreateCalculationRecordActivity(
    ICreateCalculationHandler createCalculationHandler)
{
    private readonly ICreateCalculationHandler _createCalculationHandler = createCalculationHandler;

    /// <summary>
    /// Create calculation status record in SQL database.
    /// </summary>
    [Function(nameof(CreateCalculationRecordActivity))]
    public async Task<CalculationMetadata> Run(
        [ActivityTrigger] CalculationOrchestrationInput calculationOrchestrationInput)
    {
        var calculationId = await _createCalculationHandler.HandleAsync(new CreateCalculationCommand(
            calculationOrchestrationInput.StartCalculationRequestDto.CalculationType,
            calculationOrchestrationInput.StartCalculationRequestDto.GridAreaCodes,
            calculationOrchestrationInput.StartCalculationRequestDto.StartDate,
            calculationOrchestrationInput.StartCalculationRequestDto.EndDate,
            calculationOrchestrationInput.RequestedByUserId)).ConfigureAwait(false);

        return new CalculationMetadata
        {
            Id = calculationId,
            Input = calculationOrchestrationInput,
        };
    }
}
