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

using Asp.Versioning.ApiExplorer;

namespace Energinet.DataHub.Wholesale.WebApi.Extensions.Builder;

/// <summary>
/// Extension methods for <see cref="IApplicationBuilder"/>
/// that allow adding Open API middleware to an ASP.NET Core app.
/// </summary>
public static class OpenApiBuilderExtensions
{
    /// <summary>
    /// Register middleware for enabling an ASP.NET Core app
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
