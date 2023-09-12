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

using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Types;
using Google.Protobuf.WellKnownTypes;
using EnergyResult = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResult;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;

public class EnergyResultFactory : IEnergyResultFactory
{
    public Contracts.Events.EnergyResult Create(EnergyResult energyResult)
    {
        if (energyResult.EnergySupplierId == null && energyResult.BalanceResponsibleId == null)
            return CreateForGridArea(energyResult);

        if (energyResult.EnergySupplierId != null && energyResult.BalanceResponsibleId == null)
            return CreateForEnergySupplier(energyResult);

        if (energyResult.EnergySupplierId == null && energyResult.BalanceResponsibleId != null)
            return CreateForBalanceResponsibleParty(energyResult);

        return CreateForEnergySupplierByBalanceResponsibleParty(energyResult);
    }

    private static Contracts.Events.EnergyResult CreateForGridArea(EnergyResult result)
    {
        var calculationResultCompleted = CreateInternal(result);
        calculationResultCompleted.AggregationPerGridarea = new Contracts.Events.AggregationPerGridArea
        {
            GridAreaCode = result.GridArea,
        };

        return calculationResultCompleted;
    }

    private static Contracts.Events.EnergyResult CreateForEnergySupplier(
        EnergyResult result)
    {
        var calculationResultCompleted = CreateInternal(result);
        calculationResultCompleted.AggregationPerEnergysupplierPerGridarea = new Contracts.Events.AggregationPerEnergySupplierPerGridArea
        {
            GridAreaCode = result.GridArea,
            EnergySupplierId = result.EnergySupplierId,
        };

        return calculationResultCompleted;
    }

    private static Contracts.Events.EnergyResult CreateForBalanceResponsibleParty(
        EnergyResult result)
    {
        var energyResult = CreateInternal(result);
        energyResult.AggregationPerBalanceresponsiblepartyPerGridarea =
            new Contracts.Events.AggregationPerBalanceResponsiblePartyPerGridArea
            {
                GridAreaCode = result.GridArea,
                BalanceResponsibleId = result.BalanceResponsibleId,
            };

        return energyResult;
    }

    private Contracts.Events.EnergyResult CreateForEnergySupplierByBalanceResponsibleParty(
        EnergyResult result)
    {
        var calculationResultCompleted = CreateInternal(result);
        calculationResultCompleted.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea =
            new Contracts.Events.AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea
            {
                GridAreaCode = result.GridArea,
                EnergySupplierId = result.EnergySupplierId,
                BalanceResponsibleId = result.BalanceResponsibleId,
            };

        return calculationResultCompleted;
    }

    private static Contracts.Events.EnergyResult CreateInternal(EnergyResult result)
    {
        var energyResult = new Contracts.Events.EnergyResult
        {
            BatchId = result.BatchId.ToString(),
            Resolution = Contracts.Events.Resolution.Quarter,
            ProcessType = ProcessTypeMapper.MapProcessType(result.ProcessType),
            QuantityUnit = Contracts.Events.QuantityUnit.Kwh,
            PeriodStartUtc = result.PeriodStart.ToTimestamp(),
            PeriodEndUtc = result.PeriodEnd.ToTimestamp(),
            TimeSeriesType = TimeSeriesTypeMapper.MapTimeSeriesType(result.TimeSeriesType),
        };
        if (result.FromGridArea != null)
            energyResult.FromGridAreaCode = result.FromGridArea;

        energyResult.TimeSeriesPoints
            .AddRange(result.TimeSeriesPoints
                .Select(timeSeriesPoint => new Contracts.Events.TimeSeriesPoint
                {
                    Quantity = new Contracts.Events.DecimalValue(timeSeriesPoint.Quantity),
                    Time = timeSeriesPoint.Time.ToTimestamp(),
                    QuantityQuality = QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality),
                }));
        return energyResult;
    }
}
