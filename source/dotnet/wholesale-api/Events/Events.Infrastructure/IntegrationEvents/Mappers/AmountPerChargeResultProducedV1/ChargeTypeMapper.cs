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

using EventQuantityQuality = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.ChargeType;
using ModelChargeType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.ChargeType;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers.AmountPerChargeResultProducedV1;

public static class ChargeTypeMapper
{
    public static EventQuantityQuality MapChargeType(ModelChargeType chargeType)
    {
        return chargeType switch
        {
            ModelChargeType.Fee => EventQuantityQuality.Fee,
            ModelChargeType.Subscription => EventQuantityQuality.Subscription,
            ModelChargeType.Tariff => EventQuantityQuality.Tariff,
            _ => throw new ArgumentOutOfRangeException(
                nameof(chargeType),
                actualValue: chargeType,
                "Value cannot be mapped to a charge type."),
        };
    }
}
