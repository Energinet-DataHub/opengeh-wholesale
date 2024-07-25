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

using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.Azure.Functions.Worker;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Activities;

internal class SendCalculationResultsActivity(
    ICalculationIntegrationEventPublisher integrationEventsPublisher,
    ICalculationRepository calculationRepository,
    ICalculationDtoMapper calculationDtoMapper,
    ICompletedCalculationFactory completedCalculationFactory,
    IClock clock,
    IUnitOfWork calculationUnitOfWork)
{
    private readonly ICalculationIntegrationEventPublisher _integrationEventsPublisher = integrationEventsPublisher;
    private readonly ICalculationRepository _calculationRepository = calculationRepository;
    private readonly ICalculationDtoMapper _calculationDtoMapper = calculationDtoMapper;
    private readonly ICompletedCalculationFactory _completedCalculationFactory = completedCalculationFactory;
    private readonly IClock _clock = clock;
    private readonly IUnitOfWork _calculationUnitOfWork = calculationUnitOfWork;

    /// <summary>
    /// Retrieve calculation results from Databricks and send them as events using ServiceBus.
    /// </summary>
    [Function(nameof(SendCalculationResultsActivity))]
    public async Task Run(
        [ActivityTrigger] SendCalculationResultsInput input)
    {
        var calculation = await _calculationRepository.GetAsync(input.CalculationId).ConfigureAwait(false);
        var calculationDto = _calculationDtoMapper.Map(calculation);

        var completedCalculation = _completedCalculationFactory.CreateFromCalculation(calculationDto, input.OrchestrationInstanceId);
        await _integrationEventsPublisher.PublishAsync(completedCalculation, CancellationToken.None).ConfigureAwait(false);

        calculation.MarkAsActorMessagesEnqueuing(_clock.GetCurrentInstant());
        await _calculationUnitOfWork.CommitAsync().ConfigureAwait(false);
    }
}
