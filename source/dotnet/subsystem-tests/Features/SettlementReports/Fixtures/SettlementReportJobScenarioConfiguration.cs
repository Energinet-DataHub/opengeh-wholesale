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

using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Configuration;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Extensions;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Performance.Fixtures;

/// <summary>
/// Responsible for retrieving environment specific settings necessary for performing tests of 'SettlementReportJob' in Databricks.
///
/// On developer machines we use the 'subsystemtest.local.settings.json' to set values.
/// On hosted agents we must set these using environment variables.
/// </summary>
public class SettlementReportJobScenarioConfiguration : SubsystemTestConfiguration
{
    public SettlementReportJobScenarioConfiguration()
    {
        InputCalculationId = Root.GetValue<string>("SETTLEMENT_REPORT_CALCULATION_ID")!;

        var databricksCatalogName = Root.GetValue<string>("DATABRICKS_CATALOG_NAME")!;
        DatabricksCatalogRoot = $"/Volumes/{databricksCatalogName}";

        var secretsConfiguration = Root.BuildSecretsConfiguration();
        DatabricksWorkspace = DatabricksWorkspaceConfiguration.CreateFromConfiguration(secretsConfiguration);
    }

    /// <summary>
    /// Calculation ID used for input parameter when starting Settlement Report Job.
    /// </summary>
    public string InputCalculationId { get; }

    /// <summary>
    /// Databricks catalog root.
    /// </summary>
    public string DatabricksCatalogRoot { get; }

    /// <summary>
    /// Settings necessary to manage the Databricks workspace.
    /// </summary>
    public DatabricksWorkspaceConfiguration DatabricksWorkspace { get; }
}
