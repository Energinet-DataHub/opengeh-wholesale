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

using Energinet.DataHub.Core.App.WebApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Wholesale.Batches.Application;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Batches.Application.UseCases;
using Energinet.DataHub.Wholesale.Batches.Application.Workers;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Calculations;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.CalculationState;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence.Calculations;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Wholesale.WebApi.Configuration.Modules;

/// <summary>
/// Registration of services required for the Batches module.
/// </summary>
public static class CalculationsRegistration
{
    public static void AddCalculationsModule(
        this IServiceCollection serviceCollection,
        Func<string> databaseConnectionStringProvider)
    {
        serviceCollection.AddScoped<ICalculationsClient, CalculationsClient>();
        serviceCollection.AddScoped<ICalculationExecutionStateInfrastructureService, CalculationExecutionStateInfrastructureService>();
        serviceCollection.AddScoped<ICalculationInfrastructureService, CalculationInfrastructureService>();
        serviceCollection.AddScoped<ICalculationFactory, CalculationFactory>();
        serviceCollection.AddScoped<ICalculationRepository, CalculationRepository>();
        serviceCollection.AddSingleton(new CalculationStateMapper());

        serviceCollection.AddScoped<ICalculationEngineClient, CalculationEngineClient>();

        serviceCollection.AddScoped<IDatabricksCalculatorJobSelector, DatabricksCalculatorJobSelector>();
        serviceCollection.AddScoped<ICalculationParametersFactory, DatabricksCalculationParametersFactory>();

        serviceCollection.AddScoped<IDatabaseContext, DatabaseContext>();
        serviceCollection.AddDbContext<DatabaseContext>(
            options => options.UseSqlServer(
                databaseConnectionStringProvider(),
                o =>
                {
                    o.UseNodaTime();
                    o.EnableRetryOnFailure();
                }));

        serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
        serviceCollection.AddScoped<ICalculationDtoMapper, CalculationDtoMapper>();

        serviceCollection.AddScoped<ICreateBatchHandler, CreateBatchHandler>();
        serviceCollection.AddScoped<IStartCalculationHandler, StartCalculationHandler>();
        serviceCollection.AddScoped<IUpdateBatchExecutionStateHandler, UpdateBatchExecutionStateHandler>();

        RegisterHostedServices(serviceCollection);
    }

    private static void RegisterHostedServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddHostedService<StartCalculationTrigger>();
        serviceCollection.AddHostedService<UpdateBatchExecutionStateTrigger>();

        serviceCollection
            .AddHealthChecks()
            .AddRepeatingTriggerHealthCheck<StartCalculationTrigger>(TimeSpan.FromMinutes(1))
            .AddRepeatingTriggerHealthCheck<UpdateBatchExecutionStateTrigger>(TimeSpan.FromMinutes(1));
    }
}
