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
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers.MonthlyAmountPerChargeResultProducedV1;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Types;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;

public class MonthlyAmountPerChargeResultProducedV1Factory : IMonthlyAmountPerChargeResultProducedV1Factory
{
    public MonthlyAmountPerChargeResultProducedV1 Create(WholesaleResult result)
    {
        if (result.TimeSeriesPoints.Count != 1)
            throw new ArgumentException("MonthlyAmountPerChargeResultProducedV1 expects exactly one time series point.");

        if (result.AmountType != AmountType.MonthlyAmountPerCharge)
            throw new ArgumentException($"MonthlyAmountPerChargeResultProducedV1 expect amount type to be '{AmountType.MonthlyAmountPerCharge}'.");

        if (result.Resolution != Resolution.Month)
            throw new ArgumentException($"MonthlyAmountPerChargeResultProducedV1 expect resolution to be '{Resolution.Month}'.");

        var amountPerChargeResultProducedV1 = new MonthlyAmountPerChargeResultProducedV1
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
            QuantityUnit = QuantityUnitMapper.MapQuantityUnit(result.QuantityUnit),
            IsTax = result.IsTax,
            Amount = result.TimeSeriesPoints.Single().Amount,
        };

        return amountPerChargeResultProducedV1;
    }
}
