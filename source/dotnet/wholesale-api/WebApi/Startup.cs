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
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.WebApp.Authentication;
using Energinet.DataHub.Core.App.WebApp.Authorization;
using Energinet.DataHub.Core.App.WebApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.Databricks.Jobs.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.Logging.LoggingMiddleware;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Security;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.WebApi.Configuration;
using Energinet.DataHub.Wholesale.WebApi.Configuration.Options;
using Energinet.DataHub.Wholesale.WebApi.HealthChecks;
using Energinet.DataHub.Wholesale.WebApi.HealthChecks.DataLake;
using Microsoft.Extensions.Azure;
using Microsoft.OpenApi.Models;

namespace Energinet.DataHub.Wholesale.WebApi;

public class Startup
{
    private const string DomainName = "wholesale";

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
    }

    public IConfiguration Configuration { get; }

    public IWebHostEnvironment Environment { get; }

    public void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddModules(Configuration);
        serviceCollection.AddHttpContextAccessor();

        serviceCollection.AddControllers(options => options.Filters.Add<BusinessValidationExceptionFilter>()).AddJsonOptions(
            options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

        serviceCollection.AddEndpointsApiExplorer();
        // Register the Swagger generator, defining 1 or more Swagger documents.
        serviceCollection.AddSwaggerGen(config =>
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

        var apiVersioningBuilder = serviceCollection.AddApiVersioning(config =>
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
        serviceCollection.ConfigureOptions<ConfigureSwaggerOptions>();

        // Options
        serviceCollection.AddOptions<JwtOptions>().Bind(Configuration);
        serviceCollection.AddOptions<ServiceBusOptions>().Bind(Configuration);
        serviceCollection.AddOptions<DateTimeOptions>().Bind(Configuration);
        serviceCollection.AddOptions<DataLakeOptions>().Bind(Configuration);
        serviceCollection.AddOptions<DeltaTableOptions>();

        // ServiceBus
        serviceCollection.AddAzureClients(builder =>
        {
            builder.AddServiceBusClient(Configuration.Get<ServiceBusOptions>()!.SERVICE_BUS_MANAGE_CONNECTION_STRING);
        });

        AddJwtTokenSecurity(serviceCollection);
        AddHealthCheck(serviceCollection);
        serviceCollection.AddApplicationInsightsTelemetry(options => options.EnableAdaptiveSampling = false);

        serviceCollection.AddUserAuthentication<FrontendUser, FrontendUserProvider>();
        serviceCollection.AddHttpLoggingScope(DomainName);
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();

        // Configure the HTTP request pipeline.
        if (Environment.IsDevelopment())
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

        app.UseLoggingScope();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        if (!Environment.IsEnvironment("Testing"))
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
    private void AddJwtTokenSecurity(IServiceCollection serviceCollection)
    {
        var options = Configuration.Get<JwtOptions>()!;
        serviceCollection.AddJwtBearerAuthentication(options.EXTERNAL_OPEN_ID_URL, options.INTERNAL_OPEN_ID_URL, options.BACKEND_BFF_APP_ID);
        serviceCollection.AddPermissionAuthorization();
    }

    private void AddHealthCheck(IServiceCollection serviceCollection)
    {
        var serviceBusOptions = Configuration.Get<ServiceBusOptions>()!;
        serviceCollection.AddHealthChecks()
            .AddLiveCheck()
            .AddDbContextCheck<EventsDatabaseContext>(
                name: HealthCheckNames.SqlDatabaseContext)
            .AddAzureServiceBusTopic(
                serviceBusOptions.SERVICE_BUS_MANAGE_CONNECTION_STRING,
                serviceBusOptions.INTEGRATIONEVENTS_TOPIC_NAME,
                name: HealthCheckNames.IntegrationEventsTopic)
            .AddDataLakeHealthCheck(
                _ => Configuration.Get<DataLakeOptions>()!,
                name: HealthCheckNames.DataLake)
            .AddDatabricksJobsApiHealthCheck(
                name: HealthCheckNames.DatabricksJobsApi)
            .AddDatabricksSqlStatementApiHealthCheck(
                name: HealthCheckNames.DatabricksSqlStatementsApi)
            .AddAzureServiceBusQueue(
                serviceBusOptions.SERVICE_BUS_MANAGE_CONNECTION_STRING,
                serviceBusOptions.WHOLESALE_INBOX_MESSAGE_QUEUE_NAME,
                name: HealthCheckNames.WholesaleInboxEventsQueue)
            .AddAzureServiceBusQueue(
                serviceBusOptions.SERVICE_BUS_MANAGE_CONNECTION_STRING,
                serviceBusOptions.EDI_INBOX_MESSAGE_QUEUE_NAME,
                name: HealthCheckNames.EdiInboxEventsQueue);
    }
}
