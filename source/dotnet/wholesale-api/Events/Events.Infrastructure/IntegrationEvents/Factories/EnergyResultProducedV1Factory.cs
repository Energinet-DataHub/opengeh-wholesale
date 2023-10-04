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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers.EnergyResultProducedV1;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Types;
using Google.Protobuf.WellKnownTypes;
using AggregationPerBalanceResponsiblePartyPerGridArea = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV1.Types.AggregationPerBalanceResponsiblePartyPerGridArea;
using AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV1.Types.AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea;
using AggregationPerEnergySupplierPerGridArea = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV1.Types.AggregationPerEnergySupplierPerGridArea;
using AggregationPerGridArea = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV1.Types.AggregationPerGridArea;
using QuantityUnit = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV1.Types.QuantityUnit;
using TimeSeriesPoint = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV1.Types.TimeSeriesPoint;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;

public class EnergyResultProducedV1Factory : IEnergyResultProducedFactory
{
    public EnergyResultProducedV1 Create(EnergyResult energyResult)
    {
        if (energyResult.EnergySupplierId == null && energyResult.BalanceResponsibleId == null)
            return CreateForGridArea(energyResult);

        if (energyResult.EnergySupplierId != null && energyResult.BalanceResponsibleId == null)
            return CreateForEnergySupplier(energyResult);

        if (energyResult.EnergySupplierId == null && energyResult.BalanceResponsibleId != null)
            return CreateForBalanceResponsibleParty(energyResult);

        return CreateForEnergySupplierByBalanceResponsibleParty(energyResult);
    }

    private EnergyResultProducedV1 CreateForGridArea(EnergyResult result)
    {
        var calculationResultCompleted = CreateInternal(result);
        calculationResultCompleted.AggregationPerGridarea = new AggregationPerGridArea
        {
            GridAreaCode = result.GridArea,
        };

        return calculationResultCompleted;
    }

    private EnergyResultProducedV1 CreateForEnergySupplier(
        EnergyResult result)
    {
        var calculationResultCompleted = CreateInternal(result);
        calculationResultCompleted.AggregationPerEnergysupplierPerGridarea = new AggregationPerEnergySupplierPerGridArea
        {
            GridAreaCode = result.GridArea,
            EnergySupplierId = result.EnergySupplierId,
        };

        return calculationResultCompleted;
    }

    private EnergyResultProducedV1 CreateForBalanceResponsibleParty(
        EnergyResult result)
    {
        var calculationResultCompleted = CreateInternal(result);
        calculationResultCompleted.AggregationPerBalanceresponsiblepartyPerGridarea =
            new AggregationPerBalanceResponsiblePartyPerGridArea
            {
                GridAreaCode = result.GridArea,
                BalanceResponsibleId = result.BalanceResponsibleId,
            };

        return calculationResultCompleted;
    }

    private EnergyResultProducedV1 CreateForEnergySupplierByBalanceResponsibleParty(
        EnergyResult result)
    {
        var calculationResultCompleted = CreateInternal(result);
        calculationResultCompleted.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea =
            new AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea
            {
                GridAreaCode = result.GridArea,
                EnergySupplierId = result.EnergySupplierId,
                BalanceResponsibleId = result.BalanceResponsibleId,
            };

        return calculationResultCompleted;
    }

    private static EnergyResultProducedV1 CreateInternal(EnergyResult result)
    {
        var calculationResultCompleted = new EnergyResultProducedV1
        {
            CalculationId = result.BatchId.ToString(),
            Resolution = EnergyResultProducedV1.Types.Resolution.Quarter,
            ProcessType = ProcessTypeMapper.MapProcessType(result.ProcessType),
            QuantityUnit = QuantityUnit.Kwh,
            PeriodStartUtc = result.PeriodStart.ToTimestamp(),
            PeriodEndUtc = result.PeriodEnd.ToTimestamp(),
            TimeSeriesType = TimeSeriesTypeMapper.MapTimeSeriesType(result.TimeSeriesType),
        };
        if (result.FromGridArea != null)
            calculationResultCompleted.FromGridAreaCode = result.FromGridArea;

        calculationResultCompleted.TimeSeriesPoints
            .AddRange(result.TimeSeriesPoints
                .Select(timeSeriesPoint => new TimeSeriesPoint()
                {
                    Quantity = new DecimalValue(timeSeriesPoint.Quantity),
                    Time = timeSeriesPoint.Time.ToTimestamp(),
                    QuantityQuality = QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality),
                }));
        return calculationResultCompleted;
    }
}
