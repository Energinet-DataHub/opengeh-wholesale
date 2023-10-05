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
using Energinet.DataHub.Core.Messaging.Communication.Internal;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.Application.Communication;

public class IntegrationEventProvider : IIntegrationEventProvider
{
    private readonly ICalculationResultIntegrationEventFactory _calculationResultIntegrationEventFactory;
    private readonly ICalculationResultQueries _calculationResultQueries;
    private readonly ICompletedBatchRepository _completedBatchRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<IntegrationEventProvider> _logger;

    public IntegrationEventProvider(
        ICalculationResultIntegrationEventFactory integrationEventFactory,
        ICalculationResultQueries calculationResultQueries,
        ICompletedBatchRepository completedBatchRepository,
        IClock clock,
        IUnitOfWork unitOfWork,
        ILogger<IntegrationEventProvider> logger)
    {
        _calculationResultIntegrationEventFactory = integrationEventFactory;
        _calculationResultQueries = calculationResultQueries;
        _completedBatchRepository = completedBatchRepository;
        _clock = clock;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async IAsyncEnumerable<IntegrationEvent> GetAsync()
    {
        do
        {
            var batch = await _completedBatchRepository.GetNextUnpublishedOrNullAsync().ConfigureAwait(false);
            if (batch == null)
            {
                break;
            }

            var resultCount = 0;
            await foreach (var energyResult in _calculationResultQueries.GetAsync(batch.Id).ConfigureAwait(false))
            {
                resultCount++;
                yield return _calculationResultIntegrationEventFactory.CreateCalculationResultCompleted(energyResult); // Deprecated
                yield return _calculationResultIntegrationEventFactory.CreateEnergyResultProducedV1(energyResult);
            }

            batch.PublishedTime = _clock.GetCurrentInstant();
            await _unitOfWork.CommitAsync().ConfigureAwait(false);

            _logger.LogInformation("Published {ResultCount} results for completed batch {BatchId}", resultCount, batch.Id);
        }
        while (true);
    }
}
