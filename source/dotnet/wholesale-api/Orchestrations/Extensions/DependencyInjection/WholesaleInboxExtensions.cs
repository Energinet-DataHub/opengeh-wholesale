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

using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.Options;
using Energinet.DataHub.Wholesale.Edi.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.Orchestrations.Extensions.DependencyInjection;

public static class WholesaleInboxExtensions
{
    public static IServiceCollection AddWholesaleInboxHandling(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<WholesaleInboxQueueOptions>()
            .BindConfiguration(WholesaleInboxQueueOptions.SectionName)
            .ValidateDataAnnotations();

        // Health checks
        services.AddHealthChecks()
            // Must use a listener connection string
            .AddAzureServiceBusQueue(
                sp => sp.GetRequiredService<IOptions<ServiceBusNamespaceOptions>>().Value.ConnectionString,
                sp => sp.GetRequiredService<IOptions<WholesaleInboxQueueOptions>>().Value.QueueName,
                name: "WholesaleInboxQueue");

        services.AddWholesaleInboxHandler();
        services.AddEdiModule(); // Edi module has Wholesale inbox handlers for requests from EDI
        services.AddCalculationsModule(configuration); // Calculations module is used by EDI
        services.AddServiceBusClientForApplication(configuration); // Service bus client is used by EDI

        return services;
    }
}
