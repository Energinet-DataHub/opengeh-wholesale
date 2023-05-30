﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.App.WebApp.Authentication;
using Energinet.DataHub.Core.App.WebApp.Authorization;
using Energinet.DataHub.Core.App.WebApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Wholesale.Common.Security;
using Energinet.DataHub.Wholesale.Components.DatabricksClient;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.WebApi.Configuration;
using Energinet.DataHub.Wholesale.WebApi.Configuration.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace Energinet.DataHub.Wholesale.WebApi;

public class Startup
{
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

        serviceCollection.AddApiVersioning(config =>
        {
            config.DefaultApiVersion = new ApiVersion(3, 0);
            config.AssumeDefaultVersionWhenUnspecified = true;
            config.ReportApiVersions = true;
        });

        serviceCollection.AddVersionedApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });
        serviceCollection.ConfigureOptions<ConfigureSwaggerOptions>();

        // Options
        serviceCollection.AddOptions<JwtOptions>().Bind(Configuration);
        serviceCollection.AddOptions<ServiceBusOptions>().Bind(Configuration);
        serviceCollection.AddOptions<DatabricksOptions>().Bind(Configuration);
        serviceCollection.AddOptions<DateTimeOptions>().Bind(Configuration);
        serviceCollection.AddOptions<DataLakeOptions>().Bind(Configuration);

        AddJwtTokenSecurity(serviceCollection);
        AddHealthCheck(serviceCollection);
        serviceCollection.AddApplicationInsightsTelemetry();
        AddCorrelationContext(serviceCollection);

        serviceCollection.AddUserAuthentication<FrontendUser, FrontendUserProvider>();
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
        var dataLakeOptions = Configuration.Get<DataLakeOptions>()!;
        serviceCollection.AddHealthChecks()
            .AddLiveCheck()
            .AddDbContextCheck<IntegrationEventPublishingDatabaseContext>(name: "SqlDatabaseContextCheck")
            .AddDataLakeContainerCheck(dataLakeOptions.STORAGE_ACCOUNT_URI, dataLakeOptions.STORAGE_CONTAINER_NAME)
            .AddAzureServiceBusTopic(
                serviceBusOptions.SERVICE_BUS_MANAGE_CONNECTION_STRING,
                serviceBusOptions.INTEGRATIONEVENTS_TOPIC_NAME,
                name: "IntegrationEventsTopicExists");
    }

    /// <summary>
    /// The middleware to handle properly set a CorrelationContext is only supported for Functions.
    /// This registry will ensure a new CorrelationContext (with a new Id) is set for each session
    /// </summary>
    private static void AddCorrelationContext(IServiceCollection serviceCollection)
    {
        var serviceDescriptor =
            serviceCollection.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(ICorrelationContext));
        serviceCollection.Remove(serviceDescriptor!);
        serviceCollection.AddScoped<ICorrelationContext>(_ =>
        {
            var correlationContext = new CorrelationContext();
            correlationContext.SetId(Guid.NewGuid().ToString());
            return correlationContext;
        });
    }
}
