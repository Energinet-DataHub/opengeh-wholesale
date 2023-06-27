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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.Application.CalculationResultPublishing;

public class OutboxRepository : IOutboxRepository
{
    private readonly ICalculationResultIntegrationEventFactory _calculationResultIntegrationEventFactory;
    private readonly ICalculationResultQueries _calculationResultQueries;
    private readonly ICompletedBatchRepository _completedBatchRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public OutboxRepository(
        ICalculationResultIntegrationEventFactory integrationEventFactory,
        ICalculationResultQueries calculationResultQueries,
        ICompletedBatchRepository completedBatchRepository,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _calculationResultIntegrationEventFactory = integrationEventFactory;
        _calculationResultQueries = calculationResultQueries;
        _completedBatchRepository = completedBatchRepository;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async IAsyncEnumerable<List<IntegrationEvent>> GetAsync()
    {
        do
        {
            var batch = await _completedBatchRepository.GetNextUnpublishedOrNullAsync().ConfigureAwait(false);
            if (batch == null) break;

            // TODO: This approach will bring all calculation results into memory. That's bad.
            var integrationEvents = await CreateIntegrationEventsAsync(batch).ConfigureAwait(false);
            yield return integrationEvents;

            batch.PublishedTime = _clock.GetCurrentInstant();
            await _unitOfWork.CommitAsync().ConfigureAwait(false);
        }
        while (true);
    }

    public async Task<List<IntegrationEvent>> CreateIntegrationEventsAsync(CompletedBatch batch)
    {
        var events = new List<IntegrationEvent>();

        await foreach (var calculationResult in _calculationResultQueries.GetAsync(batch.Id))
        {
            var integrationEvent = CreateIntegrationEvent(calculationResult);
            events.Add(integrationEvent);
        }

        return events;
    }

    private IntegrationEvent CreateIntegrationEvent(CalculationResult calculationResult)
    {
        if (calculationResult.EnergySupplierId == null && calculationResult.BalanceResponsibleId == null)
        {
            return _calculationResultIntegrationEventFactory.CreateForTotalGridArea(calculationResult);
        }

        if (calculationResult.EnergySupplierId != null && calculationResult.BalanceResponsibleId == null)
        {
            return _calculationResultIntegrationEventFactory.CreateForEnergySupplier(calculationResult);
        }

        if (calculationResult.EnergySupplierId == null && calculationResult.BalanceResponsibleId != null)
        {
            return
                _calculationResultIntegrationEventFactory.CreateForBalanceResponsibleParty(calculationResult);
        }

        return _calculationResultIntegrationEventFactory.CreateForEnergySupplierByBalanceResponsibleParty(calculationResult);
    }
}
