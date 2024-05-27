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

using Energinet.DataHub.Edi.Responses;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;

namespace Energinet.DataHub.Wholesale.Edi.Mappers;

public static class MeteringPointTypeMapper
{
    public static WholesaleServicesRequestSeries.Types.MeteringPointType Map(MeteringPointType seriesMeteringPointType)
    {
        return seriesMeteringPointType switch
        {
            MeteringPointType.Consumption => WholesaleServicesRequestSeries.Types.MeteringPointType.Consumption,
            MeteringPointType.Production => WholesaleServicesRequestSeries.Types.MeteringPointType.Production,
            MeteringPointType.VeProduction => WholesaleServicesRequestSeries.Types.MeteringPointType.VeProduction,
            MeteringPointType.NetProduction => WholesaleServicesRequestSeries.Types.MeteringPointType.NetProduction,
            MeteringPointType.SupplyToGrid => WholesaleServicesRequestSeries.Types.MeteringPointType.SupplyToGrid,
            MeteringPointType.ConsumptionFromGrid => WholesaleServicesRequestSeries.Types.MeteringPointType.ConsumptionFromGrid,
            MeteringPointType.WholesaleServicesInformation => WholesaleServicesRequestSeries.Types.MeteringPointType.WholesaleServicesInformation,
            MeteringPointType.OwnProduction => WholesaleServicesRequestSeries.Types.MeteringPointType.OwnProduction,
            MeteringPointType.NetFromGrid => WholesaleServicesRequestSeries.Types.MeteringPointType.NetFromGrid,
            MeteringPointType.NetToGrid => WholesaleServicesRequestSeries.Types.MeteringPointType.NetToGrid,
            MeteringPointType.TotalConsumption => WholesaleServicesRequestSeries.Types.MeteringPointType.TotalConsumption,
            MeteringPointType.ElectricalHeating => WholesaleServicesRequestSeries.Types.MeteringPointType.ElectricalHeating,
            MeteringPointType.NetConsumption => WholesaleServicesRequestSeries.Types.MeteringPointType.NetConsumption,
            MeteringPointType.EffectSettlement => WholesaleServicesRequestSeries.Types.MeteringPointType.EffectSettlement,
            _ => throw new ArgumentOutOfRangeException(
                nameof(seriesMeteringPointType),
                actualValue: seriesMeteringPointType,
                $"Value cannot be mapped to a {nameof(WholesaleServicesRequestSeries.Types.MeteringPointType)}."),
        };
    }
}
