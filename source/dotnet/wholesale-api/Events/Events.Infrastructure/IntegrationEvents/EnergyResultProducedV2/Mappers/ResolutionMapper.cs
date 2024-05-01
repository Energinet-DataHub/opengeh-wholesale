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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Resolution = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV2.Types.Resolution;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EnergyResultProducedV2.Mappers;

public static class ResolutionMapper
{
    public static Resolution MapResolution(CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.Resolution resolution)
    {
        return resolution switch
        {
            CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.Resolution.Quarter => Resolution.Quarter,
            CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.Resolution.Hour => Resolution.Hour,
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                actualValue: resolution,
                "Value cannot be mapped to a resolution."),
        };
    }
}
