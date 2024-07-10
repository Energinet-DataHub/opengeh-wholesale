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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EnergyResultProducedV2.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.GridLossResultProducedV1.Factories;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EventProviders;

public class EnergyResultEventProvider(
    IEnergyResultQueries energyResultQueries,
    IEnergyResultProducedV2Factory energyResultProducedV2Factory,
    IGridLossResultProducedV1Factory gridLossResultProducedV2Factory)
    : ResultEventProvider, IEnergyResultEventProvider
{
    public async IAsyncEnumerable<IntegrationEvent> GetAsync(CompletedCalculation calculation)
    {
        await foreach (var energyResult in energyResultQueries.GetAsync(calculation.Id).ConfigureAwait(false))
        {
            if (energyResultProducedV2Factory.CanCreate(energyResult))
                yield return CreateIntegrationEvent(energyResultProducedV2Factory.Create(energyResult));

            // This is to trigger CreateForEnergySupplier in factory. We have removed the calculation for es_ga,
            // so we are using energy_supplier instead and need to trigger the correct factory method. Events will be completely remove by Mosaic.
            if (energyResultProducedV2Factory.CanCreate(energyResult) && energyResult.EnergySupplierId is not null &&
                energyResult.BalanceResponsibleId is not null)
            {
                var energyResultSetBalanceResponsibleToNull = new EnergyResult(
                    energyResult.Id,
                    energyResult.CalculationId,
                    energyResult.GridArea,
                    energyResult.TimeSeriesType,
                    energyResult.EnergySupplierId,
                    null,
                    energyResult.TimeSeriesPoints,
                    energyResult.CalculationType,
                    energyResult.PeriodStart,
                    energyResult.PeriodEnd,
                    energyResult.FromGridArea,
                    energyResult.MeteringPointId,
                    energyResult.Resolution,
                    energyResult.Version);

                yield return CreateIntegrationEvent(energyResultProducedV2Factory.Create(energyResultSetBalanceResponsibleToNull));
            }

            if (gridLossResultProducedV2Factory.CanCreate(energyResult))
                yield return CreateIntegrationEvent(gridLossResultProducedV2Factory.Create(energyResult));
        }
    }
}
