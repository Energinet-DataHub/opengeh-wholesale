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

using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.Application.Communication;

public class IntegrationEventProvider : IIntegrationEventProvider
{
    private readonly ICalculationCompletedEventProvider _calculationCompletedEventProvider;
    private readonly IEnergyResultEventProvider _energyResultEventProvider;
    private readonly ICompletedCalculationRepository _completedCalculationRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<IntegrationEventProvider> _logger;

    public IntegrationEventProvider(
        ICalculationCompletedEventProvider calculationCompletedEventProvider,
        IEnergyResultEventProvider energyResultEventProvider,
        ICompletedCalculationRepository completedCalculationRepository,
        IClock clock,
        IUnitOfWork unitOfWork,
        ILogger<IntegrationEventProvider> logger)
    {
        _calculationCompletedEventProvider = calculationCompletedEventProvider;
        _energyResultEventProvider = energyResultEventProvider;
        _completedCalculationRepository = completedCalculationRepository;
        _clock = clock;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async IAsyncEnumerable<IntegrationEvent> GetAsync()
    {
        do
        {
            var hasFailed = false;
            var unpublishedCalculation = await _completedCalculationRepository.GetNextUnpublishedOrNullAsync().ConfigureAwait(false);
            if (unpublishedCalculation == null)
            {
                break;
            }

            // Publish integration events for energy results
            var energyResultCount = 0;
            var energyResultEventProviderEnumerator = _energyResultEventProvider.GetAsync(unpublishedCalculation).GetAsyncEnumerator();
            try
            {
                var hasResult = true;
                while (hasResult)
                {
                    try
                    {
                        hasResult = await energyResultEventProviderEnumerator.MoveNextAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        hasResult = false;
                        hasFailed = true;
                        _logger.LogError(ex, "Failed energy result event publishing for completed calculation {calculation_id}. Handled '{energy_result_count}' energy results before failing.", unpublishedCalculation.Id, energyResultCount);
                    }

                    if (hasResult)
                    {
                        energyResultCount++;
                        yield return energyResultEventProviderEnumerator.Current;
                    }
                }
            }
            finally
            {
                if (energyResultEventProviderEnumerator != null)
                {
                    await energyResultEventProviderEnumerator.DisposeAsync().ConfigureAwait(false);
                }
            }

            // Publish integration events for calculation completed
            IntegrationEvent? calculationCompletedEvent = default;
            try
            {
                calculationCompletedEvent = _calculationCompletedEventProvider.Get(unpublishedCalculation);
            }
            catch (Exception ex)
            {
                hasFailed = true;
                _logger.LogError(ex, "Failed calculation completed event publishing for completed calculation {calculation_id}.", unpublishedCalculation.Id);
            }

            if (calculationCompletedEvent != null)
            {
                yield return calculationCompletedEvent;
            }

            if (hasFailed)
            {
                unpublishedCalculation.SetPublishFailed();
            }
            else
            {
                unpublishedCalculation.SetPublished(_clock.GetCurrentInstant());
            }

            await _unitOfWork.CommitAsync().ConfigureAwait(false);

            _logger.LogInformation("Published results for succeeded energy calculation {calculation_id} to the service bus ({energy_result_count} integration events).", unpublishedCalculation.Id, energyResultCount);
        }
        while (true);
    }
}
