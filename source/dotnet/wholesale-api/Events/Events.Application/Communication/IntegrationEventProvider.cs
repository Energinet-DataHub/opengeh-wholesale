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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Models;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.Application.Communication;

public class IntegrationEventProvider : IIntegrationEventProvider
{
    private readonly IIntegrationEventFactory _integrationEventFactory;
    private readonly IEnergyResultQueries _energyResultQueries;
    private readonly IWholesaleResultQueries _wholesaleResultQueries;
    private readonly ICompletedBatchRepository _completedBatchRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<IntegrationEventProvider> _logger;

    public IntegrationEventProvider(
        IIntegrationEventFactory integrationEventFactory,
        IEnergyResultQueries energyResultQueries,
        IWholesaleResultQueries wholesaleResultQueries,
        ICompletedBatchRepository completedBatchRepository,
        IClock clock,
        IUnitOfWork unitOfWork,
        ILogger<IntegrationEventProvider> logger)
    {
        _integrationEventFactory = integrationEventFactory;
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
            var energyEventProviderState = new EventProviderState();
            await foreach (var integrationEvent in GetEnergyResultsAsync(batch.Id, energyEventProviderState).ConfigureAwait(false))
            {
                yield return integrationEvent;
            }

            // Publish wholesale results
            var wholesaleEventProviderState = new EventProviderState();
            if (IsWholesaleCalculationType(batch.ProcessType))
            {
                await foreach (var integrationEvent in GetWholesaleResultsAsync(batch.Id, wholesaleEventProviderState).ConfigureAwait(false))
                {
                    yield return integrationEvent;
                }
            }

            batch.PublishedTime = _clock.GetCurrentInstant();
            await _unitOfWork.CommitAsync().ConfigureAwait(false);

            _logger.LogInformation("Published {EnergyResultCount} energy results for completed batch {BatchId}", energyEventProviderState.EventCount, batch.Id);
            if (IsWholesaleCalculationType(batch.ProcessType))
            {
                _logger.LogInformation("Published {WholesaleResultCount} wholesale results for completed batch {BatchId}", wholesaleEventProviderState.EventCount, batch.Id);
            }
        }
        while (true);
    }

    private async IAsyncEnumerable<IntegrationEvent> GetEnergyResultsAsync(Guid batchId, EventProviderState state)
    {
        await foreach (var energyResult in _energyResultQueries.GetAsync(batchId).ConfigureAwait(false))
        {
            state.EventCount++;
            yield return _integrationEventFactory.CreateCalculationResultCompleted(energyResult); // Deprecated
            yield return _integrationEventFactory.CreateEnergyResultProducedV1(energyResult);
        }
    }

    private async IAsyncEnumerable<IntegrationEvent> GetWholesaleResultsAsync(Guid batchId, EventProviderState state)
    {
        await foreach (var wholesaleResult in _wholesaleResultQueries.GetAsync(batchId).ConfigureAwait(false))
        {
            state.EventCount++;
            yield return CreateEventFromWholesaleResult(wholesaleResult);
        }
    }

    private IntegrationEvent CreateEventFromWholesaleResult(WholesaleResult wholesaleResult)
    {
        return wholesaleResult.ChargeResolution switch
        {
            ChargeResolution.Day or ChargeResolution.Hour => _integrationEventFactory
                .CreateAmountPerChargeResultProducedV1(wholesaleResult),
            ChargeResolution.Month => _integrationEventFactory
                .CreateMonthlyAmountPerChargeResultProducedV1(wholesaleResult),
            _ => throw new ArgumentOutOfRangeException(
                nameof(wholesaleResult.ChargeResolution),
                actualValue: wholesaleResult.ChargeResolution,
                "Unexpected resolution."),
        };
    }

    private static bool IsWholesaleCalculationType(ProcessType calculationType)
    {
        return calculationType is ProcessType.WholesaleFixing or ProcessType.FirstCorrectionSettlement or ProcessType.SecondCorrectionSettlement or ProcessType.ThirdCorrectionSettlement;
    }
}
