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
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.Application.Communication;

public class IntegrationEventProvider : IIntegrationEventProvider
{
    private readonly IEnergyResultEventProvider _energyResultEventProvider;
    private readonly IWholesaleResultEventProvider _wholesaleResultEventProvider;
    private readonly ICompletedBatchRepository _completedBatchRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<IntegrationEventProvider> _logger;

    public IntegrationEventProvider(
        IEnergyResultEventProvider energyResultEventProvider,
        IWholesaleResultEventProvider wholesaleResultEventProvider,
        ICompletedBatchRepository completedBatchRepository,
        IClock clock,
        IUnitOfWork unitOfWork,
        ILogger<IntegrationEventProvider> logger)
    {
        _energyResultEventProvider = energyResultEventProvider;
        _wholesaleResultEventProvider = wholesaleResultEventProvider;
        _completedBatchRepository = completedBatchRepository;
        _clock = clock;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async IAsyncEnumerable<IntegrationEvent> GetAsync()
    {
        do
        {
            var unpublishedBatch = await _completedBatchRepository.GetNextUnpublishedOrNullAsync().ConfigureAwait(false);
            if (unpublishedBatch == null)
            {
                break;
            }

            // Publish integration events for energy results
            var energyResultCount = 0;
            await foreach (var integrationEvent in _energyResultEventProvider.GetAsync(unpublishedBatch).ConfigureAwait(false))
            {
                energyResultCount++;
                yield return integrationEvent;
            }

            // Publish integration events for wholesale results
            var wholesaleResultCount = 0;
            if (_wholesaleResultEventProvider.CanContainWholesaleResults(unpublishedBatch))
            {
                await foreach (var integrationEvent in _wholesaleResultEventProvider.GetAsync(unpublishedBatch).ConfigureAwait(false))
                {
                    wholesaleResultCount++;
                    yield return integrationEvent;
                }
            }

            unpublishedBatch.PublishedTime = _clock.GetCurrentInstant();
            await _unitOfWork.CommitAsync().ConfigureAwait(false);

            _logger.LogInformation("Handled '{EnergyResultCount}' energy results for completed batch {BatchId}.", energyResultCount, unpublishedBatch.Id);
            if (_wholesaleResultEventProvider.CanContainWholesaleResults(unpublishedBatch))
            {
                _logger.LogInformation("Handled '{WholesaleResultCount}' wholesale results for completed batch {BatchId}.", wholesaleResultCount, unpublishedBatch.Id);
            }
        }
        while (true);
    }
}
