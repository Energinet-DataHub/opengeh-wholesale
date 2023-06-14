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

using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Wholesale.Common.DatabricksClient;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures
{
    /// <summary>
    /// Responsible for retrieving settings necessary for performing integration tests of 'CalculationResults'.
    ///
    /// On developer machines we use the 'integrationtest.local.settings.json' to set values.
    /// On hosted agents we must set these using environment variables.
    /// </summary>
    public class CalculationResultsConfiguration : CalculationResultsTestConfiguration
    {
        public CalculationResultsConfiguration()
        {
            var secretsConfiguration = BuildSecretsConfiguration(Root);

            // DatabricksOptions = new DatabricksOptions
            // {
            //     DATABRICKS_WAREHOUSE_ID =
            //         secretsConfiguration.GetValue<string>()!,
            //     DATABRICKS_WORKSPACE_URL = secretsConfiguration.GetValue<string>()!,
            //     DATABRICKS_WORKSPACE_TOKEN = secretsConfiguration.GetValue<string>())!,
            //     // DATABRICKS_WAREHOUSE_ID =
            //     //     secretsConfiguration.GetValue<string>(nameof(DatabricksOptions.DATABRICKS_WAREHOUSE_ID))!,
            //     // DATABRICKS_WORKSPACE_URL = secretsConfiguration.GetValue<string>(nameof(DatabricksOptions.DATABRICKS_WORKSPACE_URL))!,
            //     // DATABRICKS_WORKSPACE_TOKEN = secretsConfiguration.GetValue<string>(nameof(DatabricksOptions.DATABRICKS_WORKSPACE_TOKEN))!,
            // };
        }

        // public DatabricksOptions DatabricksOptions { get; }

        /// <summary>
        /// Load settings from key vault secrets.
        /// </summary>
        private static IConfigurationRoot BuildSecretsConfiguration(IConfigurationRoot root)
        {
            var sharedKeyVaultName = root.GetValue<string>("SHARED_KEYVAULT_NAME");
            var sharedKeyVaultUrl = $"https://{sharedKeyVaultName}.vault.azure.net/";

            return new ConfigurationBuilder()
                .AddAuthenticatedAzureKeyVault(sharedKeyVaultUrl)
                .Build();
        }
    }
}
