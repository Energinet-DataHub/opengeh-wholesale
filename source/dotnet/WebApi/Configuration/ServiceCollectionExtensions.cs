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

using Energinet.DataHub.Core.App.Common.Abstractions.Identity;
using Energinet.DataHub.Core.App.Common.Abstractions.Security;
using Energinet.DataHub.Core.App.Common.Identity;
using Energinet.DataHub.Core.App.Common.Security;
using Energinet.DataHub.Core.App.WebApp.Middleware;
using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Batches;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Wholesale.WebApi.Configuration;

internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds registrations of JwtTokenMiddleware and corresponding dependencies.
    /// </summary>
    /// <param name="serviceCollection">ServiceCollection container</param>
    public static void AddJwtTokenSecurity(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<JwtTokenMiddleware>();
        serviceCollection.AddScoped<IJwtTokenValidator, JwtTokenValidator>();
        serviceCollection.AddScoped<IClaimsPrincipalAccessor, ClaimsPrincipalAccessor>();
        serviceCollection.AddScoped<ClaimsPrincipalContext>();

        var address = Environment.GetEnvironmentVariable(EnvironmentSettingNames.FrontEndOpenIdUrl) ??
                      throw new Exception(
                          $"Function app is missing required environment variable '{EnvironmentSettingNames.FrontEndOpenIdUrl}'");
        var audience = Environment.GetEnvironmentVariable(EnvironmentSettingNames.FrontEndServiceAppId) ??
                       throw new Exception(
                           $"Function app is missing required environment variable '{EnvironmentSettingNames.FrontEndServiceAppId}'");
        serviceCollection.AddScoped(_ => new OpenIdSettings(address, audience));
    }

    public static void AddCommandStack(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(EnvironmentSettingNames.DbConnectionString);
        if (connectionString == null)
            throw new ArgumentNullException(EnvironmentSettingNames.DbConnectionString, "does not exist in configuration settings");

        services.AddDbContext<DatabaseContext>(
            options => options.UseSqlServer(connectionString, o => o.UseNodaTime()));

        services.AddScoped<IDatabaseContext, DatabaseContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IBatchApplicationService, BatchApplicationService>();
        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<IProcessCompletedPublisher>(_ => null!); // Unused in the use cases of this app
    }
}
