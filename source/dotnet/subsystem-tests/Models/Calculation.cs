using NodaTime;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Models;

public class Calculation
{
    private readonly List<GridAreaCode> _gridAreaCodes;

    public Calculation(
        CalculationType calculationType,
        List<GridAreaCode> gridAreaCodes,
        Instant periodStart,
        Instant periodEnd,
        Guid createdByUserId,
        bool isInternalCalculation)
    {
        Id = Guid.NewGuid();
        _gridAreaCodes = gridAreaCodes.ToList();
        CalculationType = calculationType;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        CreatedByUserId = createdByUserId;
        IsInternalCalculation = isInternalCalculation;
    }

    public Guid Id { get; }

    public CalculationType CalculationType { get; }

    public IReadOnlyCollection<GridAreaCode> GridAreaCodes => _gridAreaCodes;

    public Guid CreatedByUserId { get; }

    /// <summary>
    /// Must be exactly at the beginning (at 00:00:00 o'clock) of the local date.
    /// </summary>
    public Instant PeriodStart { get; private set; }

    /// <summary>
    /// Must be exactly 1 ms before the end (midnight) of the local date.
    /// The 1 ms off is by design originating from the front-end decision on how to handle date periods.
    /// </summary>
    public Instant PeriodEnd { get; }

    /// <summary>
    /// When a calculation is internal, the calculation result is for internal use only.
    /// Results should not be exposed to external users/actors.
    /// </summary>
    public bool IsInternalCalculation { get; }
}
