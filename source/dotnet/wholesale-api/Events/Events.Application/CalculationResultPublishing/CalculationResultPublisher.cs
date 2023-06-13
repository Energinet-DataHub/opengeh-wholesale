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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.Actors;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.Actors.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Events.Application.CalculationResultPublishing.Model;
using Energinet.DataHub.Wholesale.Events.Application.IntegrationEventsManagement;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Events.Application.CalculationResultPublishing;

public class CalculationResultPublisher : ICalculationResultPublisher
{
    private readonly ICalculationResultClient _calculationResultClient;
    private readonly IActorClient _actorClient;
    private readonly ICalculationResultCompletedFactory _calculationResultCompletedFactory;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ILogger<CalculationResultPublisher> _logger;

    public CalculationResultPublisher(
        ICalculationResultClient calculationResultClient,
        IActorClient actorClient,
        ICalculationResultCompletedFactory integrationEventFactory,
        IIntegrationEventPublisher integrationEventPublisher,
        ILogger<CalculationResultPublisher> logger)
    {
        _calculationResultClient = calculationResultClient;
        _actorClient = actorClient;
        _calculationResultCompletedFactory = integrationEventFactory;
        _integrationEventPublisher = integrationEventPublisher;
        _logger = logger;
    }

    public async Task PublishForGridAreaAsync(BatchGridAreaInfo batchGridAreaInfo)
    {
        // Publish events for total grid area
        await PublishForGridAccessProviderAsync(batchGridAreaInfo, TimeSeriesType.NonProfiledConsumption).ConfigureAwait(false);
        await PublishForGridAccessProviderAsync(batchGridAreaInfo, TimeSeriesType.FlexConsumption).ConfigureAwait(false);
        await PublishForGridAccessProviderAsync(batchGridAreaInfo, TimeSeriesType.Production).ConfigureAwait(false);
        await PublishForGridAccessProviderAsync(batchGridAreaInfo, TimeSeriesType.NetExchangePerGa).ConfigureAwait(false);
        await PublishForGridAccessProviderAsync(batchGridAreaInfo, TimeSeriesType.NetExchangePerNeighboringGa).ConfigureAwait(false);

        // Publish events for energy suppliers
        await PublishForEnergySuppliersAsync(batchGridAreaInfo, TimeSeriesType.NonProfiledConsumption).ConfigureAwait(false);
        await PublishForEnergySuppliersAsync(batchGridAreaInfo, TimeSeriesType.FlexConsumption).ConfigureAwait(false);
        await PublishForEnergySuppliersAsync(batchGridAreaInfo, TimeSeriesType.Production).ConfigureAwait(false);

        // Publish events for balance responsible party
        await PublishForBalanceResponsiblePartiesAsync(batchGridAreaInfo, TimeSeriesType.NonProfiledConsumption).ConfigureAwait(false);
        await PublishForBalanceResponsiblePartiesAsync(batchGridAreaInfo, TimeSeriesType.FlexConsumption).ConfigureAwait(false);
        await PublishForBalanceResponsiblePartiesAsync(batchGridAreaInfo, TimeSeriesType.Production).ConfigureAwait(false);

        // Publish events for energy suppliers results for balance responsible parties
        await PublishForEnergySupplierBalanceResponsiblePartiesAsync(batchGridAreaInfo, TimeSeriesType.NonProfiledConsumption).ConfigureAwait(false);
        await PublishForEnergySupplierBalanceResponsiblePartiesAsync(batchGridAreaInfo, TimeSeriesType.FlexConsumption).ConfigureAwait(false);
        await PublishForEnergySupplierBalanceResponsiblePartiesAsync(batchGridAreaInfo, TimeSeriesType.Production).ConfigureAwait(false);
    }

    private async Task PublishForEnergySupplierBalanceResponsiblePartiesAsync(BatchGridAreaInfo batchGridAreaInfo, TimeSeriesType timeSeriesType)
    {
        Actor[] brps;
        try
        {
            brps = await _actorClient
                .GetBalanceResponsiblePartiesAsync(
                    batchGridAreaInfo.BatchId,
                    batchGridAreaInfo.GridAreaCode,
                    timeSeriesType).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to get balance responsible parties with time series type {timeSeriesType} on batch {batchGridAreaInfo.BatchId}");
            return;
        }

        foreach (var brp in brps)
        {
            var energySuppliersByBalanceResponsibleParty = await _actorClient
                .GetEnergySuppliersByBalanceResponsiblePartyAsync(
                    batchGridAreaInfo.BatchId,
                    batchGridAreaInfo.GridAreaCode,
                    timeSeriesType,
                    brp.Gln).ConfigureAwait(false);

            foreach (var energySupplier in energySuppliersByBalanceResponsibleParty)
            {
                var result = await _calculationResultClient.GetAsync(
                        batchGridAreaInfo.BatchId,
                        batchGridAreaInfo.GridAreaCode,
                        timeSeriesType,
                        energySupplier.Gln,
                        brp.Gln)
                    .ConfigureAwait(false);

                var integrationEvent = _calculationResultCompletedFactory.CreateForEnergySupplierByBalanceResponsibleParty(result, batchGridAreaInfo, energySupplier.Gln, brp.Gln);
                await _integrationEventPublisher.PublishAsync(integrationEvent).ConfigureAwait(false);
            }
        }
    }

    private async Task PublishForGridAccessProviderAsync(BatchGridAreaInfo batchGridAreaInfo, TimeSeriesType timeSeriesType)
    {
        CalculationResult productionForTotalGa;
        try
        {
            productionForTotalGa = await _calculationResultClient
                .GetAsync(
                    batchGridAreaInfo.BatchId,
                    batchGridAreaInfo.GridAreaCode,
                    timeSeriesType,
                    null,
                    null)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to get calculation result for total grid area with time series type {timeSeriesType} on batch {batchGridAreaInfo.BatchId}");
            return;
        }

        var integrationEvent = _calculationResultCompletedFactory.CreateForTotalGridArea(productionForTotalGa, batchGridAreaInfo);
        await _integrationEventPublisher.PublishAsync(integrationEvent).ConfigureAwait(false);
    }

    private async Task PublishForEnergySuppliersAsync(BatchGridAreaInfo batchGridAreaInfo, TimeSeriesType timeSeriesType)
    {
        Actor[] energySuppliers;
        try
        {
            energySuppliers = await _actorClient.GetEnergySuppliersAsync(
                batchGridAreaInfo.BatchId,
                batchGridAreaInfo.GridAreaCode,
                timeSeriesType).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to get energy suppliers for grid area {batchGridAreaInfo.GridAreaCode} with time series type {timeSeriesType} on batch {batchGridAreaInfo.BatchId}");
            return;
        }

        foreach (var energySupplier in energySuppliers)
        {
            var result = await _calculationResultClient
                .GetAsync(
                    batchGridAreaInfo.BatchId,
                    batchGridAreaInfo.GridAreaCode,
                    timeSeriesType,
                    energySupplier.Gln,
                    null)
                .ConfigureAwait(false);

            var integrationEvent = _calculationResultCompletedFactory.CreateForEnergySupplier(result, batchGridAreaInfo, energySupplier.Gln);
            await _integrationEventPublisher.PublishAsync(integrationEvent).ConfigureAwait(false);
        }
    }

    private async Task PublishForBalanceResponsiblePartiesAsync(BatchGridAreaInfo batchGridAreaInfo, TimeSeriesType timeSeriesType)
    {
        Actor[] balanceResponsibleParties;
        try
        {
            balanceResponsibleParties = await _actorClient.GetBalanceResponsiblePartiesAsync(
                batchGridAreaInfo.BatchId,
                batchGridAreaInfo.GridAreaCode,
                timeSeriesType).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to get balance responsible parties for grid area {batchGridAreaInfo.GridAreaCode} with time series type {timeSeriesType} on batch {batchGridAreaInfo.BatchId}");
            return;
        }

        foreach (var balanceResponsibleParty in balanceResponsibleParties)
        {
            var result = await _calculationResultClient
                .GetAsync(
                    batchGridAreaInfo.BatchId,
                    batchGridAreaInfo.GridAreaCode,
                    timeSeriesType,
                    null,
                    balanceResponsibleParty.Gln)
                .ConfigureAwait(false);

            var integrationEvent = _calculationResultCompletedFactory.CreateForBalanceResponsibleParty(result, batchGridAreaInfo, balanceResponsibleParty.Gln);
            await _integrationEventPublisher.PublishAsync(integrationEvent).ConfigureAwait(false);
        }
    }
}
