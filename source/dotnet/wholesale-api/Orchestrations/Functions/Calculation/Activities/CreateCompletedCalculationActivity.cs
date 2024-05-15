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
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Activities;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
internal class CreateCompletedCalculationActivity(
    ICalculationRepository calculationRepository,
    ICalculationDtoMapper calculationDtoMapper,
    ICompletedCalculationFactory completedCalculationFactory,
    Events.Application.UseCases.IUnitOfWork eventsUnitOfWork,
    ICompletedCalculationRepository completedCalculationRepository)
{
    private readonly ICalculationRepository _calculationRepository = calculationRepository;
    private readonly ICalculationDtoMapper _calculationDtoMapper = calculationDtoMapper;
    private readonly ICompletedCalculationFactory _completedCalculationFactory = completedCalculationFactory;
    private readonly Events.Application.UseCases.IUnitOfWork _eventsUnitOfWork = eventsUnitOfWork;
    private readonly ICompletedCalculationRepository _completedCalculationRepository = completedCalculationRepository;

    /// <summary>
    /// Update calculation status record in SQL database.
    /// </summary>
    [Function(nameof(CreateCompletedCalculationActivity))]
    public async Task Run(
        [ActivityTrigger] CreateCompletedCalculationInput input)
    {
        var calculation = await _calculationRepository.GetAsync(input.CalculationId);
        var calculationDto = _calculationDtoMapper.Map(calculation);

        var completedCalculation = _completedCalculationFactory.CreateFromCalculation(calculationDto, input.OrchestrationInstanceId);
        await _completedCalculationRepository.AddAsync([completedCalculation]);
        await _eventsUnitOfWork.CommitAsync();
    }
}
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
