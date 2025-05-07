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

using Energinet.DataHub.Core.App.Common.Extensions.DependencyInjection;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Core.Outbox.Extensions.DependencyInjection;
using Energinet.DataHub.RevisionLog.Integration.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Calculations.Application.AuditLog;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.GridArea;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.AuditLog;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.GridArea;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Energinet.DataHub.Wholesale.Calculations.Infrastructure.Extensions.DependencyInjection;

/// <summary>
/// Registration of services required for the Calculations module.
/// </summary>
public static class CalculationsExtensions
{
    /// <summary>
    /// Dependencies solely needed for running calculations in the calculation engine.
    /// </summary>
    public static IServiceCollection AddCalculationEngineModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDatabricksJobsForApplication(configuration);

        services.AddScoped<ICalculationEngineClient, CalculationEngineClient>();

        services.AddScoped<IDatabricksCalculatorJobSelector, DatabricksCalculatorJobSelector>();
        services.AddScoped<ICalculationParametersFactory, DatabricksCalculationParametersFactory>();

        AddAuditLogging(services, configuration);

        return services;
    }

    /// <summary>
    /// Dependencies needed for retrieving and updating information about calculations.
    /// </summary>
    public static IServiceCollection AddCalculationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddScoped<ICalculationsClient, CalculationsClient>();
        services.AddScoped<ICalculationFactory, CalculationFactory>();
        services.AddScoped<ICalculationRepository, CalculationRepository>();
        services.AddScoped<IGridAreaOwnerRepository, GridAreaOwnerRepository>();

        services.AddDbContext<DatabaseContext>(
            options => options.UseSqlServer(
                configuration
                    .GetSection(ConnectionStringsOptions.ConnectionStrings)
                    .Get<ConnectionStringsOptions>()!.DB_CONNECTION_STRING,
                o =>
                {
                    o.UseNodaTime();
                    o.EnableRetryOnFailure();
                }));

        // Make sure injection an IDatabaseContext will return the same instance as injection a DatabaseContext
        services.AddTransient<IDatabaseContext>(sp => sp.GetRequiredService<DatabaseContext>());

        // Database Health check
        services.TryAddHealthChecks(
            registrationKey: HealthCheckNames.WholesaleDatabase,
            (key, builder) =>
            {
                builder.AddDbContextCheck<DatabaseContext>(name: key);
            });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICalculationDtoMapper, CalculationDtoMapper>();

        services.AddScoped<IGridAreaOwnershipClient, GridAreaOwnershipClient>();

        AddAuditLogging(services, configuration);

        return services;
    }

    private static void AddAuditLogging(IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddTransient<IJsonSerializer, JsonSerializer>();
        services.TryAddTransient<IAuditLogger, AuditLogger>();

        services.AddRevisionLogIntegrationModule(configuration);
    }
}
