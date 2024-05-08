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

using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Application.UseCases;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.CalculationState;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.Calculations.Infrastructure.Extensions.DependencyInjection;

/// <summary>
/// Registration of services required for the Calculations module.
/// </summary>
public static class CalculationsExtensions
{
    public static IServiceCollection AddCalculationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICalculationsClient, CalculationsClient>();
        services.AddScoped<ICalculationExecutionStateInfrastructureService, CalculationExecutionStateInfrastructureService>();
        services.AddScoped<ICalculationInfrastructureService, CalculationInfrastructureService>();
        services.AddScoped<ICalculationFactory, CalculationFactory>();
        services.AddScoped<ICalculationRepository, CalculationRepository>();
        services.AddSingleton(new CalculationStateMapper());

        services.AddScoped<ICalculationEngineClient, CalculationEngineClient>();

        services.AddScoped<IDatabricksCalculatorJobSelector, DatabricksCalculatorJobSelector>();
        services.AddScoped<ICalculationParametersFactory, DatabricksCalculationParametersFactory>();

        services.AddWholesaleSqlDatabase<IDatabaseContext, DatabaseContext>(configuration);
        services.AddScoped<ICalculationDtoMapper, CalculationDtoMapper>();

        services.AddScoped<ICreateCalculationHandler, CreateCalculationHandler>();
        services.AddScoped<IStartCalculationHandler, StartCalculationHandler>();
        services.AddScoped<IUpdateCalculationExecutionStateHandler, UpdateCalculationExecutionStateHandler>();

        return services;
    }
}
