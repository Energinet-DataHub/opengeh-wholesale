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
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Builder;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Energinet.DataHub.Edi.Requests;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.Options;
using Energinet.DataHub.Wholesale.Edi.Client;
using Energinet.DataHub.Wholesale.Edi.Factories;
using Energinet.DataHub.Wholesale.Edi.Validation;
using Energinet.DataHub.Wholesale.Edi.Validation.AggregatedTimeSeriesRequest;
using Energinet.DataHub.Wholesale.Edi.Validation.AggregatedTimeSeriesRequest.Rules;
using Energinet.DataHub.Wholesale.Edi.Validation.Helpers;
using Energinet.DataHub.Wholesale.Edi.Validation.WholesaleServicesRequest;
using Energinet.DataHub.Wholesale.Events.Interfaces;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.Edi.Extensions.DependencyInjection;

/// <summary>
/// Registration of services required for the Calculations module.
/// </summary>
public static class EdiExtensions
{
    public static void AddEdiModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddScoped<IWholesaleInboxRequestHandler, AggregatedTimeSeriesRequestHandler>();
        services.AddScoped<IWholesaleInboxRequestHandler, WholesaleServicesRequestHandler>();
        services.AddTransient<WholesaleServicesRequestMapper>();

        services.AddSingleton<IEdiClient, EdiClient>();

        services
            .AddOptions<EdiInboxQueueOptions>()
            .BindConfiguration(EdiInboxQueueOptions.SectionName)
            .ValidateDataAnnotations();

        services.AddAzureClients(builder =>
        {
            var ediInboxQueueOptions =
                configuration
                    .GetRequiredSection(EdiInboxQueueOptions.SectionName)
                    .Get<EdiInboxQueueOptions>()
                ?? throw new InvalidOperationException("Missing EDI Inbox configuration.");

            builder
                .AddClient<ServiceBusSender, ServiceBusClientOptions>((_, _, provider) =>
                    provider
                        .GetRequiredService<ServiceBusClient>()
                        .CreateSender(ediInboxQueueOptions.QueueName))
                .WithName(ediInboxQueueOptions.QueueName);
        });

        // Health checks
        var defaultAzureCredential = new DefaultAzureCredential();

        services
            .AddHealthChecks()
            .AddAzureServiceBusQueue(
                sp => sp.GetRequiredService<IOptions<ServiceBusNamespaceOptions>>().Value.FullyQualifiedNamespace,
                sp => sp.GetRequiredService<IOptions<EdiInboxQueueOptions>>().Value.QueueName,
                _ => defaultAzureCredential,
                name: "EdiInboxQueue");

        // Validation helpers
        services.AddTransient<PeriodValidationHelper>();
        // Validation
        services.AddAggregatedTimeSeriesRequestValidation();
        services.AddWholesaleServicesRequestValidation();
    }

    public static IServiceCollection AddAggregatedTimeSeriesRequestValidation(this IServiceCollection services)
    {
        services.AddScoped<IValidator<AggregatedTimeSeriesRequest>, AggregatedTimeSeriesRequestValidator>();
        services.AddScoped<IValidationRule<AggregatedTimeSeriesRequest>, PeriodValidationRule>();
        services.AddSingleton<IValidationRule<AggregatedTimeSeriesRequest>, MeteringPointTypeValidationRule>();
        services.AddSingleton<IValidationRule<AggregatedTimeSeriesRequest>, EnergySupplierValidationRule>();
        services.AddSingleton<IValidationRule<AggregatedTimeSeriesRequest>, SettlementMethodValidationRule>();
        services.AddSingleton<IValidationRule<AggregatedTimeSeriesRequest>, TimeSeriesTypeValidationRule>();
        services.AddSingleton<IValidationRule<AggregatedTimeSeriesRequest>, BalanceResponsibleValidationRule>();
        services.AddSingleton<IValidationRule<AggregatedTimeSeriesRequest>, SettlementVersionValidationRule>();
        services.AddScoped<IValidationRule<AggregatedTimeSeriesRequest>, GridAreaValidationRule>();
        services.AddSingleton<IValidationRule<AggregatedTimeSeriesRequest>, RequestedByActorRoleValidationRule>();

        return services;
    }

    public static IServiceCollection AddWholesaleServicesRequestValidation(this IServiceCollection services)
    {
        services.AddScoped<IValidator<WholesaleServicesRequest>, WholesaleServicesRequestValidator>();
        services
            .AddSingleton<
                IValidationRule<WholesaleServicesRequest>,
                Energinet.DataHub.Wholesale.Edi.Validation.WholesaleServicesRequest.Rules.ResolutionValidationRule>()
            .AddSingleton<
                IValidationRule<WholesaleServicesRequest>,
                Energinet.DataHub.Wholesale.Edi.Validation.WholesaleServicesRequest.Rules.EnergySupplierValidationRule>()
            .AddSingleton<
                IValidationRule<WholesaleServicesRequest>,
                Energinet.DataHub.Wholesale.Edi.Validation.WholesaleServicesRequest.Rules.ChargeCodeValidationRule>()
            .AddScoped<
                IValidationRule<WholesaleServicesRequest>,
                Energinet.DataHub.Wholesale.Edi.Validation.WholesaleServicesRequest.Rules.PeriodValidationRule>()
            .AddScoped<
                IValidationRule<WholesaleServicesRequest>,
                Energinet.DataHub.Wholesale.Edi.Validation.WholesaleServicesRequest.Rules.GridAreaValidationRule>()
            .AddScoped<
                IValidationRule<WholesaleServicesRequest>,
                Energinet.DataHub.Wholesale.Edi.Validation.WholesaleServicesRequest.Rules.SettlementVersionValidationRule>();

        return services;
    }
}
