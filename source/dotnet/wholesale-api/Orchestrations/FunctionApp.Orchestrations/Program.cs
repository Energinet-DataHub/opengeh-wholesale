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

using Energinet.DataHub.Core.Databricks.Jobs.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Batches.Application;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Batches.Application.UseCases;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Calculations;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence.Calculations;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Register dependencies for calculation activities
        // => NodaTIme
        services.AddSingleton<IClock>(_ => SystemClock.Instance);
        services.AddSingleton<DateTimeZone>(_ =>
        {
            return DateTimeZoneProviders.Tzdb.GetZoneOrNull(Environment.GetEnvironmentVariable("TIME_ZONE")!)!;
        });

        // => SQL Database
        services.AddScoped<IDatabaseContext, DatabaseContext>();
        services.AddDbContext<DatabaseContext>(
            options => options.UseSqlServer(
                Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"),
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

        // => Factories
        services.AddScoped<ICalculationFactory, CalculationFactory>();

        // => Handlers
        services.AddScoped<ICreateCalculationHandler, CreateCalculationHandler>();
    })
    .Build();

host.Run();
