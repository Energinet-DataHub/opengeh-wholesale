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
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Events.Application.CalculationResultPublishing.Model;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Types;
using Google.Protobuf.WellKnownTypes;
using TimeSeriesPoint = Energinet.DataHub.Wholesale.Contracts.Events.TimeSeriesPoint;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;

public class CalculationResultCompletedIntegrationEventFactory : ICalculationResultCompletedIntegrationEventFactory
{
    public CalculationResultCompleted CreateForGridArea(
        CalculationResult calculationResultDto,
        BatchGridAreaInfo batchGridAreaInfo)
    {
        var calculationResultCompleted = Create(calculationResultDto, batchGridAreaInfo);
        calculationResultCompleted.AggregationPerGridarea = new AggregationPerGridArea
        {
            GridAreaCode = batchGridAreaInfo.GridAreaCode,
        };

        return calculationResultCompleted;
    }

    public CalculationResultCompleted CreateForEnergySupplier(
        CalculationResult calculationResultDto,
        BatchGridAreaInfo batchGridAreaInfo,
        string energySupplierGln)
    {
        var calculationResultCompleted = Create(calculationResultDto, batchGridAreaInfo);
        calculationResultCompleted.AggregationPerEnergysupplierPerGridarea = new AggregationPerEnergySupplierPerGridArea
        {
            GridAreaCode = batchGridAreaInfo.GridAreaCode,
            EnergySupplierGlnOrEic = energySupplierGln,
        };

        return calculationResultCompleted;
    }

    public CalculationResultCompleted CreateForBalanceResponsibleParty(
        CalculationResult calculationResultDto,
        BatchGridAreaInfo batchGridAreaInfo,
        string balanceResponsiblePartyGln)
    {
        var calculationResultCompleted = Create(calculationResultDto, batchGridAreaInfo);
        calculationResultCompleted.AggregationPerBalanceresponsiblepartyPerGridarea =
            new AggregationPerBalanceResponsiblePartyPerGridArea
            {
                GridAreaCode = batchGridAreaInfo.GridAreaCode,
                BalanceResponsiblePartyGlnOrEic = balanceResponsiblePartyGln,
            };

        return calculationResultCompleted;
    }

    public CalculationResultCompleted CreateForEnergySupplierByBalanceResponsibleParty(
        CalculationResult result,
        BatchGridAreaInfo processCompletedEvent,
        string energySupplierGln,
        string balanceResponsiblePartyGln)
    {
        var calculationResultCompleted = Create(result, processCompletedEvent);
        calculationResultCompleted.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea =
            new AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea
            {
                GridAreaCode = processCompletedEvent.GridAreaCode,
                EnergySupplierGlnOrEic = energySupplierGln,
                BalanceResponsiblePartyGlnOrEic = balanceResponsiblePartyGln,
            };

        return calculationResultCompleted;
    }

    private static CalculationResultCompleted Create(
        CalculationResult calculationResultDto,
        BatchGridAreaInfo batchGridAreaInfo)
    {
        var calculationResultCompleted = new CalculationResultCompleted
        {
            BatchId = batchGridAreaInfo.BatchId.ToString(),
            Resolution = Resolution.Quarter,
            ProcessType = ProcessTypeMapper.MapProcessType(batchGridAreaInfo.ProcessType),
            QuantityUnit = QuantityUnit.Kwh,
            PeriodStartUtc = batchGridAreaInfo.PeriodStart.ToTimestamp(),
            PeriodEndUtc = batchGridAreaInfo.PeriodEnd.ToTimestamp(),
            TimeSeriesType = TimeSeriesTypeMapper.MapTimeSeriesType(calculationResultDto.TimeSeriesType),
        };

        calculationResultCompleted.TimeSeriesPoints
            .AddRange(calculationResultDto.TimeSeriesPoints
                .Select(timeSeriesPoint => new TimeSeriesPoint
                {
                    Quantity = new DecimalValue(timeSeriesPoint.Quantity),
                    Time = timeSeriesPoint.Time.ToTimestamp(),
                    QuantityQuality = QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality),
                }));
        return calculationResultCompleted;
    }
}
