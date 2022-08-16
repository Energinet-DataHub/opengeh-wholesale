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

using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;

namespace Energinet.DataHub.Wholesale.Domain.BatchAggregate;

public class Batch
{
    private readonly List<GridAreaCode> _gridAreaCodes;

    public Batch(ProcessType processType, IEnumerable<GridAreaCode> gridAreaCodes)
    {
        Id = new BatchId();
        ExecutionState = BatchExecutionState.Pending;
        ProcessType = processType;

        _gridAreaCodes = gridAreaCodes.ToList();
        if (!_gridAreaCodes.Any())
            throw new ArgumentException("Batch must contain at least one grid area code.");
    }

    /// <summary>
    /// Required by Entity Framework
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private Batch()
    {
        Id = null!;
        _gridAreaCodes = new List<GridAreaCode>();
    }

    public BatchId Id { get; }

    public ProcessType ProcessType { get; }

    public IReadOnlyCollection<GridAreaCode> GridAreaCodes => _gridAreaCodes;

    public BatchExecutionState ExecutionState { get; private set; }

    public JobRunId? RunId { get; private set; }

    public void ResetStatus()
    {
        if (ExecutionState == BatchExecutionState.Completed)
            throw new InvalidOperationException("Cannot reset status of a completed batch.");

        ExecutionState = BatchExecutionState.Pending;
        RunId = null;
    }

    public void MarkAsCompleted()
    {
        if (ExecutionState != BatchExecutionState.Executing)
            throw new InvalidOperationException("Batch cannot be completed because it is not in state executing.");

        ExecutionState = BatchExecutionState.Completed;
    }

    public void MarkAsExecuting(JobRunId jobRunId)
    {
        ArgumentNullException.ThrowIfNull(jobRunId);

        if (ExecutionState != BatchExecutionState.Pending)
            throw new InvalidOperationException("Batch cannot be completed because it is not in state pending.");

        ExecutionState = BatchExecutionState.Executing;
        RunId = jobRunId;
    }
}
