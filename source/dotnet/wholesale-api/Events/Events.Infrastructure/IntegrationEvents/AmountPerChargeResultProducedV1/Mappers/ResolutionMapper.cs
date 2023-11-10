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

using EventResolution = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.Resolution;
using ModelResolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.AmountPerChargeResultProducedV1.Mappers;

public static class ResolutionMapper
{
    public static EventResolution MapResolution(ModelResolution resolution)
    {
        return resolution switch
        {
            ModelResolution.Hour => EventResolution.Hour,
            ModelResolution.Day => EventResolution.Day,
            ModelResolution.Month => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                actualValue: resolution,
                $"Value is not valid for {nameof(AmountPerChargeResultProducedV1)}."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                actualValue: resolution,
                "Value cannot be mapped to a resolution."),
        };
    }
}
