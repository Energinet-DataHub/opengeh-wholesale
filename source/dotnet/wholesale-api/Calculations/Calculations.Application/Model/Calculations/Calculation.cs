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

    public Calculation(
        Instant createdTime,
        CalculationType calculationType,
        IEnumerable<GridAreaCode> gridAreaCodes,
        Instant periodStart,
        Instant periodEnd,
        Instant executionTimeStart,
        DateTimeZone dateTimeZone,
        Guid createdByUserId,
        long version)
        : this()
    {
        _gridAreaCodes = gridAreaCodes.ToList();
        if (!IsValid(_gridAreaCodes, calculationType, periodStart, periodEnd, dateTimeZone, out var errorMessages))
            throw new BusinessValidationException(string.Join(" ", errorMessages));

        ExecutionState = CalculationExecutionState.Created;
        CalculationType = calculationType;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        ExecutionTimeStart = executionTimeStart;
        CreatedTime = createdTime;
        CreatedByUserId = createdByUserId;
        ExecutionTimeEnd = null;
        AreSettlementReportsCreated = false;
        Version = version;
    }

    /// <summary>
    /// Validate if parameters are valid for a <see cref="Calculation"/>.
    /// </summary>
    /// <param name="gridAreaCodes"></param>
    /// <param name="calculationType"></param>
    /// <param name="periodStart"></param>
    /// <param name="periodEnd"></param>
    /// <param name="dateTimeZone"></param>
    /// <param name="validationErrors"></param>
    /// <returns>If the parameters are valid for a <see cref="Calculation"/></returns>
    private static bool IsValid(
        IEnumerable<GridAreaCode> gridAreaCodes,
        CalculationType calculationType,
        Instant periodStart,
        Instant periodEnd,
        DateTimeZone dateTimeZone,
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
    }

    // Private setter is used implicitly by tests
    public Guid Id { get; private set; }

    public CalculationType CalculationType { get; }

    public IReadOnlyCollection<GridAreaCode> GridAreaCodes => _gridAreaCodes;

    public CalculationExecutionState ExecutionState { get; private set; }

    public Instant? ExecutionTimeStart { get; }

    public Instant CreatedTime { get; }

    public Guid CreatedByUserId { get; }

    public Instant? ExecutionTimeEnd { get; private set; }

    public CalculationJobId? CalculationJobId { get; private set; }

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
    /// Get the ISO 8601 duration for the given calculation type.
    /// </summary>
    public string GetResolution()
    {
        return CalculationType switch
        {
            CalculationType.BalanceFixing or CalculationType.Aggregation or CalculationType.WholesaleFixing or CalculationType.FirstCorrectionSettlement or CalculationType.SecondCorrectionSettlement or CalculationType.ThirdCorrectionSettlement => "PT15M",
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
            CalculationType.BalanceFixing or CalculationType.Aggregation or CalculationType.WholesaleFixing or CalculationType.FirstCorrectionSettlement or CalculationType.SecondCorrectionSettlement or CalculationType.ThirdCorrectionSettlement => QuantityUnit.Kwh,
            _ => throw new NotImplementedException(),
        };
    }

    public void MarkAsSubmitted(CalculationJobId calculationJobId)
    {
        ArgumentNullException.ThrowIfNull(calculationJobId);
        if (ExecutionState is CalculationExecutionState.Submitted or CalculationExecutionState.Pending
            or CalculationExecutionState.Executing or CalculationExecutionState.Completed or CalculationExecutionState.Failed)
        {
            ThrowInvalidStateTransitionException(ExecutionState, CalculationExecutionState.Submitted);
        }

        CalculationJobId = calculationJobId;
        ExecutionState = CalculationExecutionState.Submitted;
    }

    public void MarkAsPending()
    {
        if (ExecutionState is CalculationExecutionState.Pending or CalculationExecutionState.Executing or CalculationExecutionState.Completed or CalculationExecutionState.Failed)
            ThrowInvalidStateTransitionException(ExecutionState, CalculationExecutionState.Pending);
        ExecutionState = CalculationExecutionState.Pending;
    }

    public void MarkAsExecuting()
    {
        if (ExecutionState is CalculationExecutionState.Executing or CalculationExecutionState.Completed or CalculationExecutionState.Failed)
            ThrowInvalidStateTransitionException(ExecutionState, CalculationExecutionState.Executing);

        ExecutionState = CalculationExecutionState.Executing;
    }

    public void MarkAsCompleted(Instant executionTimeEnd)
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
    }

    public void MarkAsFailed()
    {
        if (ExecutionState is CalculationExecutionState.Failed)
            ThrowInvalidStateTransitionException(ExecutionState, CalculationExecutionState.Failed);

        ExecutionState = CalculationExecutionState.Failed;
    }

    /// <summary>
    /// Reset a <see cref="Calculation"/>. This will ensure that it will be picked up and run again in a new calculation.
    /// </summary>
    public void Reset()
    {
        if (ExecutionState is CalculationExecutionState.Completed)
            ThrowInvalidStateTransitionException(ExecutionState, CalculationExecutionState.Created);

        ExecutionState = CalculationExecutionState.Created;
    }

    private void ThrowInvalidStateTransitionException(CalculationExecutionState currentState, CalculationExecutionState desiredState)
    {
        throw new BusinessValidationException($"Cannot change {nameof(CalculationExecutionState)} from {currentState} to {desiredState}");
    }
}
