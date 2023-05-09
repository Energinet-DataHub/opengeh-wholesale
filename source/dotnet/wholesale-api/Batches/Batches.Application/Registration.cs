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

using Energinet.DataHub.Wholesale.Batches.Application.BatchExecutionStateUpdateService;
using Energinet.DataHub.Wholesale.Batches.Application.Model;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.BatchAggregate;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.BatchExecutionStateDomainService;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.CalculationDomainService;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Calculations;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence.Batches;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Components.DatabricksClient.DatabricksWheelClient;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.Batches.Application;

/// <summary>
/// Registration of services required for the Batches module.
/// </summary>
public static class Registration
{
    public static void AddBatchesModule(
        this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IBatchExecutionStateDomainService, BatchExecutionStateDomainService>();
        serviceCollection.AddScoped<ICalculationDomainService, CalculationDomainService>();
        serviceCollection.AddScoped<IBatchFactory, BatchFactory>();
        serviceCollection.AddScoped<IBatchRepository, BatchRepository>();
        serviceCollection.AddSingleton(new BatchStateMapper());

        serviceCollection.AddScoped<ICalculationEngineClient, CalculationEngineClient>();

        serviceCollection.AddScoped<IDatabricksCalculatorJobSelector, DatabricksCalculatorJobSelector>();
        serviceCollection.AddScoped<IDatabricksWheelClient, DatabricksWheelClient>();
        serviceCollection.AddScoped<ICalculationParametersFactory, DatabricksCalculationParametersFactory>();

        serviceCollection.AddScoped<IDatabaseContext, DatabaseContext>();
        serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
        serviceCollection.AddScoped<IBatchApplicationService, BatchApplicationService>();
        serviceCollection.AddScoped<IBatchDtoMapper, BatchDtoMapper>();

        serviceCollection.AddScoped<ICreateBatchHandler, CreateBatchHandler>();

        serviceCollection.AddHostedService<UpdateBatchExecutionStateWorker>();
    }
}
