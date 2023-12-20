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

using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.Application.Communication;

public class IntegrationEventProvider : IIntegrationEventProvider
{
    private readonly IEnergyResultEventProvider _energyResultEventProvider;
    private readonly IWholesaleResultEventProvider _wholesaleResultEventProvider;
    private readonly ICompletedCalculationRepository _completedCalculationRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<IntegrationEventProvider> _logger;

    public IntegrationEventProvider(
        IEnergyResultEventProvider energyResultEventProvider,
        IWholesaleResultEventProvider wholesaleResultEventProvider,
        ICompletedCalculationRepository completedCalculationRepository,
        IClock clock,
        IUnitOfWork unitOfWork,
        ILogger<IntegrationEventProvider> logger)
    {
        _energyResultEventProvider = energyResultEventProvider;
        _wholesaleResultEventProvider = wholesaleResultEventProvider;
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
            var unpublishedBatch = await _completedCalculationRepository.GetNextUnpublishedOrNullAsync().ConfigureAwait(false);
            if (unpublishedBatch == null)
            {
                break;
            }

            // Publish integration events for energy results
            var energyResultCount = 0;
            var energyResultEventProviderEnumerator = _energyResultEventProvider.GetAsync(unpublishedBatch).GetAsyncEnumerator();
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
                        _logger.LogError(ex, "Failed energy result event publishing for completed calculation {calculation_id}. Handled '{EnergyResultCount}' energy results before failing.", unpublishedBatch.Id, energyResultCount);
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

            // Publish integration events for wholesale results
            var wholesaleResultCount = 0;
            if (_wholesaleResultEventProvider.CanContainWholesaleResults(unpublishedBatch))
            {
                var wholesaleResultEventProviderEnumerator = _wholesaleResultEventProvider.GetAsync(unpublishedBatch).GetAsyncEnumerator();
                try
                {
                    var hasResult = true;
                    while (hasResult)
                    {
                        try
                        {
                            hasResult = await wholesaleResultEventProviderEnumerator.MoveNextAsync().ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            hasResult = false;
                            hasFailed = true;
                            _logger.LogError(ex, "Failed wholesale result event publishing for completed calculation {calculation_id}. Handled '{WholesaleResultCount}' wholesale results before failing.", unpublishedBatch.Id, wholesaleResultCount);
                        }

                        if (hasResult)
                        {
                            wholesaleResultCount++;
                            yield return wholesaleResultEventProviderEnumerator.Current;
                        }
                    }
                }
                finally
                {
                    if (wholesaleResultEventProviderEnumerator != null)
                    {
                        await wholesaleResultEventProviderEnumerator.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }

            if (hasFailed)
            {
                // Quick fix: We currently do not have any status field to mark failures, so instead we set this property to a constant.
                unpublishedBatch.PublishedTime = NodaConstants.UnixEpoch;
            }
            else
            {
                unpublishedBatch.PublishedTime = _clock.GetCurrentInstant();
            }

            await _unitOfWork.CommitAsync().ConfigureAwait(false);

            _logger.LogInformation("Handled {EnergyResultCount} energy results for completed calculation {calculation_id}.", energyResultCount, unpublishedBatch.Id);
            if (_wholesaleResultEventProvider.CanContainWholesaleResults(unpublishedBatch))
            {
                _logger.LogInformation("Handled {WholesaleResultCount} wholesale results for completed calculation {calculation_id}.", wholesaleResultCount, unpublishedBatch.Id);
            }
        }
        while (true);
    }
}
