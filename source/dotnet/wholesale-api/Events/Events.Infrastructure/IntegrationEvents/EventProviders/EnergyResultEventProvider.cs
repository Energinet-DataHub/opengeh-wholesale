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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EventProviders
{
    public class EnergyResultEventProvider : ResultEventProvider, IEnergyResultEventProvider
    {
        private readonly IEnergyResultQueries _energyResultQueries;
        private readonly ICalculationResultCompletedFactory _calculationResultCompletedFactory;
        private readonly IEnergyResultProducedV1Factory _energyResultProducedV1Factory;

        public EnergyResultEventProvider(
            IEnergyResultQueries energyResultQueries,
            ICalculationResultCompletedFactory calculationResultCompletedFactory,
            IEnergyResultProducedV1Factory energyResultProducedV1Factory)
        {
            _energyResultQueries = energyResultQueries;
            _calculationResultCompletedFactory = calculationResultCompletedFactory;
            _energyResultProducedV1Factory = energyResultProducedV1Factory;
        }

        public async IAsyncEnumerable<IntegrationEvent> GetAsync(CompletedBatch batch, EventProviderState state)
        {
            await foreach (var energyResult in _energyResultQueries.GetAsync(batch.Id).ConfigureAwait(false))
            {
                state.EventCount++;
                yield return CreateIntegrationEvent(_calculationResultCompletedFactory.Create(energyResult)); // Deprecated
                yield return CreateIntegrationEvent(_energyResultProducedV1Factory.Create(energyResult));
            }
        }
    }
}
