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
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;

namespace Energinet.DataHub.Wholesale.Infrastructure.Integration;

public class CalculationResultReadyIntegrationEventMapper : ICalculationResultReadyIntegrationEventMapper
{
    public ProcessStepResult MapFrom(
        ProcessStepResult processStepResultDto,
        ProcessCompletedEventDto processCompletedEventDto)
    {
        // var calculationResultCompleted = new CalculationResultCompleted()
        // {
        //     BatchId = processCompletedEventDto.BatchId.ToString(),
        //     Resolution = Resolution.Quarter,
        //     ProcessType = MapProcessType(processCompletedEventDto.ProcessType),
        //     QuantityUnit = QuantityUnit.Kwh,
        //     AggregationPerGridarea = new AggregationPerGridArea
        //     {
        //         GridAreaCode = processCompletedEventDto.GridAreaCode,
        //     },
        //     PeriodStartUtc = processCompletedEventDto.PeriodStart.ToTimestamp(),
        //     PeriodEndUtc = processCompletedEventDto.PeriodEnd.ToTimestamp(),
        //     TimeSeriesType = Contracts.Events.TimeSeriesType.Production,
        // };
        // calculationResultCompleted.TimeSeriesPoints
        //     .AddRange(processStepResultDto.TimeSeriesPoints
        //         .Select(x => new TimeSeriesPoint { Quantity = new DecimalValue { Units = (long)x.Quantity }, Time = x.Time.ToTimestamp() })); // How to set DecimalValue correctly? I Dont understand how to use QuantityQuality
        // return calculationResultCompleted;
        throw new NotImplementedException();
    }
}
