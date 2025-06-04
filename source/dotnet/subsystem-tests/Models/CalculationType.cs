namespace Energinet.DataHub.Wholesale.Common.Interfaces.Models;

/// <summary>
/// Defines the wholesale calculation type
/// </summary>
public enum CalculationType
{
    /// <summary>
    /// Balance fixing
    /// </summary>
    BalanceFixing = 0,

    /// <summary>
    /// Aggregation.
    /// </summary>
    Aggregation = 1,

    /// <summary>
    /// Wholesale fixing.
    /// </summary>
    WholesaleFixing = 2,

    /// <summary>
    /// First correction settlement.
    /// </summary>
    FirstCorrectionSettlement = 3,

    /// <summary>
    /// Second correction settlement.
    /// </summary>
    SecondCorrectionSettlement = 4,

    /// <summary>
    /// Third correction settlement.
    /// </summary>
    ThirdCorrectionSettlement = 5,
}
