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
using Asp.Versioning.ApiExplorer;
using Energinet.DataHub.Wholesale.WebApi.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace Energinet.DataHub.Wholesale.WebApi.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// that allow adding Open API services to an ASP.NET Core app.
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Register services necessary for enabling an ASP.NET Core app
    /// to generate Open API specifications and work with Swagger UI.
    /// </summary>
    public static IServiceCollection AddSwaggerForWebApplication(this IServiceCollection services)
    {
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

        return services;
    }

    /// <summary>
    /// Register services necessary for enabling an ASP.NET Core app
    /// to generate Open API specifications and work with Swagger UI.
    /// </summary>
    public static IApplicationBuilder UseSwaggerForWebApplication(this IApplicationBuilder app)
    {
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

        return app;
    }
}
