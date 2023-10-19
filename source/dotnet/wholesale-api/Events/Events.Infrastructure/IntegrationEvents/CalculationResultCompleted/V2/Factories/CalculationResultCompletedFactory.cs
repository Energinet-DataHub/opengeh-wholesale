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
using Energinet.DataHub.Wholesale.Contracts.Events.V2;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.Common;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.CalculationResultCompleted.V2.Mappers;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Common;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Common.Mappers;
using Google.Protobuf.WellKnownTypes;
using DecimalValue = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.Common.DecimalValue;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.CalculationResultCompleted.V2.Factories;

public class CalculationResultCompletedFactory : ICalculationResultCompletedFactory
{
    public Contracts.Events.V2.CalculationResultCompleted Create(EnergyResult energyResult)
    {
        if (energyResult.EnergySupplierId == null && energyResult.BalanceResponsibleId == null)
            return CreateForGridArea(energyResult);

        if (energyResult.EnergySupplierId != null && energyResult.BalanceResponsibleId == null)
            return CreateForEnergySupplier(energyResult);

        if (energyResult.EnergySupplierId == null && energyResult.BalanceResponsibleId != null)
            return CreateForBalanceResponsibleParty(energyResult);

        return CreateForEnergySupplierByBalanceResponsibleParty(energyResult);
    }

    private Contracts.Events.V2.CalculationResultCompleted CreateForGridArea(EnergyResult result)
    {
        var calculationResultCompleted = CreateInternal(result);
        calculationResultCompleted.AggregationPerGridarea = new AggregationPerGridArea
        {
            GridAreaCode = result.GridArea,
        };

        return calculationResultCompleted;
    }

    private Contracts.Events.V2.CalculationResultCompleted CreateForEnergySupplier(
        EnergyResult result)
    {
        var calculationResultCompleted = CreateInternal(result);
        calculationResultCompleted.AggregationPerEnergysupplierPerGridarea = new AggregationPerEnergySupplierPerGridArea
        {
            GridAreaCode = result.GridArea,
            EnergySupplierGlnOrEic = result.EnergySupplierId,
        };

        return calculationResultCompleted;
    }

    private Contracts.Events.V2.CalculationResultCompleted CreateForBalanceResponsibleParty(
        EnergyResult result)
    {
        var calculationResultCompleted = CreateInternal(result);
        calculationResultCompleted.AggregationPerBalanceresponsiblepartyPerGridarea =
            new AggregationPerBalanceResponsiblePartyPerGridArea
            {
                GridAreaCode = result.GridArea,
                BalanceResponsiblePartyGlnOrEic = result.BalanceResponsibleId,
            };

        return calculationResultCompleted;
    }

    private Contracts.Events.V2.CalculationResultCompleted CreateForEnergySupplierByBalanceResponsibleParty(
        EnergyResult result)
    {
        var calculationResultCompleted = CreateInternal(result);
        calculationResultCompleted.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea =
            new AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea
            {
                GridAreaCode = result.GridArea,
                EnergySupplierGlnOrEic = result.EnergySupplierId,
                BalanceResponsiblePartyGlnOrEic = result.BalanceResponsibleId,
            };

        return calculationResultCompleted;
    }

    private static Contracts.Events.V2.CalculationResultCompleted CreateInternal(EnergyResult result)
    {
        var calculationResultCompleted = new Contracts.Events.V2.CalculationResultCompleted
        {
            BatchId = result.BatchId.ToString(),
            Resolution = Resolution.Quarter,
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
                .Select(timeSeriesPoint =>
                {
                    var mappedTimeSeriesPoint = new TimeSeriesPoint
                    {
                        Quantity = new DecimalValue(timeSeriesPoint.Quantity),
                        Time = timeSeriesPoint.Time.ToTimestamp(),
                        QuantityQuality = QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality),
                    };
                    return mappedTimeSeriesPoint;
                }));
        return calculationResultCompleted;
    }
}
