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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Events.Application.Communication;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories
{
    public class CalculationResultIntegrationEventFactory : ICalculationResultIntegrationEventFactory
    {
        private readonly ICalculationResultCompletedFactory _calculationResultCompletedFactory;
        private readonly IEnergyResultProducedV1Factory _energyResultProducedV1Factory;
        private readonly IWholesaleResultProducedV1Factory _wholesaleResultProducedV1Factory;

        public CalculationResultIntegrationEventFactory(ICalculationResultCompletedFactory calculationResultCompletedFactory, IEnergyResultProducedV1Factory energyResultProducedV1Factory, IWholesaleResultProducedV1Factory wholesaleResultProducedV1Factory)
        {
            _calculationResultCompletedFactory = calculationResultCompletedFactory;
            _energyResultProducedV1Factory = energyResultProducedV1Factory;
            _wholesaleResultProducedV1Factory = wholesaleResultProducedV1Factory;
        }

        public IntegrationEvent CreateEventForEnergyResultDeprecated(EnergyResult energyResult)
        {
            return _calculationResultCompletedFactory.Create(energyResult);
        }

        public IntegrationEvent CreateEventForEnergyResult(EnergyResult energyResult)
        {
            return _energyResultProducedV1Factory.Create(energyResult);
        }

        public IntegrationEvent CreateEventForWholesaleResult(WholesaleResult wholesaleResult)
        {
            return _wholesaleResultProducedV1Factory.Create(wholesaleResult);
        }
    }
}
