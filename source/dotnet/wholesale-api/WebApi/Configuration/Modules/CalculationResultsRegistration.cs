﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Wholesale.CalculationResults.Application;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.BatchActor;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.DataLake;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.JsonNewlineSerializer;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Processes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.Actors;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;

namespace Energinet.DataHub.Wholesale.WebApi.Configuration;

/// <summary>
/// Registration of services required for the CalculationResults module.
/// </summary>
public static class CalculationResultsRegistration
{
    public static void AddCalculationResultsModule(
        this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ISettlementReportClient, SettlementReportClient>();
        serviceCollection.AddHttpClient<ICalculationResultClient>();
        serviceCollection.AddScoped<ICalculationResultClient, CalculationResultClient>();
        serviceCollection.AddScoped<ISettlementReportResultsCsvWriter, SettlementReportResultsCsvWriter>();
        serviceCollection.AddScoped<IDatabricksSqlResponseParser, DatabricksSqlResponseParser>();
        serviceCollection.AddScoped<IDataLakeClient, DataLakeClient>();
        serviceCollection.AddScoped<IStreamZipper, StreamZipper>();
        serviceCollection.AddScoped<IProcessStepResultRepository, ProcessStepResultRepository>();
        serviceCollection.AddScoped<IActorClient, ActorClient>();
        serviceCollection.AddScoped<IJsonNewlineSerializer, JsonNewlineSerializer>();
        serviceCollection.AddScoped<ISettlementReportRepository>(
            provider => new SettlementReportRepository(
                provider.GetRequiredService<IDataLakeClient>(),
                provider.GetRequiredService<IStreamZipper>()));
    }
}
