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
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.DataLake;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.JsonSerialization;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Persistence.SettlementReportRequest;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Extensions.DependencyInjection;

/// <summary>
/// Registration of services required for the CalculationResults module.
/// </summary>
public static class CalculationResultsExtensions
{
    public static IServiceCollection AddCalculationResultsModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDatabricksSqlStatementForApplication(configuration);
        services.AddDataLakeClientForApplication();

        services.AddScoped<ISettlementReportClient, SettlementReportClient>();
        services.AddScoped<ISettlementReportResultsCsvWriter, SettlementReportResultsCsvWriter>();
        services.AddScoped<IDataLakeClient, DataLakeClient>();
        services.AddScoped<IStreamZipper, StreamZipper>();
        services.AddScoped<IJsonNewlineSerializer, JsonNewlineSerializer>();

        services.AddScoped<ISettlementReportDatabaseContext, SettlementReportDatabaseContext>();
        services.AddDbContext<SettlementReportDatabaseContext>(
            options => options.UseSqlServer(
                configuration
                    .GetSection(ConnectionStringsOptions.ConnectionStrings)
                    .Get<ConnectionStringsOptions>()!.DB_CONNECTION_STRING,
                o =>
                {
                    o.UseNodaTime();
                    o.EnableRetryOnFailure();
                }));
        // Database Health check
        services.TryAddHealthChecks(
            registrationKey: HealthCheckNames.WholesaleDatabase,
            (key, builder) =>
            {
                builder.AddDbContextCheck<SettlementReportDatabaseContext>(name: key);
            });

        // Used by sql statements (queries)
        services.AddOptions<DeltaTableOptions>().Bind(configuration);
        services.AddScoped<IEnergyResultQueries, EnergyResultQueries>();
        services.AddScoped<IWholesaleResultQueries, WholesaleResultQueries>();
        services.AddScoped<IWholesaleServicesQueries, WholesaleServicesQueries>();
        services.AddScoped<IAggregatedTimeSeriesQueries, AggregatedTimeSeriesQueries>();
        services.AddScoped<ISettlementReportResultQueries, SettlementReportResultQueries>();

        return services;
    }

    public static IServiceCollection AddCalculationResultsV2Module(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDatabricksSqlStatementForApplication(configuration);
        services.AddDataLakeClientForApplication();

        services.AddScoped<ISettlementReportClient, SettlementReportClient>();
        services.AddScoped<ISettlementReportResultsCsvWriter, SettlementReportResultsCsvWriter>();
        services.AddScoped<IDataLakeClient, DataLakeClient>();
        services.AddScoped<IStreamZipper, StreamZipper>();
        services.AddScoped<IJsonNewlineSerializer, JsonNewlineSerializer>();

        // Settlement Reports
        services.AddScoped<ISettlementReportRequestHandler, SettlementReportRequestHandler>();
        services.AddScoped<ISettlementReportFileRequestHandler, SettlementReportFileRequestHandler>();
        services.AddScoped<ISettlementReportFromFilesHandler, SettlementReportFromFilesHandler>();
        services.AddScoped<ISettlementReportFinalizeHandler, SettlementReportFinalizeHandler>();
        services.AddScoped<ISettlementReportInitializeHandler, SettlementReportInitializeHandler>();

        services.AddScoped<IGetSettlementReportsHandler, GetSettlementReportsHandler>();
        services.AddScoped<IUpdateFailedSettlementReportsHandler, UpdateFailedSettlementReportsHandler>();

        services.AddScoped<IRemoveExpiredSettlementReports, RemoveExpiredSettlementReports>();
        services.AddScoped<ISettlementReportDataRepository, LegacySettlementReportDataRepository>();
        services.AddScoped<ISettlementReportWholesaleRepository, SettlementReportWholesaleRepository>();
        services.AddScoped<ISettlementReportWholesaleResultQueries, SettlementReportWholesaleResultQueries>();
        services.AddScoped<ISettlementReportRepository, SettlementReportRepository>();
        services.AddScoped<ISettlementReportFileGeneratorFactory, SettlementReportFileGeneratorFactory>();
        services.AddScoped<ISettlementReportDownloadHandler, SettlementReportDownloadHandler>();
        services.AddSettlementReportBlobStorage();

        services.AddScoped<ISettlementReportDatabaseContext, SettlementReportDatabaseContext>();
        services.AddDbContext<SettlementReportDatabaseContext>(
            options => options.UseSqlServer(
                configuration
                    .GetSection(ConnectionStringsOptions.ConnectionStrings)
                    .Get<ConnectionStringsOptions>()!.DB_CONNECTION_STRING,
                o =>
                {
                    o.UseNodaTime();
                    o.EnableRetryOnFailure();
                }));

        // Database Health check
        services.TryAddHealthChecks(
            registrationKey: HealthCheckNames.WholesaleDatabase,
            (key, builder) =>
            {
                builder.AddDbContextCheck<SettlementReportDatabaseContext>(name: key);
            });

        // Used by sql statements (queries)
        services.AddOptions<DeltaTableOptions>().Bind(configuration);
        services.AddScoped<IEnergyResultQueries, EnergyResultQueries>();
        services.AddScoped<IWholesaleResultQueries, WholesaleResultQueries>();
        services.AddScoped<IWholesaleServicesQueries, WholesaleServicesQueries>();
        services.AddScoped<ITotalMonthlyAmountResultQueries, TotalMonthlyAmountResultQueries>();
        services.AddScoped<IAggregatedTimeSeriesQueries, AggregatedTimeSeriesQueries>();
        services.AddScoped<ISettlementReportResultQueries, SettlementReportResultQueries>();

        return services;
    }
}
