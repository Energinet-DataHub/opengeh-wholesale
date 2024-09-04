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

namespace Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Configuration;

/// <summary>
/// Configuration necessary to use the shared Service Bus.
/// </summary>
public sealed class ServiceBusConfiguration
{
    private ServiceBusConfiguration(
        string fullyQualifiedNamespace,
        string subsystemRelayTopicName,
        string wholesaleInboxQueueName)
    {
        if (string.IsNullOrWhiteSpace(fullyQualifiedNamespace))
            throw new ArgumentException("Cannot be null or whitespace.", nameof(fullyQualifiedNamespace));
        if (string.IsNullOrWhiteSpace(subsystemRelayTopicName))
            throw new ArgumentException("Cannot be null or whitespace.", nameof(subsystemRelayTopicName));
        if (string.IsNullOrWhiteSpace(wholesaleInboxQueueName))
            throw new ArgumentException("Cannot be null or whitespace.", nameof(wholesaleInboxQueueName));

        FullyQualifiedNamespace = fullyQualifiedNamespace;
        SubsystemRelayTopicName = subsystemRelayTopicName;
        WholesaleInboxQueueName = wholesaleInboxQueueName;
    }

    /// <summary>
    /// Fully qualified namespace for the shared service bus.
    /// </summary>
    public string FullyQualifiedNamespace { get; }

    /// <summary>
    /// Service bus topic name for the subsystem relay messages (integration events).
    /// </summary>
    public string SubsystemRelayTopicName { get; internal set; }

    public string WholesaleInboxQueueName { get; set; }

    /// <summary>
    /// Retrieve secrets from Key Vaults and create configuration.
    /// </summary>
    /// <param name="secretsConfiguration">A configuration that has been built so it can retrieve secrets from the shared key vault.</param>
    public static ServiceBusConfiguration CreateFromConfiguration(IConfigurationRoot secretsConfiguration)
    {
        return new ServiceBusConfiguration(
            secretsConfiguration.GetValue<string>("sb-domain-relay-namespace-endpoint")!,
            secretsConfiguration.GetValue<string>("sbt-shres-integrationevent-received-name")!,
            secretsConfiguration.GetValue<string>("sbq-wholesale-inbox-messagequeue-name")!);
    }
}
