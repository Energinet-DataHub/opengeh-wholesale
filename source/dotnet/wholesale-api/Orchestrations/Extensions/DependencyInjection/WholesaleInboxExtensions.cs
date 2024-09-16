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

using Azure.Identity;
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Builder;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.Options;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Events.Interfaces;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.WholesaleInbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.Orchestrations.Extensions.DependencyInjection;

public static class WholesaleInboxExtensions
{
    public static IServiceCollection AddInboxSubscription(this IServiceCollection services)
    {
        services.AddWholesaleInboxHandler();

        services
            .AddOptions<WholesaleInboxQueueOptions>()
            .BindConfiguration(WholesaleInboxQueueOptions.SectionName)
            .ValidateDataAnnotations();

        // Health checks
        var defaultAzureCredential = new DefaultAzureCredential();

        services
            .AddHealthChecks()
            .AddAzureServiceBusQueue(
                sp => sp.GetRequiredService<IOptions<ServiceBusNamespaceOptions>>().Value.FullyQualifiedNamespace,
                sp => sp.GetRequiredService<IOptions<WholesaleInboxQueueOptions>>().Value.QueueName,
                _ => defaultAzureCredential,
                name: "WholesaleInboxQueue")
            .AddServiceBusQueueDeadLetter(
                sp => sp.GetRequiredService<IOptions<ServiceBusNamespaceOptions>>().Value.FullyQualifiedNamespace,
                sp => sp.GetRequiredService<IOptions<WholesaleInboxQueueOptions>>().Value.QueueName,
                _ => defaultAzureCredential,
                "Dead-letter (wholesale inbox)",
                [HealthChecksConstants.StatusHealthCheckTag]);

        return services;
    }

    public static IServiceCollection AddCalculationOrchestrationInboxRequestHandler(this IServiceCollection services)
    {
        // Orchestration Wholesale inbox request handlers
        services.AddScoped<IWholesaleInboxRequestHandler, ActorMessagesEnqueuedV1RequestHandler>();

        // Durable task client injection
        // Must be registered as scoped, since it needs to share its state throughout a functions scope
        services.AddScoped<DurableTaskClientAccessor>();

        return services;
    }
}
