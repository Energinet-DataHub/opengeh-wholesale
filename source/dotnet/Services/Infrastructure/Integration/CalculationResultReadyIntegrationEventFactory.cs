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
using TimeSeriesPoint = Energinet.DataHub.Wholesale.Contracts.Events.TimeSeriesPoint;

namespace Energinet.DataHub.Wholesale.Infrastructure.Integration;

public class CalculationResultReadyIntegrationEventFactory : ICalculationResultReadyIntegrationEventFactory
{
    public CalculationResultCompleted CreateCalculationResultCompletedForGridArea(
        ProcessStepResult processStepResultDto,
        ProcessCompletedEventDto processCompletedEventDto)
    {
        var calculationResultCompleted = new CalculationResultCompleted
        {
            BatchId = processCompletedEventDto.BatchId.ToString(),
            Resolution = Resolution.Quarter,
            ProcessType = ProcessTypeMapper.MapProcessType(processCompletedEventDto.ProcessType),
            QuantityUnit = QuantityUnit.Kwh,
            AggregationPerGridarea = new AggregationPerGridArea
            {
                GridAreaCode = processCompletedEventDto.GridAreaCode,
            },
            PeriodStartUtc = processCompletedEventDto.PeriodStart.ToTimestamp(),
            PeriodEndUtc = processCompletedEventDto.PeriodEnd.ToTimestamp(),
            TimeSeriesType = TimeSeriesTypeMapper.MapTimeSeriesType(processStepResultDto.TimeSeriesType),
        };

        calculationResultCompleted.TimeSeriesPoints
            .AddRange(processStepResultDto.TimeSeriesPoints
                .Select(x => new TimeSeriesPoint
                {
                    Quantity = new DecimalValue(x.Quantity),
                    Time = x.Time.ToTimestamp(),
                    QuantityQuality = QuantityQualityMapper.MapQuantityQuality(x.Quality),
                }));

        return calculationResultCompleted;
    }
}
