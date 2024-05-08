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
using Energinet.DataHub.Wholesale.Common.Application;
using Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> for setting up Entity Framework Core against Wholesale SQL DB.
/// </summary>
public static class EntityFrameworkCoreExtensions
{
    /// <summary>
    /// Register EF Core DB context for wholesale sql DB.
    /// </summary>
    public static IServiceCollection AddWholesaleSqlDatabase<TDbContextInterface, TDbContext>(this IServiceCollection services, IConfiguration configuration)
        where TDbContextInterface : class
        where TDbContext : DbContext, TDbContextInterface
    {
        services
            .AddScoped<TDbContextInterface, TDbContext>()
            .AddDbContext<TDbContext>(
                options => options.UseSqlServer(
                    configuration
                        .GetSection(ConnectionStringsOptions.ConnectionStrings)
                        .Get<ConnectionStringsOptions>()!.DB_CONNECTION_STRING,
                    o =>
                    {
                        o.UseNodaTime();
                        o.EnableRetryOnFailure();
                    }));

        services
            .TryAddHealthChecks(
                registrationKey: HealthCheckNames.WholesaleDatabase,
                (key, builder) =>
                {
                    builder.AddDbContextCheck<TDbContext>(name: key);
                });

        services
            .AddScoped<IUnitOfWork>(sp => new UnitOfWork((sp.GetRequiredService<TDbContextInterface>() as DbContext)!));

        return services;
    }
}
