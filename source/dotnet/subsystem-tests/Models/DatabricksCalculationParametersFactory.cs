using Microsoft.Azure.Databricks.Client.Models;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Models;

public class DatabricksCalculationParametersFactory
{
    public RunParameters CreateParameters(Calculation calculation)
    {
        var gridAreas = string.Join(", ", calculation.GridAreaCodes.Select(c => c.Code));

        var jobParameters = new List<string>
        {
            $"--calculation-id={calculation.Id}",
            $"--grid-areas=[{gridAreas}]",
            $"--period-start-datetime={calculation.PeriodStart}",
            $"--period-end-datetime={calculation.PeriodEnd}",
            $"--calculation-type={ToJobParameterValue(calculation.CalculationType)}",
            $"--created-by-user-id={calculation.CreatedByUserId}",
        };
        if (calculation.IsInternalCalculation)
        {
            jobParameters.Add("--is-internal-calculation");
        }

        return RunParameters.CreatePythonParams(jobParameters);
    }

    private static string ToJobParameterValue(CalculationType calculationType)
    {
        return calculationType switch
        {
            CalculationType.BalanceFixing => "balance_fixing",
            CalculationType.Aggregation => "aggregation",
            CalculationType.WholesaleFixing => "wholesale_fixing",
            CalculationType.FirstCorrectionSettlement => "first_correction_settlement",
            CalculationType.SecondCorrectionSettlement => "second_correction_settlement",
            CalculationType.ThirdCorrectionSettlement => "third_correction_settlement",

            _ => throw new ArgumentOutOfRangeException(
                nameof(calculationType),
                actualValue: calculationType,
                "Value cannot be mapped to a string representation of a calculation type."),
        };
    }
}
