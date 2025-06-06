using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Microsoft.Azure.Databricks.Client.Models;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Calculations.States;

public class CalculationJobScenarioState
{
    [NotNull]
    public long? LatestCalculationVersion { get; set; }

    [NotNull]
    public Calculation? CalculationJobInput { get; set; }

    [NotNull]
    public CalculationJobId? CalculationJobId { get; set; }

    [NotNull]
    public Run? Run { get; set; }
}
