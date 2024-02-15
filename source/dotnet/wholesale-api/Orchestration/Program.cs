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

using Energinet.DataHub.Core.Databricks.Jobs.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.Databricks.Jobs.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Calculations.Application.UseCases;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Orchestration.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Common
        // => Application Insights (telemetry)
        services.AddApplicationInsightsForIsolatedWorker();
        // => Health checks
        services.AddHealthChecksForIsolatedWorker();
        // => NodaTime
        services.AddNodaTimeForApplication(context.Configuration);

        // Calculation
        // => Database
        services.AddHealthChecks()
            .AddDbContextCheck<DatabaseContext>(
                name: HealthCheckNames.CalculationDatabaseContext);
        services.AddScoped<IDatabaseContext, DatabaseContext>();
        var connectionStringOptions = context.Configuration
            .GetSection(ConnectionStringsOptions.ConnectionStrings)
            .Get<ConnectionStringsOptions>();
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
        services.AddHealthChecks()
            .AddDatabricksJobsApiHealthCheck(
                name: HealthCheckNames.DatabricksJobsApi);
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
        logging.AddLoggingConfigurationForIsolatedWorker(hostingContext);
    })
    .Build();

host.Run();
