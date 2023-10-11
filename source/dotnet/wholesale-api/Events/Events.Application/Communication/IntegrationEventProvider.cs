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
using Energinet.DataHub.Wholesale.Common.Models;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.Application.Communication;

public class IntegrationEventProvider : IIntegrationEventProvider
{
    private readonly ICalculationResultIntegrationEventFactory _calculationResultIntegrationEventFactory;
    private readonly IEnergyResultQueries _energyResultQueries;
    private readonly IWholesaleResultQueries _wholesaleResultQueries;
    private readonly ICompletedBatchRepository _completedBatchRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<IntegrationEventProvider> _logger;

    public IntegrationEventProvider(
        ICalculationResultIntegrationEventFactory integrationEventFactory,
        IEnergyResultQueries energyResultQueries,
        IWholesaleResultQueries wholesaleResultQueries,
        ICompletedBatchRepository completedBatchRepository,
        IClock clock,
        IUnitOfWork unitOfWork,
        ILogger<IntegrationEventProvider> logger)
    {
        _calculationResultIntegrationEventFactory = integrationEventFactory;
        _energyResultQueries = energyResultQueries;
        _wholesaleResultQueries = wholesaleResultQueries;
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

            // Publish energy results
            var energyResultCount = 0;
            await foreach (var energyResult in _energyResultQueries.GetAsync(batch.Id).ConfigureAwait(false))
            {
                energyResultCount++;
                yield return _calculationResultIntegrationEventFactory.CreateEventForEnergyResultDeprecated(
                    energyResult); // Deprecated
                yield return _calculationResultIntegrationEventFactory.CreateEventForEnergyResult(energyResult);
            }

            // Publish wholesale results
            var wholesaleResultCount = 0;
            if (IsWholesaleCalculationType(batch.ProcessType))
            {
                await foreach (var wholesaleResult in _wholesaleResultQueries.GetAsync(batch.Id).ConfigureAwait(false))
                {
                    wholesaleResultCount++;
                    yield return _calculationResultIntegrationEventFactory.CreateEventForWholesaleResult(wholesaleResult);
                }
            }

            batch.PublishedTime = _clock.GetCurrentInstant();
            await _unitOfWork.CommitAsync().ConfigureAwait(false);

            _logger.LogInformation("Published {EnergyResultCount} energy results for completed batch {BatchId}", energyResultCount, batch.Id);
            if (IsWholesaleCalculationType(batch.ProcessType))
            {
                _logger.LogInformation("Published {WholesaleResultCount} results for completed batch {BatchId}", wholesaleResultCount, batch.Id);
            }
        }
        while (true);
    }

    private static bool IsWholesaleCalculationType(ProcessType calculationType)
    {
        return calculationType is ProcessType.WholesaleFixing or ProcessType.FirstCorrectionSettlement or ProcessType.SecondCorrectionSettlement or ProcessType.ThirdCorrectionSettlement;
    }
}
