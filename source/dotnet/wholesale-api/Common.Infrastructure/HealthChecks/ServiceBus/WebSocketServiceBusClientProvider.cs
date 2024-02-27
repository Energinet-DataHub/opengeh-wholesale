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

using Azure.Core;
using Azure.Messaging.ServiceBus;
using HealthChecks.AzureServiceBus;

namespace Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks.ServiceBus;

/// <summary>
/// Allow us to use the transport type AWMP over WebSocket when communicating with ServiceBus.
/// </summary>
internal class WebSocketServiceBusClientProvider : ServiceBusClientProvider
{
    public override ServiceBusClient CreateClient(string? connectionString)
    {
        return new ServiceBusClient(connectionString, new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets });
    }

    public override ServiceBusClient CreateClient(string? fullyQualifiedName, TokenCredential credential)
    {
        return new ServiceBusClient(fullyQualifiedName, credential, new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets });
    }
}
