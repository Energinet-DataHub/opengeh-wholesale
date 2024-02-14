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
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.Common.Reflection;
using Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.Databricks.Jobs.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.Databricks.Jobs.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Calculations.Application.UseCases;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Telemetry;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Application Insights (Telemetry)
        // => Telemetry initializers only adds information to logs emitted by the isolated worker; not logs emitted by the function host.
        services.AddSingleton<ITelemetryInitializer>(new SubsystemInitializer(TelemetryConstants.SubsystemName));
        // => Configure isolated worker to emit logs directly to Application Insights.
        // See https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows#application-insights
        services.AddApplicationInsightsTelemetryWorkerService(options =>
        {
            options.ApplicationVersion = Assembly
                .GetEntryAssembly()!
                .GetAssemblyInformationalVersionAttribute()!
                .GetSourceVersionInformation()
                .ToString();
        });
        services.ConfigureFunctionsApplicationInsights();

        // Health check
        services.AddScoped<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>();
        services.AddHealthChecks()
            .AddLiveCheck()
            .AddDbContextCheck<DatabaseContext>(
                name: HealthCheckNames.CalculationDatabaseContext)
            .AddDatabricksJobsApiHealthCheck(
                name: HealthCheckNames.DatabricksJobsApi);

        // Common
        // => NodaTime
        services.AddSingleton<IClock>(_ => SystemClock.Instance);

        // Calculation
        // => Database
        var connectionStringOptions = context.Configuration
            .GetSection(ConnectionStringsOptions.ConnectionStrings)
            .Get<ConnectionStringsOptions>();
        services.AddScoped<IDatabaseContext, DatabaseContext>();
        services.AddDbContext<DatabaseContext>(
            options => options.UseSqlServer(
                connectionStringOptions!.DB_CONNECTION_STRING,
                o =>
                {
                    o.UseNodaTime();
                    o.EnableRetryOnFailure();
                }));

        services.AddScoped<ICalculationRepository, CalculationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // => Databricks
        services.AddDatabricksJobs(context.Configuration);
        services.AddScoped<IDatabricksCalculatorJobSelector, DatabricksCalculatorJobSelector>();
        services.AddScoped<ICalculationParametersFactory, DatabricksCalculationParametersFactory>();
        services.AddScoped<ICalculationEngineClient, CalculationEngineClient>();

        // => Clients
        services.AddScoped<ICalculationInfrastructureService, CalculationInfrastructureService>();

        // => Handlers
        services.AddScoped<IStartCalculationHandler, StartCalculationHandler>();
    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        // Make sure the logging configuration is picked up from settings.
        // Found inspiration in https://github.com/Azure/azure-functions-dotnet-worker/issues/1447
        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
    })
    .Build();

host.Run();
