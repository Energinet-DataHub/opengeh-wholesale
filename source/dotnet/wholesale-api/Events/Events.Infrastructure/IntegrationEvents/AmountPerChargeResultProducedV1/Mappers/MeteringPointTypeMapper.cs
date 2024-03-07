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

using EventMeteringPointType = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.MeteringPointType;
using ModelMeteringPointType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.MeteringPointType;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.AmountPerChargeResultProducedV1.Mappers;

public static class MeteringPointTypeMapper
{
    public static EventMeteringPointType MapMeteringPointType(ModelMeteringPointType? meteringPointType)
    {
        if (meteringPointType == null)
            return EventMeteringPointType.Unspecified;

        var type = Enum.Parse<EventMeteringPointType>(meteringPointType.Value.ToString());

        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(
                nameof(meteringPointType),
                actualValue: meteringPointType,
                "Value cannot be mapped to a metering point type.");
        }

        return type;
    }
}
