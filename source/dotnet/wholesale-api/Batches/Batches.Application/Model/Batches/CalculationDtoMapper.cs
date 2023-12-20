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

public class CalculationDtoMapper : ICalculationDtoMapper
{
    public BatchDto Map(Calculation calculation)
    {
        return new BatchDto(
            calculation.CalculationId?.Id,
            calculation.Id,
            calculation.PeriodStart.ToDateTimeOffset(),
            calculation.PeriodEnd.ToDateTimeOffset(),
            calculation.GetResolution(),
            calculation.GetQuantityUnit().ToString(),
            calculation.ExecutionTimeStart?.ToDateTimeOffset(),
            calculation.ExecutionTimeEnd?.ToDateTimeOffset() ?? null,
            MapState(calculation.ExecutionState),
            calculation.AreSettlementReportsCreated,
            MapGridAreaCodes(calculation.GridAreaCodes),
            calculation.ProcessType,
            calculation.CreatedByUserId);
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
