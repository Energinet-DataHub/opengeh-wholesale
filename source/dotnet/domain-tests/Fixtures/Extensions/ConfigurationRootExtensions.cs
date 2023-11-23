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
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures.Extensions
{
    public static class ConfigurationRootExtensions
    {
        /// <summary>
        /// Build configuration for loading settings from key vault secrets.
        /// </summary>
        public static IConfigurationRoot BuildSecretsConfiguration(this IConfigurationRoot root)
        {
            var sharedKeyVaultName = root.GetValue<string>("SHARED_KEYVAULT_NAME");
            var sharedKeyVaultUrl = $"https://{sharedKeyVaultName}.vault.azure.net/";

            var internalKeyVaultName = root.GetValue<string>("INTERNAL_KEYVAULT_NAME");
            var internalKeyVaultUrl = $"https://{internalKeyVaultName}.vault.azure.net/";

            return new ConfigurationBuilder()
                .AddAuthenticatedAzureKeyVault(sharedKeyVaultUrl)
                .AddAuthenticatedAzureKeyVault(internalKeyVaultUrl)
                .Build();
        }
    }
}
