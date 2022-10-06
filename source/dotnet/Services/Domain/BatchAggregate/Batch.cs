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
        ExecutionState = BatchExecutionState.Created;
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

    public void MarkAsSubmitted(JobRunId jobRunId)
    {
        if (ExecutionState is BatchExecutionState.Submitted or BatchExecutionState.Pending or BatchExecutionState.Executing or BatchExecutionState.Completed)
            throw new InvalidOperationException("Cannot change batchExecutionState from Submitted, Pending, Executing or Completed to Submitted");
        ArgumentNullException.ThrowIfNull(jobRunId);
        RunId = jobRunId;
        ExecutionState = BatchExecutionState.Submitted;
    }

    public void MarkAsPending()
    {
        if (ExecutionState is BatchExecutionState.Pending or BatchExecutionState.Executing or BatchExecutionState.Completed)
            throw new InvalidOperationException("Cannot change batchExecutionState from Pending, Executing or Completed to Pending");
        ExecutionState = BatchExecutionState.Pending;
    }

    public void MarkAsExecuting()
    {
        if (ExecutionState is BatchExecutionState.Executing or BatchExecutionState.Completed)
            throw new InvalidOperationException("Cannot change batchExecutionState from Executing or Completed to Executing");

        ExecutionState = BatchExecutionState.Executing;
    }

    public void MarkAsCompleted()
    {
        if (ExecutionState == BatchExecutionState.Completed)
            throw new InvalidOperationException("Cannot change batchExecutionState from Completed to Completed");

        ExecutionState = BatchExecutionState.Completed;
        ExecutionTimeEnd = _clock.GetCurrentInstant();
    }

    public void MarkAsFailed()
    {
        ExecutionState = BatchExecutionState.Failed;
    }
}
