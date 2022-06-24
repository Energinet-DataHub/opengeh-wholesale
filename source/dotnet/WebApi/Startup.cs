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

using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.WebApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.WebApp.Middleware;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.WebApi.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.Wholesale.WebApi;

public sealed class Startup : Apps.Core.StartupBase
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            // This middleware has to be configured after 'UseRouting' for 'AllowAnonymousAttribute' to work.
            // We dont want JwtToken when running locally
            app.UseMiddleware<JwtTokenMiddleware>();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();

            // Health check
            endpoints.MapLiveHealthChecks();
            endpoints.MapReadyHealthChecks();
        });
    }

    public void ConfigureServices(IServiceCollection services)
    {
        Initialize(_configuration, services);
    }

    protected override void Configure(IConfiguration configuration, IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddJwtTokenSecurity(configuration);
        services.AddApiVersioning(config =>
        {
            config.DefaultApiVersion = new ApiVersion(1, 0);
            config.AssumeDefaultVersionWhenUnspecified = true;
            config.ReportApiVersions = true;
        });

        HealthCheck(services);
    }

    private static void HealthCheck(IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddLiveCheck()
            .AddDbContextCheck<DatabaseContext>(name: "SqlDatabaseContextCheck");
    }
}
