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

using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Microsoft.Azure.Functions.Worker;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Activities;

internal class SendCalculationResultsActivity(
    IPublisher integrationEventsPublisher,
    ICalculationRepository calculationRepository,
    ICompletedCalculationRepository completedCalculationRepository,
    IClock clock,
    IUnitOfWork calculationUnitOfWork)
{
    private readonly IPublisher _integrationEventsPublisher = integrationEventsPublisher;
    private readonly ICompletedCalculationRepository _completedCalculationRepository = completedCalculationRepository;
    private readonly ICalculationRepository _calculationRepository = calculationRepository;
    private readonly IClock _clock = clock;
    private readonly IUnitOfWork _calculationUnitOfWork = calculationUnitOfWork;

    /// <summary>
    /// Retrieve calculation results from Databricks and send them as events using ServiceBus.
    /// </summary>
    [Function(nameof(SendCalculationResultsActivity))]
    public async Task Run(
        [ActivityTrigger] Guid calculationId)
    {
        //// TODO - XDAST refactoring:
        ////  - Read calculation by id from database
        ////  - Publish integration events by calculation id (currently this is done in a loop, which we don't want to)
        ////  - Do not update database with "published" state; if publishing fails, the activity should be retried instead.

        await _integrationEventsPublisher.PublishAsync(CancellationToken.None).ConfigureAwait(false);

        var completedCalculation = await _completedCalculationRepository.GetAsync(calculationId).ConfigureAwait(false);

        if (completedCalculation.PublishFailed)
            throw new Exception($"Publish failed for completed calculation (id: {calculationId})");

        if (!completedCalculation.IsPublished)
            throw new Exception($"Completed calculation (id: {calculationId}) was not published");

        var calculation = await _calculationRepository.GetAsync(calculationId).ConfigureAwait(false);
        calculation.MarkAsActorMessagesEnqueuing(_clock.GetCurrentInstant());

        await _calculationUnitOfWork.CommitAsync().ConfigureAwait(false);
    }
}
