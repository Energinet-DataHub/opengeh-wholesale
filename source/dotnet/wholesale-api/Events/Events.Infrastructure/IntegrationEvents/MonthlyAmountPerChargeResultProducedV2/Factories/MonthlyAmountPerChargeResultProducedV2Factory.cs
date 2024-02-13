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
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Common;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.MonthlyAmountPerChargeResultProducedV2.Mappers;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.MonthlyAmountPerChargeResultProducedV2.Factories;

public class MonthlyAmountPerChargeResultProducedV2Factory : IMonthlyAmountPerChargeResultProducedV1Factory
{
    public bool CanCreate(WholesaleResult result) =>
        result.AmountType == AmountType.MonthlyAmountPerCharge
        && result.Resolution is Resolution.Month
        && result.TimeSeriesPoints.Count == 1;

    public Contracts.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1 Create(WholesaleResult result)
    {
        if (!CanCreate(result))
            throw new ArgumentException($"Cannot create '{nameof(MonthlyAmountPerChargeResultProducedV1)}' from wholesale result.", nameof(result));

        var amountPerChargeResultProducedV1 = new Contracts.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1
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
            Currency = Contracts.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1.Types.Currency.Dkk,
            Amount = result.TimeSeriesPoints.Single().Amount,
        };

        return amountPerChargeResultProducedV1;
    }
}
