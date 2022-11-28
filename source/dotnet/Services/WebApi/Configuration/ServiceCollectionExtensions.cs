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

using System.IdentityModel.Tokens.Jwt;
using Azure.Storage.Files.DataLake;
using Energinet.DataHub.Core.App.Common.Abstractions.Identity;
using Energinet.DataHub.Core.App.Common.Abstractions.Security;
using Energinet.DataHub.Core.App.Common.Identity;
using Energinet.DataHub.Core.App.Common.Security;
using Energinet.DataHub.Core.App.WebApp.Middleware;
using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Application.Infrastructure;
using Energinet.DataHub.Wholesale.Application.JobRunner;
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Application.ProcessResult;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.BasisData;
using Energinet.DataHub.Wholesale.Infrastructure.Core;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Batches;
using Energinet.DataHub.Wholesale.WebApi.Controllers.V2;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using NodaTime;

namespace Energinet.DataHub.Wholesale.WebApi.Configuration;

internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds registrations of JwtTokenMiddleware and corresponding dependencies.
    /// </summary>
    /// <param name="serviceCollection">ServiceCollection container</param>
    public static void AddJwtTokenSecurity(this IServiceCollection serviceCollection)
    {
        var metadataAddress = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.FrontEndOpenIdUrl);
        var audience = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.FrontEndServiceAppId);

        serviceCollection.AddSingleton<ISecurityTokenValidator, JwtSecurityTokenHandler>();
        serviceCollection.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(_ =>
            new ConfigurationManager<OpenIdConnectConfiguration>(
                metadataAddress,
                new OpenIdConnectConfigurationRetriever()));

        serviceCollection.AddScoped<IJwtTokenValidator>(sp =>
            new JwtTokenValidator(
                sp.GetRequiredService<ILogger<JwtTokenValidator>>(),
                sp.GetRequiredService<ISecurityTokenValidator>(),
                sp.GetRequiredService<IConfigurationManager<OpenIdConnectConfiguration>>(),
                audience));

        serviceCollection.AddScoped<ClaimsPrincipalContext>();
        serviceCollection.AddScoped<IClaimsPrincipalAccessor, ClaimsPrincipalAccessor>();

        serviceCollection.AddScoped<JwtTokenMiddleware>();
    }

    public static void AddCommandStack(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(EnvironmentSettingNames.DbConnectionString);
        if (connectionString == null)
            throw new ArgumentNullException(EnvironmentSettingNames.DbConnectionString, "does not exist in configuration settings");

        services.AddDbContext<DatabaseContext>(
            options => options.UseSqlServer(connectionString, o =>
            {
                o.UseNodaTime();
                o.EnableRetryOnFailure();
            }));
        services.AddScoped<IClock>(_ => SystemClock.Instance);
        services.AddScoped<IDatabaseContext, DatabaseContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IBatchApplicationService, BatchApplicationService>();
        services.AddScoped<IBasisDataApplicationService, BasisDataApplicationService>();
        services.AddScoped<IBatchFileManager, BatchFileManager>();
        services.AddScoped<IStreamZipper, StreamZipper>();
        var calculationStorageConnectionString = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.CalculationStorageConnectionString);
        var calculationStorageContainerName = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.CalculationStorageContainerName);
        var dataLakeFileSystemClient = new DataLakeFileSystemClient(calculationStorageConnectionString, calculationStorageContainerName);
        services.AddSingleton(dataLakeFileSystemClient);
        services.AddScoped<HttpClient>(_ => null!);
        services.AddScoped<IBatchCompletedPublisher>(_ => null!); // Unused in the use cases of this app
        services.AddScoped<IBatchFactory, BatchFactory>();
        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<IBatchExecutionStateHandler, BatchExecutionStateHandler>();
        services.AddScoped<IBatchDtoMapper, BatchDtoMapper>();
        services.AddScoped<IBatchDtoV2Mapper, BatchDtoV2Mapper>();
        services.AddScoped<IProcessCompletedPublisher>(_ => null!); // Unused in the use cases of this app
        services.AddScoped<ICalculatorJobRunner>(_ => null!); // Unused in the use cases of this app
        services.AddScoped<ICalculatorJobParametersFactory>(_ => null!); // Unused in the use cases of this app
        services.AddScoped<IProcessResultApplicationService, ProcessResultApplicationService>();
    }
}
