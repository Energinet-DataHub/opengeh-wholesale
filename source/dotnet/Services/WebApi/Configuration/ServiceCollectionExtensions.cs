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

using Azure.Storage.Files.DataLake;
using Energinet.DataHub.Core.App.WebApp.Authentication;
using Energinet.DataHub.Core.App.WebApp.Authorization;
using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Application.CalculationJobs;
using Energinet.DataHub.Wholesale.Application.Infrastructure;
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Application.ProcessResult;
using Energinet.DataHub.Wholesale.Application.SettlementReport;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Energinet.DataHub.Wholesale.Domain.SettlementReportAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Core;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Batches;
using Energinet.DataHub.Wholesale.Infrastructure.Processes;
using Energinet.DataHub.Wholesale.Infrastructure.SettlementReport;
using Energinet.DataHub.Wholesale.WebApi.Controllers.V2;
using Microsoft.EntityFrameworkCore;
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
        var externalOpenIdUrl = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ExternalOpenIdUrl);
        var internalOpenIdUrl = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.InternalOpenIdUrl);
        var backendAppId = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.BackendAppId);

        serviceCollection.AddJwtBearerAuthentication(externalOpenIdUrl, internalOpenIdUrl, backendAppId);
        serviceCollection.AddPermissionAuthorization();
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
        services.AddScoped<ISettlementReportApplicationService, SettlementReportApplicationService>();
        services.AddScoped<ISettlementReportRepository, SettlementReportRepository>();
        services.AddScoped<IStreamZipper, StreamZipper>();
        services.AddScoped<IProcessResultPointFactory, ProcessResultPointFactory>();
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
        services.AddScoped<IProcessStepResultApplicationService, ProcessStepResultApplicationService>();
        services.AddScoped<IProcessStepResultMapper, ProcessStepResultMapper>();
        services.AddScoped<IProcessStepResultRepository, ProcessStepResultRepository>();
        services.AddScoped<IBatchRequestDtoValidator, BatchRequestDtoValidator>();

        services.ConfigureDateTime();
    }

    private static void ConfigureDateTime(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IClock>(_ => SystemClock.Instance);
        var dateTimeZoneId = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.DateTimeZoneId);
        var dateTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(dateTimeZoneId);
        if (dateTimeZone == null)
            throw new ArgumentNullException($"Cannot resolve date time zone object for zone id '{dateTimeZoneId}' from application setting '{EnvironmentSettingNames.DateTimeZoneId}'");
        serviceCollection.AddSingleton(dateTimeZone);
    }
}
