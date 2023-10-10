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

using Energinet.DataHub.Core.Messaging.Communication.Internal;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Types;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;

public class WholesaleResultProducedV1Factory : IWholesaleResultProducedV1Factory
{
    public IntegrationEvent Create(WholesaleResult wholesaleResult)
    {
        var @event = CreateInternal(wholesaleResult);
        return new IntegrationEvent(Guid.NewGuid(), CalculationResultCompleted.EventName, CalculationResultCompleted.EventMinorVersion,  @event);
    }

    private static AmountPerChargeResultProducedV1 CreateInternal(WholesaleResult result)
    {
        var @event = new AmountPerChargeResultProducedV1()
        {
            CalculationId = result.CalculationId.ToString(),
            Resolution = AmountPerChargeResultProducedV1.Types.Resolution.Hour,
            // CalculationType = CalculationTypeMapper.MapCalculationType(result.CalculationType),
            QuantityUnit = AmountPerChargeResultProducedV1.Types.QuantityUnit.Kwh,
            PeriodStartUtc = result.PeriodStart.ToTimestamp(),
            PeriodEndUtc = result.PeriodEnd.ToTimestamp(),
        };
        return @event;
    }
}
