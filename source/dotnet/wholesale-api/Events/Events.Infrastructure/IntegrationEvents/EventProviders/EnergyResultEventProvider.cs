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

using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.GridLossResultProducedV1.Factories;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EventProviders;

public class EnergyResultEventProvider(
    IEnergyResultQueries energyResultQueries,
    IGridLossResultProducedV1Factory gridLossResultProducedV2Factory)
    : IEnergyResultEventProvider
{
    public async IAsyncEnumerable<IntegrationEvent> GetAsync(Guid calculationId)
    {
        await foreach (var energyResult in energyResultQueries.GetAsync(calculationId).ConfigureAwait(false))
        {
            if (gridLossResultProducedV2Factory.CanCreate(energyResult))
                yield return CreateIntegrationEvent(eventId: energyResult.Id, gridLossResultProducedV2Factory.Create(energyResult));
        }
    }

    private static IntegrationEvent CreateIntegrationEvent(Guid eventId, IEventMessage eventMessage)
    {
        return new IntegrationEvent(
            eventId,
            eventMessage.EventName,
            eventMessage.EventMinorVersion,
            eventMessage);
    }
}
