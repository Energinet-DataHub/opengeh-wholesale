using Energinet.DataHub.Wholesale.Common.Interfaces.Models;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;

public static class CalculationTypeMapper
{
    public static string ToDeltaTableValue(CalculationType calculationType)
    {
        return calculationType switch
        {
            CalculationType.BalanceFixing => DeltaTableConstants.DeltaTableCalculationType.BalanceFixing,
            CalculationType.Aggregation => DeltaTableConstants.DeltaTableCalculationType.Aggregation,
            CalculationType.WholesaleFixing => DeltaTableConstants.DeltaTableCalculationType.WholesaleFixing,
            CalculationType.FirstCorrectionSettlement => DeltaTableConstants.DeltaTableCalculationType.FirstCorrectionSettlement,
            CalculationType.SecondCorrectionSettlement => DeltaTableConstants.DeltaTableCalculationType.SecondCorrectionSettlement,
            CalculationType.ThirdCorrectionSettlement => DeltaTableConstants.DeltaTableCalculationType.ThirdCorrectionSettlement,

            _ => throw new ArgumentOutOfRangeException(
                nameof(calculationType),
                actualValue: calculationType,
                "Value cannot be mapped to a string representation of a calculation type."),
        };
    }
}
