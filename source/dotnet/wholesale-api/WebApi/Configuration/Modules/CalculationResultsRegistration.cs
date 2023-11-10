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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.DataLake;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.JsonNewlineSerializer;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.RequestCalculationResult;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;

namespace Energinet.DataHub.Wholesale.WebApi.Configuration.Modules;

/// <summary>
/// Registration of services required for the CalculationResults module.
/// </summary>
public static class CalculationResultsRegistration
{
    public static void AddCalculationResultsModule(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddScoped<ISettlementReportClient, SettlementReportClient>();

        serviceCollection.AddDatabricksSqlStatementExecution(configuration);

        serviceCollection.AddScoped<ISettlementReportResultsCsvWriter, SettlementReportResultsCsvWriter>();
        serviceCollection.AddScoped<IDataLakeClient, DataLakeClient>();
        serviceCollection.AddScoped<IStreamZipper, StreamZipper>();
        serviceCollection.AddScoped<IEnergyResultQueries, EnergyResultQueries>();
        serviceCollection.AddScoped<IWholesaleResultQueries, WholesaleResultQueries>();
        serviceCollection.AddScoped<IRequestCalculationResultQueries, RequestCalculationResultQueries>();
        serviceCollection.AddScoped<IJsonNewlineSerializer, JsonNewlineSerializer>();
        serviceCollection.AddScoped<ISettlementReportRepository>(
            provider => new SettlementReportRepository(
                provider.GetRequiredService<IDataLakeClient>(),
                provider.GetRequiredService<IStreamZipper>()));
        serviceCollection.AddScoped<ISettlementReportResultQueries, SettlementReportResultQueries>();
    }
}
