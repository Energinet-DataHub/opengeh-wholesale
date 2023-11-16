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
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.Identity;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures.Configuration
{
    /// <summary>
    /// Responsible for retrieving settings necessary for performing domain tests of 'Wholesale'.
    ///
    /// On developer machines we use the 'domaintest.local.settings.json' to set values.
    /// On hosted agents we must set these using environment variables.
    /// </summary>
    public class WholesaleDomainConfiguration : DomainTestConfiguration
    {
        public WholesaleDomainConfiguration()
        {
            UserTokenConfiguration = B2CUserTokenConfiguration.CreateFromConfiguration(Root);
            WebApiBaseAddress = new Uri(Root.GetValue<string>("WEBAPI_BASEADDRESS")!);

            var secretsConfiguration = BuildSecretsConfiguration(Root);
            var serviceBusNamespace = secretsConfiguration.GetValue<string>("sb-domain-relay-namespace-name")!;
            ServiceBusFullyQualifiedNamespace = $"{serviceBusNamespace}.servicebus.windows.net";
            ServiceBusConnectionString = secretsConfiguration.GetValue<string>("sb-domain-relay-listen-connection-string")!;
            DomainRelayTopicName = secretsConfiguration.GetValue<string>("sbt-shres-integrationevent-received-name")!;

            DatabricksWorkspace = DatabricksWorkspaceConfiguration.CreateFromConfiguration(Root);
        }

        /// <summary>
        /// Settings necessary to retrieve a user token for authentication with Wholesale Web API in live environment.
        /// </summary>
        public B2CUserTokenConfiguration UserTokenConfiguration { get; }

        /// <summary>
        /// Base address setting for Wholesale Web API in live environment.
        /// </summary>
        public Uri WebApiBaseAddress { get; }

        /// <summary>
        /// Fully qualified namespace for the service bus used for domain relay.
        /// </summary>
        public string ServiceBusFullyQualifiedNamespace { get; }

        /// <summary>
        /// Connection string for the service bus used for domain relay.
        /// </summary>
        public string ServiceBusConnectionString { get; }

        /// <summary>
        /// Service bus topic name for the domain relay messages.
        /// </summary>
        public string DomainRelayTopicName { get; internal set; }

        /// <summary>
        /// Settings necessary to start the Databricks workspace SQL warehouse.
        /// </summary>
        public DatabricksWorkspaceConfiguration DatabricksWorkspace { get; }

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
