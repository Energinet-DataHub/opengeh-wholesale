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
using NodaTime;

namespace Energinet.DataHub.Wholesale.Domain.BatchAggregate;

public class Batch
{
    private readonly List<GridAreaCode> _gridAreaCodes;
    private readonly IClock _clock;

    public Batch(ProcessType processType, IEnumerable<GridAreaCode> gridAreaCodes, Instant periodStart, Instant periodEnd, IClock clock)
        : this()
    {
        ExecutionState = BatchExecutionState.Pending;
        ProcessType = processType;
        _clock = clock;

        _gridAreaCodes = gridAreaCodes.ToList();
        if (!_gridAreaCodes.Any())
            throw new ArgumentException("Batch must contain at least one grid area code.");

        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        if (periodStart >= periodEnd)
        {
            throw new ArgumentException("periodStart is greater or equal to periodEnd");
        }

        ExecutionTimeStart = _clock.GetCurrentInstant();
        ExecutionTimeEnd = null;
    }

    /// <summary>
    /// Required by Entity Framework
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private Batch()
    {
        Id = Guid.NewGuid();
        _gridAreaCodes = new List<GridAreaCode>();
        _clock = SystemClock.Instance;
    }

    public Guid Id { get; }

    public ProcessType ProcessType { get; }

    public IReadOnlyCollection<GridAreaCode> GridAreaCodes => _gridAreaCodes;

    public BatchExecutionState ExecutionState { get; private set; }

    public Instant? ExecutionTimeStart { get; private set; }

    public Instant? ExecutionTimeEnd { get; private set; }

    public JobRunId? RunId { get; private set; }

    public Instant PeriodStart { get; }

    public Instant PeriodEnd { get; }

    public void MarkAsCompleted()
    {
        if (ExecutionState != BatchExecutionState.Executing)
            throw new InvalidOperationException("Batch cannot be completed because it is not in state executing.");

        ExecutionState = BatchExecutionState.Completed;
        ExecutionTimeEnd = _clock.GetCurrentInstant();
    }

    public void MarkAsExecuting(JobRunId jobRunId)
    {
        ArgumentNullException.ThrowIfNull(jobRunId);

        if (ExecutionState != BatchExecutionState.Pending)
            throw new InvalidOperationException("Batch cannot be completed because it is not in state pending.");

        ExecutionState = BatchExecutionState.Executing;
        RunId = jobRunId;
    }

    public void MarkAsFailed()
    {
        ExecutionState = BatchExecutionState.Failed;
    }
}
