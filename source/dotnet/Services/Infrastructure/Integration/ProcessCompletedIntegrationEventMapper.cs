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

using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Contracts;

namespace Energinet.DataHub.Wholesale.Infrastructure.Integration;

public class ProcessCompletedIntegrationEventMapper : IProcessCompletedIntegrationEventMapper
{
    public ProcessCompleted MapFrom(ProcessCompletedEventDto processCompletedEvent)
    {
        return new ProcessCompleted
        {
            BatchId = processCompletedEvent.BatchId.ToString(),
            ProcessType = GetProcessType(processCompletedEvent),
            GridAreaCode = processCompletedEvent.GridAreaCode,
            PeriodStartUtc = processCompletedEvent.PeriodStart.ToTimestamp(),
            PeriodEndUtc = processCompletedEvent.PeriodEnd.ToTimestamp(),
        };
    }

    private static ProcessCompleted.Types.ProcessType GetProcessType(ProcessCompletedEventDto processCompletedEvent) =>
        processCompletedEvent.ProcessType switch
        {
            ProcessType.BalanceFixing => ProcessCompleted.Types.ProcessType.PtBalancefixing,
            _ => throw new NotImplementedException(
                $"Cannot map process type '{processCompletedEvent.ProcessType.ToString()}'"),
        };
}
