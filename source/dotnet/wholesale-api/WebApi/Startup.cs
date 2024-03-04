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

using System.Text.Json.Serialization;
using Asp.Versioning;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.App.WebApp.Authentication;
using Energinet.DataHub.Core.App.WebApp.Authorization;
using Energinet.DataHub.Core.App.WebApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks;
using Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks.ServiceBus;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Security;
using Energinet.DataHub.Wholesale.Edi.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.WebApi.Extensions.Builder;
using Energinet.DataHub.Wholesale.WebApi.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;

namespace Energinet.DataHub.Wholesale.WebApi;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Common
        services.AddApplicationInsightsForWebApp();
        services.AddHealthChecksForWebApp();

        // Shared by modules
        services.AddNodaTimeForApplication(Configuration);
        services.AddDatabricksJobsForApplication(Configuration);

        // Modules
        services.AddCalculationsModule(Configuration);
        services.AddCalculationResultsModule(Configuration);
        services.AddIntegrationEventsSubscription();
        services.AddInboxHandling();
        services.AddEdiModule();

        services
            .AddControllers(options => options.Filters.Add<BusinessValidationExceptionFilter>())
            .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

        // Open API generation
        services.AddSwaggerForWebApplication();

        // API versioning
        services.AddApiVersioningForWebApplication(new ApiVersion(3, 0));

        // Authentication/authorization
        services
            .AddTokenAuthenticationForWebApp(Configuration)
            .AddUserAuthenticationForWebApp<FrontendUser, FrontendUserProvider>()
            .AddPermissionAuthorization();

        // ServiceBus
        services.AddOptions<ServiceBusOptions>().Bind(Configuration);
        services.AddAzureClients(builder =>
        {
            builder
                .AddServiceBusClient(Configuration.Get<ServiceBusOptions>()!.SERVICE_BUS_TRANCEIVER_CONNECTION_STRING)
                .ConfigureOptions(options =>
                {
                    options.TransportType = ServiceBusTransportType.AmqpWebSockets;
                });
        });
        var serviceBusOptions = Configuration.Get<ServiceBusOptions>()!;
        services.AddHealthChecks()
            .AddAzureServiceBusSubscriptionUsingWebSockets(
                serviceBusOptions.SERVICE_BUS_TRANCEIVER_CONNECTION_STRING,
                serviceBusOptions.INTEGRATIONEVENTS_TOPIC_NAME,
                serviceBusOptions.INTEGRATIONEVENTS_SUBSCRIPTION_NAME,
                name: HealthCheckNames.IntegrationEventsTopicSubscription)
            .AddAzureServiceBusQueueUsingWebSockets(
                serviceBusOptions.SERVICE_BUS_TRANCEIVER_CONNECTION_STRING,
                serviceBusOptions.WHOLESALE_INBOX_MESSAGE_QUEUE_NAME,
                name: HealthCheckNames.WholesaleInboxEventsQueue)
            .AddAzureServiceBusQueueUsingWebSockets(
                serviceBusOptions.SERVICE_BUS_TRANCEIVER_CONNECTION_STRING,
                serviceBusOptions.EDI_INBOX_MESSAGE_QUEUE_NAME,
                name: HealthCheckNames.EdiInboxEventsQueue);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
    {
        app.UseRouting();

        // Configure the HTTP request pipeline.
        if (environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwaggerForWebApplication();

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        if (!environment.IsEnvironment("Testing"))
        {
            app.UseUserMiddleware<FrontendUser>();
        }

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers().RequireAuthorization();

            // Health check
            endpoints.MapLiveHealthChecks();
            endpoints.MapReadyHealthChecks();
        });
    }
}
