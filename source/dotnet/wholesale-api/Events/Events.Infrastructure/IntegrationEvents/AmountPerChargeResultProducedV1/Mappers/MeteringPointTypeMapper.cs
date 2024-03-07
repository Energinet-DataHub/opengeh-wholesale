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
        return meteringPointType switch
        {
            ModelMeteringPointType.Consumption => EventMeteringPointType.Consumption,
            ModelMeteringPointType.Production => EventMeteringPointType.Production,
            ModelMeteringPointType.VeProduction => EventMeteringPointType.VeProduction,
            ModelMeteringPointType.NetProduction => EventMeteringPointType.NetProduction,
            ModelMeteringPointType.SupplyToGrid => EventMeteringPointType.SupplyToGrid,
            ModelMeteringPointType.ConsumptionFromGrid => EventMeteringPointType.ConsumptionFromGrid,
            ModelMeteringPointType.WholesaleServicesInformation => EventMeteringPointType.WholesaleServicesInformation,
            ModelMeteringPointType.OwnProduction => EventMeteringPointType.OwnProduction,
            ModelMeteringPointType.NetFromGrid => EventMeteringPointType.NetFromGrid,
            ModelMeteringPointType.NetToGrid => EventMeteringPointType.NetToGrid,
            ModelMeteringPointType.TotalConsumption => EventMeteringPointType.TotalConsumption,
            ModelMeteringPointType.ElectricalHeating => EventMeteringPointType.ElectricalHeating,
            ModelMeteringPointType.NetConsumption => EventMeteringPointType.NetConsumption,
            ModelMeteringPointType.EffectSettlement => EventMeteringPointType.EffectSettlement,
            null => EventMeteringPointType.Unspecified,
            _ => throw new ArgumentOutOfRangeException(
                nameof(meteringPointType),
                actualValue: meteringPointType,
                "Value cannot be mapped to a metering point type."),
        };
    }
}
