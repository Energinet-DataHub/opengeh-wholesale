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

using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Google.Protobuf.WellKnownTypes;
using QuantityQuality = Energinet.DataHub.Wholesale.Contracts.Events.QuantityQuality;
using TimeSeriesPoint = Energinet.DataHub.Wholesale.Contracts.Events.TimeSeriesPoint;

namespace Energinet.DataHub.Wholesale.Infrastructure.Integration;

public class CalculationResultReadyIntegrationEventMapper : ICalculationResultReadyIntegrationEventMapper
{
    public CalculationResultCompleted MapFrom(
        ProcessStepResult processStepResultDto,
        ProcessCompletedEventDto processCompletedEventDto)
    {
        var calculationResultCompleted = new CalculationResultCompleted
        {
            BatchId = processCompletedEventDto.BatchId.ToString(),
            Resolution = Resolution.Quarter,
            ProcessType = MapProcessType(processCompletedEventDto.ProcessType),
            QuantityUnit = QuantityUnit.Kwh,
            AggregationPerGridarea = new AggregationPerGridArea
            {
                GridAreaCode = processCompletedEventDto.GridAreaCode,
            },
            PeriodStartUtc = processCompletedEventDto.PeriodStart.ToTimestamp(),
            PeriodEndUtc = processCompletedEventDto.PeriodEnd.ToTimestamp(),
            TimeSeriesType = Contracts.Events.TimeSeriesType.Production,
        };
        calculationResultCompleted.TimeSeriesPoints
            .AddRange(processStepResultDto.TimeSeriesPoints
                .Select(x => new TimeSeriesPoint { Quantity = new DecimalValue(x.Quantity), Time = x.Time.ToTimestamp(), QuantityQuality = MapQuantityQuality(x.Quality) }));
        return calculationResultCompleted;
    }

    private QuantityQuality MapQuantityQuality(Domain.ProcessStepResultAggregate.QuantityQuality quantityQuality)
    {
        return quantityQuality switch
        {
            Domain.ProcessStepResultAggregate.QuantityQuality.Calculated => QuantityQuality.Read, // ?
            Domain.ProcessStepResultAggregate.QuantityQuality.Estimated => QuantityQuality.Read, // ?
            Domain.ProcessStepResultAggregate.QuantityQuality.Incomplete => QuantityQuality.Read, // ?
            Domain.ProcessStepResultAggregate.QuantityQuality.Measured => QuantityQuality.Measured,
            Domain.ProcessStepResultAggregate.QuantityQuality.Missing => QuantityQuality.Missing,
            _ => throw new ArgumentException(),
        };
    }

    private ProcessType MapProcessType(Contracts.ProcessType processType)
    {
        return processType switch
        {
            Contracts.ProcessType.Aggregation => ProcessType.Aggregation,
            Contracts.ProcessType.BalanceFixing => ProcessType.BalanceFixing,
            _ => ProcessType.Unspecified,
        };
    }
}
