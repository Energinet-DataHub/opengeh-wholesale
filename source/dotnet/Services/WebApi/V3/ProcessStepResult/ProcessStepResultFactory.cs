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

using Energinet.DataHub.Wholesale.Application.Batches.Model;

namespace Energinet.DataHub.Wholesale.WebApi.V3.ProcessStepResult;

public class ProcessStepResultFactory : IProcessStepResultFactory
{
    public ProcessStepResultDto Create(Contracts.ProcessStepResultDto stepResult, BatchDto batch)
    {
        return new ProcessStepResultDto(
            stepResult.Sum,
            stepResult.Min,
            stepResult.Max,
            batch.PeriodStart,
            batch.PeriodEnd,
            batch.Resolution,
            batch.Unit,
            stepResult.TimeSeriesPoints.Select(MapTimeSeriesPoint()).ToArray());
    }

    private static Func<Contracts.TimeSeriesPointDto, TimeSeriesPointDto> MapTimeSeriesPoint()
    {
        return p => new TimeSeriesPointDto(p.Time, p.Quantity, p.Quality);
    }
}
