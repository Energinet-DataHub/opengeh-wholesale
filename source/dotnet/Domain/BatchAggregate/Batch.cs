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

using Energinet.DataHub.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;

namespace Energinet.DataHub.Wholesale.Domain.BatchAggregate;

public class Batch
{
    public Batch(WholesaleProcessType processType, IEnumerable<GridAreaId> gridAreaIds)
    {
        Id = new BatchId();
        ExecutionState = BatchExecutionState.Requested;
        ProcessType = processType;
        GridAreaIds = gridAreaIds.ToList();
    }

    /// <summary>
    /// Required by Entity Framework
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private Batch()
    {
        Id = null!;
        GridAreaIds = new List<GridAreaId>();
    }

    public BatchId Id { get; }
    public WholesaleProcessType ProcessType { get; }
    public List<GridAreaId> GridAreaIds { get; }
    public BatchExecutionState ExecutionState { get; }
}
