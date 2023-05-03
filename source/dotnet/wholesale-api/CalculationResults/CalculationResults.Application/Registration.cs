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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.CalculationResults.Application;

/// <summary>
/// Registration of services required for the CalculationResults module.
/// </summary>
public static class Registration
{
    public static void AddCalculationResultsModule(
        this IServiceCollection serviceCollection)
    {
        // TODO: This registration depends on IOptions<DatabricksOptions> - how do we make that explicit?
        serviceCollection.AddHttpClient<ICalculationResultClient>();
        serviceCollection.AddScoped<ICalculationResultClient, CalculationResultClient>();
        serviceCollection.AddScoped<IProcessResultPointFactory, ProcessResultPointFactory>();

        // TODO: Should we add all required stuff? What if it's shared - like e.g. IJsonSerializer?
    }
}
