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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.Options;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// that allow adding ServiceBus services to an application.
/// </summary>
public static class ServiceBusExtensions
{
    /// <summary>
    /// Register ServiceBus services commonly used by DH3 applications.
    /// </summary>
    public static IServiceCollection AddServiceBusClientForApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<ServiceBusNamespaceOptions>()
            .BindConfiguration(ServiceBusNamespaceOptions.SectionName)
            .ValidateDataAnnotations();

        services.AddAzureClients(builder =>
        {
            var serviceBusNamespaceOptions = configuration
                .GetRequiredSection(ServiceBusNamespaceOptions.SectionName)
                .Get<ServiceBusNamespaceOptions>();

            builder
                .AddServiceBusClient(serviceBusNamespaceOptions!.ConnectionString)
                .ConfigureOptions(options =>
                {
                    options.TransportType = ServiceBusTransportType.AmqpWebSockets;
                });
        });

        return services;
    }
}
