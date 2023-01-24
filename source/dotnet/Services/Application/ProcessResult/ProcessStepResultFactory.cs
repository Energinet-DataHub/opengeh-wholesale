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

using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;

namespace Energinet.DataHub.Wholesale.Application.ProcessResult;

public class ProcessStepResultFactory : IProcessStepResultFactory
{
    public ProcessStepResultDto Create(ProcessStepResult processStepResult, Batch batch)
    {
        ArgumentNullException.ThrowIfNull(processStepResult);

        return new ProcessStepResultDto(
            ProcessStepMeteringPointType.Production,
            processStepResult.Sum,
            processStepResult.Min,
            processStepResult.Max,
            processStepResult
                .TimeSeriesPoints
                .Select(MapTimeSeriesPointToDto)
                .ToArray(),
            batch.PeriodStart.ToDateTimeOffset(),
            batch.PeriodEnd.ToDateTimeOffset());
    }

    private static TimeSeriesPointDto MapTimeSeriesPointToDto(TimeSeriesPoint point)
    {
        return new TimeSeriesPointDto(point.Time, point.Quantity, point.Quality);
    }
}
