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
using Energinet.DataHub.Wholesale.Events.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Types;
using Google.Protobuf.WellKnownTypes;
using TimeSeriesPoint = Energinet.DataHub.Wholesale.Contracts.Events.TimeSeriesPoint;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;

public class CalculationResultCompletedIntegrationEventFactory : ICalculationResultCompletedIntegrationEventFactory
{
    public CalculationResultCompleted CreateForGridArea(
        ProcessStepResult processStepResultDto,
        ProcessCompletedEventDto processCompletedEventDto)
    {
        var calculationResultCompleted = Create(processStepResultDto, processCompletedEventDto);
        calculationResultCompleted.AggregationPerGridarea = new AggregationPerGridArea
        {
            GridAreaCode = processCompletedEventDto.GridAreaCode,
        };

        return calculationResultCompleted;
    }

    public CalculationResultCompleted CreateForEnergySupplier(
        ProcessStepResult processStepResultDto,
        ProcessCompletedEventDto processCompletedEventDto,
        string energySupplierGln)
    {
        var calculationResultCompleted = Create(processStepResultDto, processCompletedEventDto);
        calculationResultCompleted.AggregationPerEnergysupplierPerGridarea = new AggregationPerEnergySupplierPerGridArea
        {
            GridAreaCode = processCompletedEventDto.GridAreaCode,
            EnergySupplierGlnOrEic = energySupplierGln,
        };

        return calculationResultCompleted;
    }

    public CalculationResultCompleted CreateForBalanceResponsibleParty(
        ProcessStepResult processStepResultDto,
        ProcessCompletedEventDto processCompletedEventDto,
        string balanceResponsiblePartyGln)
    {
        var calculationResultCompleted = Create(processStepResultDto, processCompletedEventDto);
        calculationResultCompleted.AggregationPerBalanceresponsiblepartyPerGridarea =
            new AggregationPerBalanceResponsiblePartyPerGridArea
            {
                GridAreaCode = processCompletedEventDto.GridAreaCode,
                BalanceResponsiblePartyGlnOrEic = balanceResponsiblePartyGln,
            };

        return calculationResultCompleted;
    }

    public CalculationResultCompleted CreateForEnergySupplierByBalanceResponsibleParty(
        ProcessStepResult result,
        ProcessCompletedEventDto processCompletedEvent,
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
        ProcessStepResult processStepResultDto,
        ProcessCompletedEventDto processCompletedEventDto)
    {
        var calculationResultCompleted = new CalculationResultCompleted
        {
            BatchId = processCompletedEventDto.BatchId.ToString(),
            Resolution = Resolution.Quarter,
            ProcessType = ProcessTypeMapper.MapProcessType(processCompletedEventDto.ProcessType),
            QuantityUnit = QuantityUnit.Kwh,
            PeriodStartUtc = processCompletedEventDto.PeriodStart.ToTimestamp(),
            PeriodEndUtc = processCompletedEventDto.PeriodEnd.ToTimestamp(),
            TimeSeriesType = TimeSeriesTypeMapper.MapTimeSeriesType(processStepResultDto.TimeSeriesType),
        };

        calculationResultCompleted.TimeSeriesPoints
            .AddRange(processStepResultDto.TimeSeriesPoints
                .Select(timeSeriesPoint => new TimeSeriesPoint
                {
                    Quantity = new DecimalValue(timeSeriesPoint.Quantity),
                    Time = timeSeriesPoint.Time.ToTimestamp(),
                    QuantityQuality = QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality),
                }));
        return calculationResultCompleted;
    }
}
