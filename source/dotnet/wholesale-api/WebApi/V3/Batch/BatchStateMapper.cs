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

namespace Energinet.DataHub.Wholesale.WebApi.V3.Batch;

public static class BatchStateMapper
{
    public static BatchState MapState(Batches.Interfaces.Models.BatchState batchDtoExecutionState)
    {
        return batchDtoExecutionState switch
        {
            Batches.Interfaces.Models.BatchState.Pending => BatchState.Pending,
            Batches.Interfaces.Models.BatchState.Executing => BatchState.Executing,
            Batches.Interfaces.Models.BatchState.Completed => BatchState.Completed,
            Batches.Interfaces.Models.BatchState.Failed => BatchState.Failed,

            _ => throw new ArgumentOutOfRangeException(
                nameof(batchDtoExecutionState),
                actualValue: batchDtoExecutionState,
                "Value cannot be mapped to a batch state."),
        };
    }

    public static Batches.Interfaces.Models.BatchState? MapState(BatchState? batchDtoExecutionState)
    {
        if (batchDtoExecutionState == null)
        {
            return null;
        }

        return batchDtoExecutionState switch
        {
            BatchState.Pending => Batches.Interfaces.Models.BatchState.Pending,
            BatchState.Executing => Batches.Interfaces.Models.BatchState.Executing,
            BatchState.Completed => Batches.Interfaces.Models.BatchState.Completed,
            BatchState.Failed => Batches.Interfaces.Models.BatchState.Failed,

            _ => throw new ArgumentOutOfRangeException(
                nameof(batchDtoExecutionState),
                actualValue: batchDtoExecutionState,
                "Value cannot be mapped to a nullable batch state."),
        };
    }
}
