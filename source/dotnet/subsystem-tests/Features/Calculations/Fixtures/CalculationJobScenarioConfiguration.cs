using Energinet.DataHub.Core.TestCommon.Xunit.Configuration;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Configuration;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Extensions;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Calculations.Fixtures;

/// <summary>
/// Responsible for retrieving settings necessary for performing performance tests of 'CalculationJob' in Databricks.
///
/// On developer machines we use the 'subsystemtest.local.settings.json' to set values.
/// On hosted agents we must set these using environment variables.
/// </summary>
public class CalculationJobScenarioConfiguration : SubsystemTestConfiguration
{
    public CalculationJobScenarioConfiguration()
    {
        var secretsConfiguration = Root.BuildSecretsConfiguration();
        DatabricksWorkspace = DatabricksWorkspaceConfiguration.CreateFromConfiguration(secretsConfiguration);
        LogAnalyticsWorkspaceId = secretsConfiguration.GetValue<string>("log-shared-workspace-id")!;
        DatabricksCatalogName = Root.GetValue<string>("DATABRICKS_CATALOG_NAME")!;
    }

    /// <summary>
    /// Settings necessary to manage the Databricks workspace.
    /// </summary>
    public DatabricksWorkspaceConfiguration DatabricksWorkspace { get; }

    /// <summary>
    /// Setting necessary to use the shared Log Analytics workspace.
    /// </summary>
    public string LogAnalyticsWorkspaceId { get; }

    /// <summary>
    /// Setting necessary for specifying the Databricks test catalog.
    /// </summary>
    public string DatabricksCatalogName { get; }
}
