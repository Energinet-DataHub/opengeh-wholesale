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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Types;
using Google.Protobuf.WellKnownTypes;
using TimeSeriesPoint = Energinet.DataHub.Wholesale.Contracts.Events.TimeSeriesPoint;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;

public class CalculationResultCompletedFactory : ICalculationResultCompletedFactory
{
    public CalculationResultCompleted Create(CalculationResult calculationResult)
    {
        if (calculationResult.EnergySupplierId == null && calculationResult.BalanceResponsibleId == null)
            return CreateForGridArea(calculationResult);

        if (calculationResult.EnergySupplierId != null && calculationResult.BalanceResponsibleId == null)
            return CreateForEnergySupplier(calculationResult);

        if (calculationResult.EnergySupplierId == null && calculationResult.BalanceResponsibleId != null)
            return CreateForBalanceResponsibleParty(calculationResult);

        return CreateForEnergySupplierByBalanceResponsibleParty(calculationResult);
    }

    private CalculationResultCompleted CreateForGridArea(CalculationResult result)
    {
        var calculationResultCompleted = CreateInternal(result);
        calculationResultCompleted.AggregationPerGridarea = new AggregationPerGridArea
        {
            GridAreaCode = result.GridArea,
        };

        return calculationResultCompleted;
    }

    private CalculationResultCompleted CreateForEnergySupplier(
        CalculationResult result)
    {
        var calculationResultCompleted = CreateInternal(result);
        calculationResultCompleted.AggregationPerEnergysupplierPerGridarea = new AggregationPerEnergySupplierPerGridArea
        {
            GridAreaCode = result.GridArea,
            EnergySupplierGlnOrEic = result.EnergySupplierId,
        };

        return calculationResultCompleted;
    }

    private CalculationResultCompleted CreateForBalanceResponsibleParty(
        CalculationResult result)
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

    private CalculationResultCompleted CreateForEnergySupplierByBalanceResponsibleParty(
        CalculationResult result)
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

    private static CalculationResultCompleted CreateInternal(CalculationResult result)
    {
        var calculationResultCompleted = new CalculationResultCompleted
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
                .Select(timeSeriesPoint => new TimeSeriesPoint
                {
                    Quantity = new DecimalValue(timeSeriesPoint.Quantity),
                    Time = timeSeriesPoint.Time.ToTimestamp(),
                    QuantityQuality = QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality),
                }));
        return calculationResultCompleted;
    }
}
