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
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Activities;

internal class CreateCalculationRecordActivity(
    ICalculationFactory calculationFactory,
    ICalculationRepository calculationRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateCalculationRecordActivity> logger)
{
    private readonly ICalculationFactory _calculationFactory = calculationFactory;
    private readonly ICalculationRepository _calculationRepository = calculationRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<CreateCalculationRecordActivity> _logger = logger;

    /// <summary>
    /// Create calculation status record in SQL database.
    /// </summary>
    [Function(nameof(CreateCalculationRecordActivity))]
    public async Task<CalculationMetadata> Run(
        [ActivityTrigger] CalculationOrchestrationInput calculationOrchestrationInput)
    {
        var calculation = _calculationFactory.Create(
            calculationOrchestrationInput.StartCalculationRequestDto.CalculationType,
            calculationOrchestrationInput.StartCalculationRequestDto.GridAreaCodes,
            calculationOrchestrationInput.StartCalculationRequestDto.StartDate,
            calculationOrchestrationInput.StartCalculationRequestDto.EndDate,
            DateTimeOffset.UtcNow,
            calculationOrchestrationInput.RequestedByUserId);
        await _calculationRepository.AddAsync(calculation).ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);

        _logger.LogInformation("Calculation created with id {calculation_id}", calculation.Id);

        return new CalculationMetadata
        {
            Id = calculation.Id,
            Input = calculationOrchestrationInput,
        };
    }
}
