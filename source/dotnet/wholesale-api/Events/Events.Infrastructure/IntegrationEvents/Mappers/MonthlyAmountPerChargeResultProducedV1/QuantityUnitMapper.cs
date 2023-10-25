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

using EventQuantityUnit= Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1.Types.QuantityUnit;
using ModelQuantityUnit = Energinet.DataHub.Wholesale.Common.Models.QuantityUnit;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers.MonthlyAmountPerChargeResultProducedV1;

public static class QuantityUnitMapper
{
    public static EventQuantityUnit MapQuantityUnit(ModelQuantityUnit quantityUnit)
    {
        return quantityUnit switch
        {
            ModelQuantityUnit.Kwh => EventQuantityUnit.Kwh,
            ModelQuantityUnit.Pieces => EventQuantityUnit.Pieces,
            _ => throw new ArgumentOutOfRangeException(
                nameof(quantityUnit),
                actualValue: quantityUnit,
                "Value cannot be mapped to a quantity unit."),
        };
    }
}
