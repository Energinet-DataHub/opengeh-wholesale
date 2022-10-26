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

using Energinet.DataHub.Wholesale.Domain.BatchAggregate;

namespace Energinet.DataHub.Wholesale.Application.Batches;

public class BatchDtoMapper : IBatchDtoMapper
{
    public BatchDto Map(Batch batch)
    {
        return new BatchDto(
            batch.RunId?.Id,
            batch.Id,
            batch.PeriodStart.ToDateTimeOffset(),
            batch.PeriodEnd.ToDateTimeOffset(),
            batch.ExecutionTimeStart?.ToDateTimeOffset() ?? null,
            batch.ExecutionTimeEnd?.ToDateTimeOffset() ?? null,
            MapState(batch.ExecutionState),
            batch.BatchHasBeenZipped);
    }

    private static BatchState MapState(BatchExecutionState state)
    {
        return state switch
        {
            BatchExecutionState.Created => BatchState.Pending,
            BatchExecutionState.Submitted => BatchState.Pending,
            BatchExecutionState.Pending => BatchState.Pending,
            BatchExecutionState.Executing => BatchState.Executing,
            BatchExecutionState.Completed => BatchState.Completed,
            BatchExecutionState.Failed => BatchState.Failed,
            _ => throw new ArgumentOutOfRangeException(nameof(state)),
        };
    }
}
