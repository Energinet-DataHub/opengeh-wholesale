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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using Energinet.DataHub.Wholesale.Events.Application.IntegrationEventsManagement;

namespace Energinet.DataHub.Wholesale.Events.Application.CalculationResultPublishing;

public class CalculationResultPublisher : ICalculationResultPublisher
{
    private readonly ICalculationResultCompletedFactory _calculationResultCompletedFactory;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ICalculationResultQueries _calculationResultQueries;

    public CalculationResultPublisher(
        ICalculationResultCompletedFactory integrationEventFactory,
        IIntegrationEventPublisher integrationEventPublisher,
        ICalculationResultQueries calculationResultQueries)
    {
        _calculationResultCompletedFactory = integrationEventFactory;
        _integrationEventPublisher = integrationEventPublisher;
        _calculationResultQueries = calculationResultQueries;
    }

    public async Task PublishForBatchAsync(CompletedBatch batch)
    {
        await foreach (var calculationResult in _calculationResultQueries.GetAsync(batch.Id))
        {
            if (calculationResult.EnergySupplierId == null && calculationResult.BalanceResponsibleId == null)
            {
                await PublishForGridAccessProviderAsync(calculationResult).ConfigureAwait(false);
            }
            else if (calculationResult.EnergySupplierId != null && calculationResult.BalanceResponsibleId == null)
            {
                await PublishForEnergySuppliersAsync(calculationResult).ConfigureAwait(false);
            }
            else if (calculationResult.EnergySupplierId == null && calculationResult.BalanceResponsibleId != null)
            {
                await PublishForBalanceResponsiblePartiesAsync(calculationResult).ConfigureAwait(false);
            }
            else
            {
                await PublishForEnergySupplierBalanceResponsiblePartiesAsync(calculationResult).ConfigureAwait(false);
            }
        }
    }

    private async Task PublishForEnergySupplierBalanceResponsiblePartiesAsync(CalculationResult result)
    {
        var integrationEvent =
            _calculationResultCompletedFactory.CreateForEnergySupplierByBalanceResponsibleParty(
                result);
        await _integrationEventPublisher.PublishAsync(integrationEvent).ConfigureAwait(false);
    }

    private async Task PublishForGridAccessProviderAsync(CalculationResult result)
    {
        var integrationEvent = _calculationResultCompletedFactory.CreateForTotalGridArea(result);
        await _integrationEventPublisher.PublishAsync(integrationEvent).ConfigureAwait(false);
    }

    private async Task PublishForEnergySuppliersAsync(CalculationResult result)
    {
        var integrationEvent = _calculationResultCompletedFactory.CreateForEnergySupplier(result);
        await _integrationEventPublisher.PublishAsync(integrationEvent).ConfigureAwait(false);
    }

    private async Task PublishForBalanceResponsiblePartiesAsync(CalculationResult result)
    {
        var integrationEvent = _calculationResultCompletedFactory.CreateForBalanceResponsibleParty(result);
        await _integrationEventPublisher.PublishAsync(integrationEvent).ConfigureAwait(false);
    }
}
