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

namespace Energinet.DataHub.Wholesale.SubsystemTests.Performance.Fixtures
{
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
        }

        /// <summary>
        /// Settings necessary to manage the Databricks workspace.
        /// </summary>
        public DatabricksWorkspaceConfiguration DatabricksWorkspace { get; }
    }
}
