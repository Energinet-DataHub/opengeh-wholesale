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

using Azure.Identity;
using Azure.Storage.Files.DataLake;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Diagnostics.HealthChecks;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.DataLake;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.JsonSerialization;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks.DataLake;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Extensions.DependencyInjection;

/// <summary>
/// Registration of services required for the CalculationResults module.
/// </summary>
public static class CalculationResultsRegistration
{
    public static IServiceCollection AddCalculationResultsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISettlementReportClient, SettlementReportClient>();

        services.AddDatabricksSqlStatementExecution(configuration);
        services.AddDataLakeFileSystemClient(configuration);

        services.AddScoped<ISettlementReportResultsCsvWriter, SettlementReportResultsCsvWriter>();
        services.AddScoped<IDataLakeClient, DataLakeClient>();
        services.AddScoped<IStreamZipper, StreamZipper>();
        services.AddScoped<IEnergyResultQueries, EnergyResultQueries>();
        services.AddScoped<IWholesaleResultQueries, WholesaleResultQueries>();
        services.AddScoped<IAggregatedTimeSeriesQueries, AggregatedTimeSeriesQueries>();
        services.AddScoped<IJsonNewlineSerializer, JsonNewlineSerializer>();
        services.AddScoped<ISettlementReportRepository>(
            provider => new SettlementReportRepository(
                provider.GetRequiredService<IDataLakeClient>(),
                provider.GetRequiredService<IStreamZipper>()));
        services.AddScoped<ISettlementReportResultQueries, SettlementReportResultQueries>();

        // Health checks
        services.AddHealthChecks()
            .AddDatabricksSqlStatementApiHealthCheck()
            .AddDataLakeHealthCheck(
                _ => configuration.Get<DataLakeOptions>()!);

        return services;
    }

    private static IServiceCollection AddDataLakeFileSystemClient(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.Get<DataLakeOptions>()!;
        services.AddSingleton<DataLakeFileSystemClient>(_ =>
        {
            var dataLakeServiceClient = new DataLakeServiceClient(new Uri(options.STORAGE_ACCOUNT_URI), new DefaultAzureCredential());
            return dataLakeServiceClient.GetFileSystemClient(options.STORAGE_CONTAINER_NAME);
        });

        return services;
    }
}
