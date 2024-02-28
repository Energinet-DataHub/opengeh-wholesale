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

using System.Reflection;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
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
using Energinet.DataHub.Wholesale.WebApi.Configuration.Modules;
using Energinet.DataHub.Wholesale.WebApi.Configuration.Options;
using Energinet.DataHub.Wholesale.WebApi.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.OpenApi.Models;

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

        // Shared by modules
        services.AddNodaTimeForApplication(Configuration);
        services.AddDatabricksJobsForApplication(Configuration);

        // Modules
        services.AddCalculationsModule(Configuration);
        services.AddCalculationResultsModule(Configuration);
        services.AddEventsModule(Configuration);
        services.AddEdiModule();

        services.AddHttpContextAccessor();

        services.AddControllers(options => options.Filters.Add<BusinessValidationExceptionFilter>()).AddJsonOptions(
            options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

        services.AddEndpointsApiExplorer();
        // Register the Swagger generator, defining 1 or more Swagger documents.
        services.AddSwaggerGen(config =>
        {
            config.SupportNonNullableReferenceTypes();
            config.OperationFilter<BinaryContentFilter>();

            // Set the comments path for the Swagger JSON and UI.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            config.IncludeXmlComments(xmlPath);

            var securitySchema = new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer", },
            };

            config.AddSecurityDefinition("Bearer", securitySchema);

            var securityRequirement = new OpenApiSecurityRequirement { { securitySchema, new[] { "Bearer" } }, };

            config.AddSecurityRequirement(securityRequirement);
        });

        var apiVersioningBuilder = services.AddApiVersioning(config =>
        {
            config.DefaultApiVersion = new ApiVersion(3, 0);
            config.AssumeDefaultVersionWhenUnspecified = true;
            config.ReportApiVersions = true;
        });
        apiVersioningBuilder.AddApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });
        services.ConfigureOptions<ConfigureSwaggerOptions>();

        // Options
        services.AddOptions<JwtOptions>().Bind(Configuration);
        services.AddOptions<ServiceBusOptions>().Bind(Configuration);
        services.AddOptions<DateTimeOptions>().Bind(Configuration);
        services.AddOptions<DataLakeOptions>().Bind(Configuration);
        services.AddOptions<DeltaTableOptions>();

        // ServiceBus
        services.AddAzureClients(builder =>
        {
            builder
                .AddServiceBusClient(Configuration.Get<ServiceBusOptions>()!.SERVICE_BUS_TRANCEIVER_CONNECTION_STRING)
                .ConfigureOptions(options =>
                {
                    options.TransportType = ServiceBusTransportType.AmqpWebSockets;
                });
        });

        AddJwtTokenSecurity(services);
        AddHealthCheck(services);

        services.AddUserAuthentication<FrontendUser, FrontendUserProvider>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
    {
        app.UseRouting();

        // Configure the HTTP request pipeline.
        if (environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger();

        var apiVersionDescriptionProvider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
        app.UseSwaggerUI(options =>
        {
            // Reverse the APIs in order to make the latest API versions appear first in select box in UI
            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse())
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            }
        });

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

    /// <summary>
    /// Adds registrations of JwtTokenMiddleware and corresponding dependencies.
    /// </summary>
    private void AddJwtTokenSecurity(IServiceCollection services)
    {
        var options = Configuration.Get<JwtOptions>()!;
        services.AddJwtBearerAuthentication(options.EXTERNAL_OPEN_ID_URL, options.INTERNAL_OPEN_ID_URL, options.BACKEND_BFF_APP_ID);
        services.AddPermissionAuthorization();
    }

    private void AddHealthCheck(IServiceCollection services)
    {
        var serviceBusOptions = Configuration.Get<ServiceBusOptions>()!;
        services.AddHealthChecks()
            .AddLiveCheck()
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
}
