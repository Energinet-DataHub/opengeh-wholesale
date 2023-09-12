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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Types;
using Google.Protobuf.WellKnownTypes;
using TimeSeriesPoint = Energinet.DataHub.Wholesale.Contracts.Events.TimeSeriesPoint;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;

public class CalculationResultCompletedFactory : ICalculationResultCompletedFactory
{
    public Contracts.Events.CalculationResultCompleted Create(EnergyResult energyResult)
    {
        if (energyResult.EnergySupplierId == null && energyResult.BalanceResponsibleId == null)
            return CreateForGridArea(energyResult);

        if (energyResult.EnergySupplierId != null && energyResult.BalanceResponsibleId == null)
            return CreateForEnergySupplier(energyResult);

        if (energyResult.EnergySupplierId == null && energyResult.BalanceResponsibleId != null)
            return CreateForBalanceResponsibleParty(energyResult);

        return CreateForEnergySupplierByBalanceResponsibleParty(energyResult);
    }

    private Contracts.Events.CalculationResultCompleted CreateForGridArea(EnergyResult result)
    {
        var calculationResultCompleted = CreateInternal(result);
        calculationResultCompleted.AggregationPerGridarea = new Contracts.Events.AggregationPerGridArea
        {
            GridAreaCode = result.GridArea,
        };

        return calculationResultCompleted;
    }

    private Contracts.Events.CalculationResultCompleted CreateForEnergySupplier(
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

    private Contracts.Events.CalculationResultCompleted CreateForBalanceResponsibleParty(
        EnergyResult result)
    {
        var calculationResultCompleted = CreateInternal(result);
        calculationResultCompleted.AggregationPerBalanceresponsiblepartyPerGridarea =
            new Contracts.Events.AggregationPerBalanceResponsiblePartyPerGridArea
            {
                GridAreaCode = result.GridArea,
                BalanceResponsibleId = result.BalanceResponsibleId,
            };

        return calculationResultCompleted;
    }

    private Contracts.Events.CalculationResultCompleted CreateForEnergySupplierByBalanceResponsibleParty(
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

    private static Contracts.Events.CalculationResultCompleted CreateInternal(EnergyResult result)
    {
        var calculationResultCompleted = new Contracts.Events.CalculationResultCompleted
        {
            BatchId = result.BatchId.ToString(),
            Resolution = Contracts.Events.Resolution.Quarter,
            ProcessType = ProcessTypeMapper.MapProcessTypeDeprecated(result.ProcessType),
            QuantityUnit = Contracts.Events.QuantityUnit.Kwh,
            PeriodStartUtc = result.PeriodStart.ToTimestamp(),
            PeriodEndUtc = result.PeriodEnd.ToTimestamp(),
            TimeSeriesType = TimeSeriesTypeMapper.MapTimeSeriesTypeDeprecated(result.TimeSeriesType),
        };
        if (result.FromGridArea != null)
            calculationResultCompleted.FromGridAreaCode = result.FromGridArea;

        calculationResultCompleted.TimeSeriesPoints
            .AddRange(result.TimeSeriesPoints
                .Select(timeSeriesPoint => new TimeSeriesPoint
                {
                    Quantity = new Contracts.Events.DecimalValue(timeSeriesPoint.Quantity),
                    Time = timeSeriesPoint.Time.ToTimestamp(),
                    QuantityQuality = QuantityQualityMapper.MapQuantityQualityDeprecated(timeSeriesPoint.Quality),
                }));
        return calculationResultCompleted;
    }
}
