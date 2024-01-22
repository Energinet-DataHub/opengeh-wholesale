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
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.Common;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Common;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EnergyResultProducedV2.Mappers;
using Google.Protobuf.WellKnownTypes;
using AggregationPerBalanceResponsiblePartyPerGridArea = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV2.Types.AggregationPerBalanceResponsiblePartyPerGridArea;
using AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV2.Types.AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea;
using AggregationPerEnergySupplierPerGridArea = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV2.Types.AggregationPerEnergySupplierPerGridArea;
using AggregationPerGridArea = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV2.Types.AggregationPerGridArea;
using QuantityUnit = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV2.Types.QuantityUnit;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EnergyResultProducedV2.Factories;

public class EnergyResultProducedV2Factory : IEnergyResultProducedV2Factory
{
    private static readonly IReadOnlyCollection<TimeSeriesType> _supportedTimeSeriesTypes = new[]
    {
        TimeSeriesType.Production,
        TimeSeriesType.FlexConsumption,
        TimeSeriesType.NonProfiledConsumption,
        TimeSeriesType.TotalConsumption,
        TimeSeriesType.NetExchangePerGa,
        TimeSeriesType.NetExchangePerNeighboringGa,
    };

    public bool CanCreate(EnergyResult result) =>
        _supportedTimeSeriesTypes.Contains(result.TimeSeriesType);

    public Contracts.IntegrationEvents.EnergyResultProducedV2 Create(EnergyResult energyResult)
    {
        if (!CanCreate(energyResult))
            throw new ArgumentException($"Cannot create '{nameof(Contracts.IntegrationEvents.EnergyResultProducedV2)}' from energy result.", nameof(energyResult));

        if (energyResult.EnergySupplierId == null && energyResult.BalanceResponsibleId == null)
            return CreateForGridArea(energyResult);

        if (energyResult.EnergySupplierId != null && energyResult.BalanceResponsibleId == null)
            return CreateForEnergySupplier(energyResult);

        if (energyResult.EnergySupplierId == null && energyResult.BalanceResponsibleId != null)
            return CreateForBalanceResponsibleParty(energyResult);

        return CreateForEnergySupplierByBalanceResponsibleParty(energyResult);
    }

    private Contracts.IntegrationEvents.EnergyResultProducedV2 CreateForGridArea(EnergyResult result)
    {
        var energyResultProducedV2 = CreateInternal(result);
        energyResultProducedV2.AggregationPerGridarea = new AggregationPerGridArea
        {
            GridAreaCode = result.GridArea,
        };

        return energyResultProducedV2;
    }

    private Contracts.IntegrationEvents.EnergyResultProducedV2 CreateForEnergySupplier(
        EnergyResult result)
    {
        var energyResultProduced = CreateInternal(result);
        energyResultProduced.AggregationPerEnergysupplierPerGridarea = new AggregationPerEnergySupplierPerGridArea
        {
            GridAreaCode = result.GridArea,
            EnergySupplierId = result.EnergySupplierId,
        };

        return energyResultProduced;
    }

    private Contracts.IntegrationEvents.EnergyResultProducedV2 CreateForBalanceResponsibleParty(
        EnergyResult result)
    {
        var energyResultProducedV2 = CreateInternal(result);
        energyResultProducedV2.AggregationPerBalanceresponsiblepartyPerGridarea =
            new AggregationPerBalanceResponsiblePartyPerGridArea
            {
                GridAreaCode = result.GridArea,
                BalanceResponsibleId = result.BalanceResponsibleId,
            };

        return energyResultProducedV2;
    }

    private Contracts.IntegrationEvents.EnergyResultProducedV2 CreateForEnergySupplierByBalanceResponsibleParty(
        EnergyResult result)
    {
        var energyResultProducedV2 = CreateInternal(result);
        energyResultProducedV2.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea =
            new AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea
            {
                GridAreaCode = result.GridArea,
                EnergySupplierId = result.EnergySupplierId,
                BalanceResponsibleId = result.BalanceResponsibleId,
            };

        return energyResultProducedV2;
    }

    private static Contracts.IntegrationEvents.EnergyResultProducedV2 CreateInternal(EnergyResult result)
    {
        var energyResultProduced = new Contracts.IntegrationEvents.EnergyResultProducedV2
        {
            CalculationId = result.BatchId.ToString(),
            Resolution = Contracts.IntegrationEvents.EnergyResultProducedV2.Types.Resolution.Quarter,
            CalculationType = CalculationTypeMapper.MapCalculationType(result.ProcessType),
            QuantityUnit = QuantityUnit.Kwh,
            PeriodStartUtc = result.PeriodStart.ToTimestamp(),
            PeriodEndUtc = result.PeriodEnd.ToTimestamp(),
            TimeSeriesType = TimeSeriesTypeMapper.MapTimeSeriesType(result.TimeSeriesType),
            CalculationResultVersion = result.Version,
        };
        if (result.FromGridArea != null)
            energyResultProduced.FromGridAreaCode = result.FromGridArea;

        energyResultProduced.TimeSeriesPoints
            .AddRange(result.TimeSeriesPoints
                .Select(timeSeriesPoint =>
                {
                    var mappedQuantityQualities = timeSeriesPoint.Qualities.Select(QuantityQualityMapper.MapQuantityQuality);
                    var mappedTimeSeriesPoint = new Contracts.IntegrationEvents.EnergyResultProducedV2.Types.TimeSeriesPoint
                    {
                        Quantity = new DecimalValue(timeSeriesPoint.Quantity),
                        Time = timeSeriesPoint.Time.ToTimestamp(),
                    };
                    mappedTimeSeriesPoint.QuantityQualities.AddRange(mappedQuantityQualities);
                    return mappedTimeSeriesPoint;
                }));
        return energyResultProduced;
    }
}
