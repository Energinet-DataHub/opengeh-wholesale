using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Microsoft.Azure.Databricks.Client.Models;

namespace Energinet.DataHub.Wholesale.Calculations.Infrastructure.Calculations;

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
            $"--calculation-type={CalculationTypeMapper.ToDeltaTableValue(calculation.CalculationType)}",
            $"--created-by-user-id={calculation.CreatedByUserId}",
        };
        if (calculation.IsInternalCalculation)
        {
            jobParameters.Add("--is-internal-calculation");
        }

        return RunParameters.CreatePythonParams(jobParameters);
    }
}
