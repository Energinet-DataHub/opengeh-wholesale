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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers.AmountPerChargeResultProducedV1;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Types;
using Google.Protobuf.WellKnownTypes;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;

public class AmountPerChargeResultProducedV1Factory : IAmountPerChargeResultProducedV1Factory
{
    public AmountPerChargeResultProducedV1 Create(WholesaleResult result)
    {
        var amountPerChargeResultProducedV1 = new AmountPerChargeResultProducedV1
        {
            CalculationId = result.CalculationId.ToString(),
            CalculationType = CalculationTypeMapper.MapCalculationType(result.CalculationType),
            PeriodStartUtc = result.PeriodStart.ToTimestamp(),
            PeriodEndUtc = result.PeriodEnd.ToTimestamp(),
            GridAreaCode = result.GridArea,
            EnergySupplierId = result.EnergySupplierId,
            ChargeCode = result.ChargeCode,
            ChargeType = ChargeTypeMapper.MapChargeType(result.ChargeType),
            ChargeOwnerId = result.ChargeOwnerId,
            Resolution = AmountPerChargeResultProducedV1.Types.Resolution.Hour,
            QuantityUnit = AmountPerChargeResultProducedV1.Types.QuantityUnit.Kwh,
            MeteringPointType = MeteringPointTypeMapper.MapMeteringPointType(result.MeteringPointType),
            SettlementMethod = SettlementMethodMapper.MapSettlementMethod(result.SettlementMethod),
            IsTax = result.IsTax,
        };

        amountPerChargeResultProducedV1.TimeSeriesPoints
            .AddRange(result.TimeSeriesPoints
                .Select(timeSeriesPoint =>
                {
                    var p = new AmountPerChargeResultProducedV1.Types.TimeSeriesPoint
                    {
                        Time = timeSeriesPoint.Time.ToTimestamp(),
                        Quantity = new DecimalValue(timeSeriesPoint.Quantity),
                        Price = new DecimalValue(timeSeriesPoint.Price),
                        Amount = new DecimalValue(timeSeriesPoint.Amount),
                    };
                    p.QuantityQualities.AddRange(timeSeriesPoint.Qualities.Select(QuantityQualityMapper.MapQuantityQuality).ToList());
                    return p;
                }));
        return amountPerChargeResultProducedV1;
    }
}
