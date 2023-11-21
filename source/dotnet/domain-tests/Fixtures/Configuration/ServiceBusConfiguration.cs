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

using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures.Configuration
{
    /// <summary>
    /// Configuration necessary to use the shared Service Bus.
    /// </summary>
    public sealed class ServiceBusConfiguration
    {
        private ServiceBusConfiguration(string fullyQualifiedNamespace, string connectionString, string domainRelayTopicName)
        {
            if (string.IsNullOrWhiteSpace(fullyQualifiedNamespace))
                throw new ArgumentException("Cannot be null or whitespace.", nameof(fullyQualifiedNamespace));
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Cannot be null or whitespace.", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(domainRelayTopicName))
                throw new ArgumentException("Cannot be null or whitespace.", nameof(domainRelayTopicName));

            FullyQualifiedNamespace = fullyQualifiedNamespace;
            ConnectionString = connectionString;
            DomainRelayTopicName = domainRelayTopicName;
        }

        /// <summary>
        /// Fully qualified namespace for the shared service bus.
        /// </summary>
        public string FullyQualifiedNamespace { get; }

        /// <summary>
        /// Connection string for the shared service bus.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Service bus topic name for the domain relay messages (integration events).
        /// </summary>
        public string DomainRelayTopicName { get; internal set; }

        /// <summary>
        /// Retrieve secrets from Key Vaults and create configuration.
        /// </summary>
        /// <param name="secretsConfiguration">A configuration that has been builded so it can retrieve secrets from the shared key vault.</param>
        public static ServiceBusConfiguration CreateFromConfiguration(IConfigurationRoot secretsConfiguration)
        {
            var serviceBusNamespace = secretsConfiguration.GetValue<string>("sb-domain-relay-namespace-name")!;

            return new ServiceBusConfiguration(
                $"{serviceBusNamespace}.servicebus.windows.net",
                secretsConfiguration.GetValue<string>("sb-domain-relay-listen-connection-string")!,
                secretsConfiguration.GetValue<string>("sbt-shres-integrationevent-received-name")!);
        }
    }
}
