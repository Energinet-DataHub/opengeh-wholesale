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
    public class IntegrationEventFactory : IIntegrationEventFactory
    {
        private readonly ICalculationResultCompletedFactory _calculationResultCompletedFactory;
        private readonly IEnergyResultProducedV1Factory _energyResultProducedV1Factory;
        private readonly IAmountPerChargeResultProducedV1Factory _amountPerChargeResultProducedV1Factory;
        private readonly IMonthlyAmountPerChargeResultProducedV1Factory _monthlyAmountPerChargeResultProducedV1Factory;

        public IntegrationEventFactory(
            ICalculationResultCompletedFactory calculationResultCompletedFactory,
            IEnergyResultProducedV1Factory energyResultProducedV1Factory,
            IAmountPerChargeResultProducedV1Factory amountPerChargeResultProducedV1Factory,
            IMonthlyAmountPerChargeResultProducedV1Factory monthlyAmountPerChargeResultProducedV1Factory)
        {
            _calculationResultCompletedFactory = calculationResultCompletedFactory;
            _energyResultProducedV1Factory = energyResultProducedV1Factory;
            _amountPerChargeResultProducedV1Factory = amountPerChargeResultProducedV1Factory;
            _monthlyAmountPerChargeResultProducedV1Factory = monthlyAmountPerChargeResultProducedV1Factory;
        }

        public IntegrationEvent CreateCalculationResultCompleted(EnergyResult energyResult)
        {
            var calculationResultCompleted = _calculationResultCompletedFactory.Create(energyResult);
            return CreateIntegrationEvent(calculationResultCompleted);
        }

        public IntegrationEvent CreateEnergyResultProducedV1(EnergyResult energyResult)
        {
            var energyResultProduced = _energyResultProducedV1Factory.Create(energyResult);
            return CreateIntegrationEvent(energyResultProduced);
        }

        public IntegrationEvent CreateAmountPerChargeResultProducedV1(WholesaleResult wholesaleResult)
        {
            var amountPerChangeResultProduced = _amountPerChargeResultProducedV1Factory.Create(wholesaleResult);
            return CreateIntegrationEvent(amountPerChangeResultProduced);
        }

        public IntegrationEvent CreateMonthlyAmountPerChargeResultProducedV1(WholesaleResult wholesaleResult)
        {
            var monthlyAmountPerChargeResultProduced = _monthlyAmountPerChargeResultProducedV1Factory.Create(wholesaleResult);
            return CreateIntegrationEvent(monthlyAmountPerChargeResultProduced);
        }

        private IntegrationEvent CreateIntegrationEvent(IEventMessage eventMessage)
        {
            return new IntegrationEvent(
                Guid.NewGuid(),
                eventMessage.EventName,
                eventMessage.EventMinorVersion,
                eventMessage);
        }
    }
}
