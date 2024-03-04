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
using Energinet.DataHub.Wholesale.WebApi.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.WebApi.Extensions.Options;
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
        services.AddHealthChecksForWebApp();

        // Shared by modules
        services.AddNodaTimeForApplication(Configuration);
        services.AddDatabricksJobsForApplication(Configuration);

        // Modules
        services.AddCalculationsModule(Configuration);
        services.AddCalculationResultsModule(Configuration);
        services.AddEventsModule(Configuration);
        services.AddEdiModule();

        services
            .AddControllers(options => options.Filters.Add<BusinessValidationExceptionFilter>())
            .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

        // Open API generation
        // The following article is a good read and shows many of the pieces that we have used: https://medium.com/@mo.esmp/api-versioning-and-swagger-in-asp-net-core-7-0-fe45f67d8419
        // TODO: EDI does this differently but the following has some similar parts: https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-8.0&tabs=visual-studio#xml-comments
        services.ConfigureOptions<ConfigureSwaggerOptions>();
        services.AddSwaggerGen(options =>
        {
            options.SupportNonNullableReferenceTypes();

            // Set the comments path for the Swagger JSON and UI.
            // See: https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-8.0&tabs=visual-studio#xml-comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

            var securitySchema = new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer", },
            };
            options.AddSecurityDefinition("Bearer", securitySchema);

            // TODO: EDI does this differently and so does: https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-8.0&tabs=visual-studio#xml-comments
            var securityRequirement = new OpenApiSecurityRequirement { { securitySchema, new[] { "Bearer" } }, };
            options.AddSecurityRequirement(securityRequirement);

            // TODO: Wholesale specific
            // Support binary content, e.g. for Settlement download
            options.OperationFilter<BinaryContentFilter>();
        });

        // API versioning
        services
            .AddApiVersioning(options =>
            {
                // If client doesn't specify version, we assume the following default
                options.DefaultApiVersion = new ApiVersion(3, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                // API version format strings: https://github.com/dotnet/aspnet-api-versioning/wiki/Version-Format#custom-api-version-format-strings
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

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

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var apiVersionDescriptionProvider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

            // Reverse the APIs in order to make the latest API versions appear first in select box in UI
            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse())
            {
                // GroupName is the version (e.g. 'v1') as configured using the AddApiExplorer and the 'GroupNameFormat' property.
                options.SwaggerEndpoint(
                    url: $"/swagger/{description.GroupName}/swagger.json",
                    name: description.GroupName.ToUpperInvariant());
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
}
