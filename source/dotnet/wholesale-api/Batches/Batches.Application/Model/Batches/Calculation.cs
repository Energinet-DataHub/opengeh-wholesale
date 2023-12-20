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

using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Batches.Application.Model.Batches;

public class Calculation
{
    private readonly List<GridAreaCode> _gridAreaCodes;

    public Calculation(
        Instant createdTime,
        ProcessType processType,
        IEnumerable<GridAreaCode> gridAreaCodes,
        Instant periodStart,
        Instant periodEnd,
        Instant executionTimeStart,
        DateTimeZone dateTimeZone,
        Guid createdByUserId)
        : this()
    {
        _gridAreaCodes = gridAreaCodes.ToList();
        if (!IsValid(_gridAreaCodes, processType, periodStart, periodEnd, dateTimeZone, out var errorMessages))
            throw new BusinessValidationException(string.Join(" ", errorMessages));

        ExecutionState = BatchExecutionState.Created;
        ProcessType = processType;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        ExecutionTimeStart = executionTimeStart;
        CreatedTime = createdTime;
        CreatedByUserId = createdByUserId;
        ExecutionTimeEnd = null;
        AreSettlementReportsCreated = false;
    }

    /// <summary>
    /// Validate if parameters are valid for a Batch.
    /// </summary>
    /// <param name="gridAreaCodes"></param>
    /// <param name="processType"></param>
    /// <param name="periodStart"></param>
    /// <param name="periodEnd"></param>
    /// <param name="dateTimeZone"></param>
    /// <param name="validationErrors"></param>
    /// <returns>If the parameters are valid for a Batch</returns>
    private static bool IsValid(
        IEnumerable<GridAreaCode> gridAreaCodes,
        ProcessType processType,
        Instant periodStart,
        Instant periodEnd,
        DateTimeZone dateTimeZone,
        out IEnumerable<string> validationErrors)
    {
        var errors = new List<string>();

        if (!gridAreaCodes.Any())
            errors.Add("Batch must contain at least one grid area code.");

        if (periodStart >= periodEnd)
            errors.Add("periodStart is greater or equal to periodEnd");

        var periodStartInTimeZone = new ZonedDateTime(periodStart, dateTimeZone);
        var periodEndInTimeZone = new ZonedDateTime(periodEnd, dateTimeZone);

        // Validate that period end is set to midnight
        if (periodEndInTimeZone.TimeOfDay != LocalTime.Midnight)
            errors.Add($"The period end '{periodEnd.ToString()}' must be midnight.");

        if (periodStartInTimeZone.TimeOfDay != LocalTime.Midnight)
            errors.Add($"The period start '{periodStart.ToString()}' must be midnight.");

        if (processType is ProcessType.WholesaleFixing
            or ProcessType.FirstCorrectionSettlement
            or ProcessType.SecondCorrectionSettlement
            or ProcessType.ThirdCorrectionSettlement)
        {
            if (!IsEntireMonth(periodStartInTimeZone, periodEndInTimeZone))
            {
                errors.Add($"The period (start: {periodStart} end: {periodEnd}) has to be an entire month when using process type {processType}.");
            }
        }

        validationErrors = errors;
        return !errors.Any();
    }

    private static bool IsEntireMonth(ZonedDateTime periodStart, ZonedDateTime periodEnd)
    {
        return periodStart.Day == 1 && periodEnd.LocalDateTime == periodStart.LocalDateTime.PlusMonths(1);
    }

    /// <summary>
    /// Required by Entity Framework
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private Calculation()
    {
        Id = Guid.NewGuid();
        _gridAreaCodes = new List<GridAreaCode>();
    }

    // Private setter is used implicitly by tests
    public Guid Id { get; private set; }

    public ProcessType ProcessType { get; }

    public IReadOnlyCollection<GridAreaCode> GridAreaCodes => _gridAreaCodes;

    public BatchExecutionState ExecutionState { get; private set; }

    public Instant? ExecutionTimeStart { get; }

    public Instant CreatedTime { get; }

    public Guid CreatedByUserId { get; }

    public Instant? ExecutionTimeEnd { get; private set; }

    public CalculationId? CalculationId { get; private set; }

    /// <summary>
    /// Must be exactly at the beginning (at 00:00:00 o'clock) of the local date.
    /// </summary>
    public Instant PeriodStart { get; }

    /// <summary>
    /// Must be exactly 1 ms before the end (midnight) of the local date.
    /// The 1 ms off is by design originating from the front-end decision on how to handle date periods.
    /// </summary>
    public Instant PeriodEnd { get; }

    public bool AreSettlementReportsCreated { get; set; }

    /// <summary>
    /// Get the ISO 8601 duration for the given process type.
    /// </summary>
    public string GetResolution()
    {
        switch (ProcessType)
        {
            case ProcessType.BalanceFixing:
            case ProcessType.Aggregation:
            case ProcessType.WholesaleFixing:
            case ProcessType.FirstCorrectionSettlement:
            case ProcessType.SecondCorrectionSettlement:
            case ProcessType.ThirdCorrectionSettlement:
                return "PT15M";
            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Get the unit for result values (an energy unit for aggregations and a price unit/currency for settlements).
    /// </summary>
    public QuantityUnit GetQuantityUnit()
    {
        switch (ProcessType)
        {
            case ProcessType.BalanceFixing:
            case ProcessType.Aggregation:
            case ProcessType.WholesaleFixing:
            case ProcessType.FirstCorrectionSettlement:
            case ProcessType.SecondCorrectionSettlement:
            case ProcessType.ThirdCorrectionSettlement:
                return QuantityUnit.Kwh;
            default:
                throw new NotImplementedException();
        }
    }

    public void MarkAsSubmitted(CalculationId calculationId)
    {
        ArgumentNullException.ThrowIfNull(calculationId);
        if (ExecutionState is BatchExecutionState.Submitted or BatchExecutionState.Pending
            or BatchExecutionState.Executing or BatchExecutionState.Completed or BatchExecutionState.Failed)
        {
            ThrowInvalidStateTransitionException(ExecutionState, BatchExecutionState.Submitted);
        }

        CalculationId = calculationId;
        ExecutionState = BatchExecutionState.Submitted;
    }

    public void MarkAsPending()
    {
        if (ExecutionState is BatchExecutionState.Pending or BatchExecutionState.Executing or BatchExecutionState.Completed or BatchExecutionState.Failed)
            ThrowInvalidStateTransitionException(ExecutionState, BatchExecutionState.Pending);
        ExecutionState = BatchExecutionState.Pending;
    }

    public void MarkAsExecuting()
    {
        if (ExecutionState is BatchExecutionState.Executing or BatchExecutionState.Completed or BatchExecutionState.Failed)
            ThrowInvalidStateTransitionException(ExecutionState, BatchExecutionState.Executing);

        ExecutionState = BatchExecutionState.Executing;
    }

    public void MarkAsCompleted(Instant executionTimeEnd)
    {
        if (ExecutionState is BatchExecutionState.Completed or BatchExecutionState.Failed)
            ThrowInvalidStateTransitionException(ExecutionState, BatchExecutionState.Completed);

        if (executionTimeEnd < ExecutionTimeStart)
        {
            throw new BusinessValidationException(
                $"Execution time end '{executionTimeEnd}' cannot be before execution time start '{ExecutionTimeStart}'");
        }

        ExecutionState = BatchExecutionState.Completed;
        ExecutionTimeEnd = executionTimeEnd;
    }

    public void MarkAsFailed()
    {
        if (ExecutionState is BatchExecutionState.Failed)
            ThrowInvalidStateTransitionException(ExecutionState, BatchExecutionState.Failed);

        ExecutionState = BatchExecutionState.Failed;
    }

    /// <summary>
    /// Reset a batch. This will ensure that it will be picked up and run again in a new calculation.
    /// </summary>
    public void Reset()
    {
        if (ExecutionState is BatchExecutionState.Completed)
            ThrowInvalidStateTransitionException(ExecutionState, BatchExecutionState.Created);

        ExecutionState = BatchExecutionState.Created;
    }

    private void ThrowInvalidStateTransitionException(BatchExecutionState currentState, BatchExecutionState desiredState)
    {
        throw new BusinessValidationException($"Cannot change batchExecutionState from {currentState} to {desiredState}");
    }
}
