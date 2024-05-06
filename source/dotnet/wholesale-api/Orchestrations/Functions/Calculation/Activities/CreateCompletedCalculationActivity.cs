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
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Activities;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
internal class CreateCompletedCalculationActivity
{
    private readonly ICalculationRepository _calculationRepository;
    private readonly ICalculationDtoMapper _calculationDtoMapper;
    private readonly ICompletedCalculationFactory _completedCalculationFactory;
    private readonly Events.Application.UseCases.IUnitOfWork _eventsUnitOfWork;
    private readonly ICompletedCalculationRepository _completedCalculationRepository;

    public CreateCompletedCalculationActivity(
        ICalculationRepository calculationRepository,
        ICalculationDtoMapper calculationDtoMapper,
        ICompletedCalculationFactory completedCalculationFactory,
        Events.Application.UseCases.IUnitOfWork eventsUnitOfWork,
        ICompletedCalculationRepository completedCalculationRepository)
    {
        _calculationRepository = calculationRepository;
        _calculationDtoMapper = calculationDtoMapper;
        _completedCalculationFactory = completedCalculationFactory;
        _eventsUnitOfWork = eventsUnitOfWork;
        _completedCalculationRepository = completedCalculationRepository;
    }

    /// <summary>
    /// Update calculation status record in SQL database.
    /// </summary>
    [Function(nameof(CreateCompletedCalculationActivity))]
    public async Task Run(
        [ActivityTrigger] Guid calculationdId)
    {
        var calculation = await _calculationRepository.GetAsync(calculationdId);
        var calculationDto = _calculationDtoMapper.Map(calculation);

        var completedCalculations = _completedCalculationFactory.CreateFromCalculations([calculationDto]);
        await _completedCalculationRepository.AddAsync(completedCalculations);
        await _eventsUnitOfWork.CommitAsync();
    }
}
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
