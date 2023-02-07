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
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.App.WebApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Wholesale.Infrastructure.Core;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.WebApi.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddApiVersioning(config =>
        {
            config.DefaultApiVersion = new ApiVersion(1, 0);
            config.AssumeDefaultVersionWhenUnspecified = true;
            config.ReportApiVersions = true;
        });

        services.AddJwtTokenSecurity();
        services.AddCommandStack(Configuration);
        services.AddApplicationInsightsTelemetry();
        RegisterCorrelationContext(services);
        ConfigureHealthChecks(services);
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();

        // Configure the HTTP request pipeline.
        if (Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers().RequireAuthorization();

            // Health check
            endpoints.MapLiveHealthChecks();
            endpoints.MapReadyHealthChecks();
        });
    }

    private static void ConfigureHealthChecks(IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddLiveCheck()
            .AddDbContextCheck<DatabaseContext>(name: "SqlDatabaseContextCheck")
            // This ought to be a Data Lake (gen 2) file system check.
            // It is, however, not easily tested so for now we stick with testing resource existence
            // and connectivity through the lesser blob storage API.
            .AddBlobStorageContainerCheck(
                EnvironmentSettingNames.CalculationStorageConnectionString.Val(),
                EnvironmentSettingNames.CalculationStorageContainerName.Val());
    }

    /// <summary>
    /// The middleware to handle properly set a CorrelationContext is only supported for Functions.
    /// This registry will ensure a new CorrelationContext (with a new Id) is set for each session
    /// </summary>
    private static void RegisterCorrelationContext(IServiceCollection services)
    {
        var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(ICorrelationContext));
        services.Remove(serviceDescriptor!);
        services.AddScoped<ICorrelationContext>(_ =>
        {
            var correlationContext = new CorrelationContext();
            correlationContext.SetId(Guid.NewGuid().ToString());
            return correlationContext;
        });
    }
}
