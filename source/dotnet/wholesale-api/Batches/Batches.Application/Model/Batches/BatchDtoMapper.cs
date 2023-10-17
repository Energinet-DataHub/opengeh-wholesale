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

using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;

namespace Energinet.DataHub.Wholesale.Batches.Application.Model.Batches;

public class BatchDtoMapper : IBatchDtoMapper
{
    public BatchDto Map(Batch batch)
    {
        return new BatchDto(
            batch.CalculationId?.Id,
            batch.Id,
            batch.PeriodStart.ToDateTimeOffset(),
            batch.PeriodEnd.ToDateTimeOffset(),
            batch.GetResolution(),
            batch.GetQuantityUnit().ToString(),
            batch.ExecutionTimeStart?.ToDateTimeOffset(),
            batch.ExecutionTimeEnd?.ToDateTimeOffset() ?? null,
            MapState(batch.ExecutionState),
            batch.AreSettlementReportsCreated,
            MapGridAreaCodes(batch.GridAreaCodes),
            batch.ProcessType,
            batch.CreatedByUserId);
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

            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                actualValue: state,
                "Value cannot be mapped to a batch state."),
        };
    }

    private static string[] MapGridAreaCodes(IReadOnlyCollection<GridAreaCode> gridAreaCodes)
    {
        return gridAreaCodes.Select(gridArea => gridArea.Code).ToArray();
    }
}
