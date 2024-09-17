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

using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;

public class Calculation
{
    private readonly List<GridAreaCode> _gridAreaCodes;
    private CalculationOrchestrationState _orchestrationState;

    public Calculation(
        Instant createdTime,
        CalculationType calculationType,
        IEnumerable<GridAreaCode> gridAreaCodes,
        Instant periodStart,
        Instant periodEnd,
        Instant scheduledAt,
        DateTimeZone dateTimeZone,
        Guid createdByUserId,
        long version,
        bool isInternalCalculation)
        : this()
    {
        _gridAreaCodes = gridAreaCodes.ToList();
        if (!IsValid(
                _gridAreaCodes,
                calculationType,
                periodStart,
                periodEnd,
                dateTimeZone,
                isInternalCalculation,
                out var errorMessages))
        {
            throw new BusinessValidationException(string.Join(" ", errorMessages));
        }

        CalculationType = calculationType;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        ScheduledAt = scheduledAt;
        CreatedTime = createdTime;
        CreatedByUserId = createdByUserId;
        ExecutionTimeEnd = null;
        AreSettlementReportsCreated = false;
        Version = version;
        IsInternalCalculation = isInternalCalculation;

        _orchestrationState = CalculationOrchestrationState.Scheduled;
        ExecutionState = CalculationExecutionState.Created;
        OrchestrationInstanceId = new OrchestrationInstanceId(Guid.NewGuid().ToString("N"));
    }

    /// <summary>
    /// Validate if parameters are valid for a <see cref="Calculation"/>.
    /// </summary>
    /// <param name="gridAreaCodes"></param>
    /// <param name="calculationType"></param>
    /// <param name="periodStart"></param>
    /// <param name="periodEnd"></param>
    /// <param name="dateTimeZone"></param>
    /// <param name="isInternalCalculation"></param>
    /// <param name="validationErrors"></param>
    /// <returns>If the parameters are valid for a <see cref="Calculation"/></returns>
    private static bool IsValid(
        List<GridAreaCode> gridAreaCodes,
        CalculationType calculationType,
        Instant periodStart,
        Instant periodEnd,
        DateTimeZone dateTimeZone,
        bool isInternalCalculation,
        out IEnumerable<string> validationErrors)
    {
        var errors = new List<string>();

        if (!gridAreaCodes.Any())
            errors.Add("Calculation must contain at least one grid area code.");

        if (periodStart >= periodEnd)
            errors.Add("periodStart is greater or equal to periodEnd");

        var periodStartInTimeZone = new ZonedDateTime(periodStart, dateTimeZone);
        var periodEndInTimeZone = new ZonedDateTime(periodEnd, dateTimeZone);

        // Validate that period end is set to midnight
        if (periodEndInTimeZone.TimeOfDay != LocalTime.Midnight)
            errors.Add($"The period end '{periodEnd}' must be midnight.");

        if (periodStartInTimeZone.TimeOfDay != LocalTime.Midnight)
            errors.Add($"The period start '{periodStart}' must be midnight.");

        if (calculationType is CalculationType.WholesaleFixing
            or CalculationType.FirstCorrectionSettlement
            or CalculationType.SecondCorrectionSettlement
            or CalculationType.ThirdCorrectionSettlement)
        {
            if (!IsEntireMonth(periodStartInTimeZone, periodEndInTimeZone))
            {
                errors.Add($"The period (start: {periodStart} end: {periodEnd}) has to be an entire month when using calculation type {calculationType}.");
            }
        }

        if (isInternalCalculation && calculationType is not CalculationType.Aggregation)
            errors.Add($"Internal calculations is not allowed for {calculationType}.");

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
        _gridAreaCodes = [];
        Version = 0;
        OrchestrationInstanceId = null!;
    }

    // Private setter is used implicitly by tests
    public Guid Id { get; private set; }

    public CalculationType CalculationType { get; }

    public IReadOnlyCollection<GridAreaCode> GridAreaCodes => _gridAreaCodes;

    public CalculationExecutionState ExecutionState { get; private set; }

    /// <summary>
    /// Get/set the orchestration state of the calculation.
    /// Throws a <see cref="BusinessValidationException"/> if the state transition is invalid.
    /// </summary>
    public CalculationOrchestrationState OrchestrationState
    {
        get => _orchestrationState;
        private set
        {
            if (_orchestrationState == value)
                return; // Do nothing if the state isn't changing

            CalculationOrchestrationState[] validStateTransitions = _orchestrationState switch
            {
                CalculationOrchestrationState.Scheduled => [CalculationOrchestrationState.Started, CalculationOrchestrationState.Canceled],
                // The state can move from Started -> Calculated if the calculation was so quick we didn't see the Calculating state
                CalculationOrchestrationState.Started => [CalculationOrchestrationState.Calculating, CalculationOrchestrationState.Calculated],

                // The state can move from Calculating  -> Started if the databricks job is canceled and the calculation is reset and started again
                CalculationOrchestrationState.Calculating => [CalculationOrchestrationState.Calculated, CalculationOrchestrationState.CalculationFailed, CalculationOrchestrationState.Started],
                CalculationOrchestrationState.Calculated => [CalculationOrchestrationState.ActorMessagesEnqueuing, CalculationOrchestrationState.Completed],
                CalculationOrchestrationState.CalculationFailed => [CalculationOrchestrationState.Scheduled],
                CalculationOrchestrationState.ActorMessagesEnqueuing => [CalculationOrchestrationState.ActorMessagesEnqueued, CalculationOrchestrationState.ActorMessagesEnqueuingFailed],
                CalculationOrchestrationState.ActorMessagesEnqueued => [CalculationOrchestrationState.Completed],
                CalculationOrchestrationState.ActorMessagesEnqueuingFailed => [], // We do not support retries, so we are stuck in failed
                CalculationOrchestrationState.Completed => [],
                CalculationOrchestrationState.Canceled => [],
                _ => throw new ArgumentOutOfRangeException(nameof(_orchestrationState), _orchestrationState, "Unsupported CalculationOrchestrationState to get valid state transitions for"),
            };

            if (!validStateTransitions.Contains(value))
                ThrowInvalidStateTransitionException(_orchestrationState, value);

            _orchestrationState = value;
        }
    }

    /// <summary>
    /// When they calculation is scheduled to run.
    /// </summary>
    public Instant ScheduledAt { get; }

    /// <summary>
    /// When the calculation orchestration actually started (after the schedule is met).
    /// </summary>
    public Instant? ExecutionTimeStart { get; private set; }

    public Instant CreatedTime { get; }

    public Guid CreatedByUserId { get; }

    /// <summary>
    /// The user who canceled the scheduled calculation.
    /// </summary>
    public Guid? CanceledByUserId { get; private set; }

    public Instant? ExecutionTimeEnd { get; private set; }

    public Instant? ActorMessagesEnqueuingTimeStart { get; private set; }

    public Instant? ActorMessagesEnqueuedTimeEnd { get; private set; }

    public Instant? CompletedTime { get; private set; }

    public CalculationJobId? CalculationJobId { get; private set; }

    public OrchestrationInstanceId OrchestrationInstanceId { get; }

    /// <summary>
    /// Must be exactly at the beginning (at 00:00:00 o'clock) of the local date.
    /// </summary>
    public Instant PeriodStart { get; private set; }

    /// <summary>
    /// Must be exactly 1 ms before the end (midnight) of the local date.
    /// The 1 ms off is by design originating from the front-end decision on how to handle date periods.
    /// </summary>
    public Instant PeriodEnd { get; }

    public bool AreSettlementReportsCreated { get; set; }

    /// <summary>
    /// The calculation version. The value of this property represents the number of 100-nanosecond
    /// intervals that have elapsed since 12:00:00 midnight, January 1, 0001 in the Gregorian calendar.
    /// https://learn.microsoft.com/en-us/dotnet/api/System.DateTime.Ticks?view=net-7.0
    /// The version is created with the calculation.
    /// </summary>
    public long Version { get; private set; }

    /// <summary>
    /// When a calculation is internal, the calculation result is for internal use only.
    /// Results should not be exposed to external users/actors.
    /// </summary>
    public bool IsInternalCalculation { get; }

    /// <summary>
    /// Get the ISO 8601 duration for the given calculation type.
    /// </summary>
    public string GetResolution()
    {
        return CalculationType switch
        {
            CalculationType.BalanceFixing
            or CalculationType.Aggregation
            or CalculationType.WholesaleFixing
            or CalculationType.FirstCorrectionSettlement
            or CalculationType.SecondCorrectionSettlement
            or CalculationType.ThirdCorrectionSettlement
                => "PT15M",

            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Get the unit for result values (an energy unit for aggregations and a price unit/currency for settlements).
    /// </summary>
    public QuantityUnit GetQuantityUnit()
    {
        return CalculationType switch
        {
            CalculationType.BalanceFixing
            or CalculationType.Aggregation
            or CalculationType.WholesaleFixing
            or CalculationType.FirstCorrectionSettlement
            or CalculationType.SecondCorrectionSettlement
            or CalculationType.ThirdCorrectionSettlement
                => QuantityUnit.Kwh,

            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// The calculation orchestration has started (after the schedule has been met)
    /// </summary>
    public void MarkAsStarted()
    {
        if (ExecutionState is not CalculationExecutionState.Created)
            ThrowInvalidStateTransitionException(ExecutionState, CalculationExecutionState.Created);

        if (OrchestrationState is not CalculationOrchestrationState.Scheduled)
            ThrowInvalidStateTransitionException(OrchestrationState, CalculationOrchestrationState.Started);

        OrchestrationState = CalculationOrchestrationState.Started;
    }

    /// <summary>
    /// The calculation job has been submitted to the calculation engine (databricks).
    /// </summary>
    /// <param name="calculationJobId">The external identifier from the calculation engine</param>
    /// <param name="executionTimeStart">When the job was submitted to the calculation engine</param>
    public void MarkAsCalculationJobSubmitted(CalculationJobId calculationJobId, Instant executionTimeStart)
    {
        ArgumentNullException.ThrowIfNull(calculationJobId);
        if (ExecutionState is CalculationExecutionState.Submitted or CalculationExecutionState.Pending
            or CalculationExecutionState.Executing or CalculationExecutionState.Completed or CalculationExecutionState.Failed)
        {
            ThrowInvalidStateTransitionException(ExecutionState, CalculationExecutionState.Submitted);
        }

        CalculationJobId = calculationJobId;
        ExecutionTimeStart = executionTimeStart;
        ExecutionState = CalculationExecutionState.Submitted;
        OrchestrationState = CalculationOrchestrationState.Calculating;
    }

    /// <summary>
    /// The calculation job is submitted and pending execution in the calculation engine (databricks).
    /// </summary>
    public void MarkAsCalculationJobPending()
    {
        if (ExecutionState is CalculationExecutionState.Executing or CalculationExecutionState.Completed or CalculationExecutionState.Failed)
            ThrowInvalidStateTransitionException(ExecutionState, CalculationExecutionState.Pending);

        ExecutionState = CalculationExecutionState.Pending;
        OrchestrationState = CalculationOrchestrationState.Calculating;
    }

    /// <summary>
    /// The calculation orchestration is waiting for the calculation job to complete.
    /// </summary>
    public void MarkAsCalculating()
    {
        if (ExecutionState is CalculationExecutionState.Completed or CalculationExecutionState.Failed)
            ThrowInvalidStateTransitionException(ExecutionState, CalculationExecutionState.Executing);

        ExecutionState = CalculationExecutionState.Executing;
        OrchestrationState = CalculationOrchestrationState.Calculating;
    }

    /// <summary>
    /// The calculation orchestration is calculated when the calculation job is complete.
    /// </summary>
    /// <param name="executionTimeEnd">When the calculation job completed in the calculation engine</param>
    public void MarkAsCalculated(Instant executionTimeEnd)
    {
        if (ExecutionState is CalculationExecutionState.Completed or CalculationExecutionState.Failed)
            ThrowInvalidStateTransitionException(ExecutionState, CalculationExecutionState.Completed);

        if (executionTimeEnd < ExecutionTimeStart)
        {
            throw new BusinessValidationException(
                $"Execution time end '{executionTimeEnd}' cannot be before execution time start '{ExecutionTimeStart}'");
        }

        ExecutionState = CalculationExecutionState.Completed;
        ExecutionTimeEnd = executionTimeEnd;
        OrchestrationState = CalculationOrchestrationState.Calculated;
    }

    public void MarkAsCalculationFailed()
    {
        if (ExecutionState is CalculationExecutionState.Failed)
            ThrowInvalidStateTransitionException(ExecutionState, CalculationExecutionState.Failed);

        ExecutionState = CalculationExecutionState.Failed;
        OrchestrationState = CalculationOrchestrationState.CalculationFailed;
    }

    public void MarkAsActorMessagesEnqueuing(Instant enqueuingTimeStart)
    {
        if (IsInternalCalculation)
        {
            throw new BusinessValidationException(
                $"Calculation with ID '{Id}' is not allowed to be marked as '{CalculationOrchestrationState.ActorMessagesEnqueuing}' because it is an internal calculation.");
        }

        ActorMessagesEnqueuingTimeStart = enqueuingTimeStart;
        OrchestrationState = CalculationOrchestrationState.ActorMessagesEnqueuing;
    }

    public void MarkAsActorMessagesEnqueued(Instant enqueuedTimeEnd)
    {
        if (enqueuedTimeEnd < ActorMessagesEnqueuingTimeStart)
        {
            throw new BusinessValidationException(
                $"Actor messages enqueued time end '{enqueuedTimeEnd}' cannot be before enqueuing time start '{ActorMessagesEnqueuingTimeStart}'");
        }

        if (IsInternalCalculation)
        {
            throw new BusinessValidationException(
                $"Calculation with ID '{Id}' is not allowed to be marked as '{CalculationOrchestrationState.ActorMessagesEnqueued}' because it is an internal calculation.");
        }

        ActorMessagesEnqueuedTimeEnd = enqueuedTimeEnd;
        OrchestrationState = CalculationOrchestrationState.ActorMessagesEnqueued;
    }

    public void MarkAsActorMessagesEnqueuingFailed()
    {
        if (IsInternalCalculation)
        {
            throw new BusinessValidationException(
                $"Calculation with ID '{Id}' is not allowed to be marked as '{CalculationOrchestrationState.ActorMessagesEnqueuingFailed}' because it is an internal calculation.");
        }

        OrchestrationState = CalculationOrchestrationState.ActorMessagesEnqueuingFailed;
    }

    public void MarkAsCompleted(Instant completedAt)
    {
        if (OrchestrationState == CalculationOrchestrationState.Calculated
            && IsInternalCalculation == false)
        {
            throw new BusinessValidationException(
                $"Calculation with ID '{Id}' cannot be marked as '{CalculationOrchestrationState.Completed}' because it is not an internal calculation. Current orchestration state: '{OrchestrationState}'.");
        }

        OrchestrationState = CalculationOrchestrationState.Completed;
        CompletedTime = completedAt;
    }

    public void MarkAsCanceled(Guid canceledByUserId)
    {
        CanceledByUserId = canceledByUserId;
        OrchestrationState = CalculationOrchestrationState.Canceled;
    }

    /// <summary>
    /// Reset a <see cref="Calculation"/>. This will ensure that it will be picked up and run again in a new calculation.
    /// </summary>
    public void Restart()
    {
        if (ExecutionState is CalculationExecutionState.Completed)
            ThrowInvalidStateTransitionException(ExecutionState, CalculationExecutionState.Created);

        ExecutionState = CalculationExecutionState.Created;
        OrchestrationState = CalculationOrchestrationState.Started;
    }

    public bool CanCancel()
    {
        return OrchestrationState is CalculationOrchestrationState.Scheduled;
    }

    private void ThrowInvalidStateTransitionException(CalculationExecutionState currentState, CalculationExecutionState desiredState)
    {
        throw new BusinessValidationException($"Cannot change {nameof(CalculationExecutionState)} from {currentState} to {desiredState}");
    }

    private void ThrowInvalidStateTransitionException(CalculationOrchestrationState currentState, CalculationOrchestrationState desiredState)
    {
        throw new BusinessValidationException($"Cannot change {nameof(CalculationOrchestrationState)} from {currentState} to {desiredState}");
    }
}
