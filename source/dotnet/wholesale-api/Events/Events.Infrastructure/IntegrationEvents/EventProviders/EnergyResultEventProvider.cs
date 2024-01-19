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
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.CalculationResultCompleted.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EnergyResultProducedV2.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.GridLossResultProducedV1.Factories;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EventProviders
{
    public class EnergyResultEventProvider : ResultEventProvider, IEnergyResultEventProvider
    {
        private readonly IEnergyResultQueries _energyResultQueries;
        private readonly ICalculationResultCompletedFactory _calculationResultCompletedFactory;
        private readonly IEnergyResultProducedV2Factory _energyResultProducedV2Factory;
        private readonly IGridLossResultProducedV1Factory _gridLossResultProducedV2Factory;

        public EnergyResultEventProvider(
            IEnergyResultQueries energyResultQueries,
            ICalculationResultCompletedFactory calculationResultCompletedFactory,
            IEnergyResultProducedV2Factory energyResultProducedV2Factory,
            IGridLossResultProducedV1Factory gridLossResultProducedV2Factory)
        {
            _energyResultQueries = energyResultQueries;
            _calculationResultCompletedFactory = calculationResultCompletedFactory;
            _energyResultProducedV2Factory = energyResultProducedV2Factory;
            _gridLossResultProducedV2Factory = gridLossResultProducedV2Factory;
        }

        public async IAsyncEnumerable<IntegrationEvent> GetAsync(CompletedCalculation calculation)
        {
            await foreach (var energyResult in _energyResultQueries.GetAsync(calculation.Id).ConfigureAwait(false))
            {
                yield return CreateIntegrationEvent(_calculationResultCompletedFactory.Create(energyResult)); // Deprecated
                yield return CreateIntegrationEvent(_energyResultProducedV2Factory.Create(energyResult));
                if (_gridLossResultProducedV2Factory.CanCreate(energyResult))
                    yield return CreateIntegrationEvent(_gridLossResultProducedV2Factory.Create(energyResult));
            }
        }
    }
}
