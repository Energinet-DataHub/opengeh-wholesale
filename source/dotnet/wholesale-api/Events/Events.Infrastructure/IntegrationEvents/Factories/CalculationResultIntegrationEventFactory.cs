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
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Google.Protobuf;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories
{
    public class CalculationResultIntegrationEventFactory : ICalculationResultIntegrationEventFactory
    {
        private readonly ICalculationResultCompletedFactory _calculationResultCompletedFactory;
        private readonly IEnergyResultProducedV1Factory _energyResultProducedV1Factory;
        private readonly IAmountPerChargeResultProducedV1Factory _amountPerChargeResultProducedV1Factory;
        private readonly IMonthlyAmountPerChargeResultProducedV1Factory _monthlyAmountPerChargeResultProducedV1Factory;

        public CalculationResultIntegrationEventFactory(
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
            var eventIdentification = Guid.NewGuid();
            return CreateIntegrationEvent(calculationResultCompleted, eventIdentification, CalculationResultCompleted.EventName, CalculationResultCompleted.EventMinorVersion);
        }

        public IntegrationEvent CreateEnergyResultProducedV1(EnergyResult energyResult)
        {
            var calculationResultCompleted = _energyResultProducedV1Factory.Create(energyResult);
            var eventIdentification = Guid.NewGuid();
            return CreateIntegrationEvent(calculationResultCompleted, eventIdentification, EnergyResultProducedV1.EventName, EnergyResultProducedV1.EventMinorVersion);
        }

        public IntegrationEvent CreateAmountPerChargeResultProducedV1(WholesaleResult wholesaleResult)
        {
            var calculationResultCompleted = _amountPerChargeResultProducedV1Factory.Create(wholesaleResult);
            var eventIdentification = Guid.NewGuid();
            return CreateIntegrationEvent(calculationResultCompleted, eventIdentification, AmountPerChargeResultProducedV1.EventName, AmountPerChargeResultProducedV1.EventMinorVersion);
        }

        public IntegrationEvent CreateMonthlyAmountPerChargeResultProducedV1(WholesaleResult wholesaleResult)
        {
            var calculationResultCompleted = _monthlyAmountPerChargeResultProducedV1Factory.Create(wholesaleResult);
            var eventIdentification = Guid.NewGuid();
            return CreateIntegrationEvent(calculationResultCompleted, eventIdentification, MonthlyAmountPerChargeResultProducedV1.EventName, MonthlyAmountPerChargeResultProducedV1.EventMinorVersion);
        }

        private IntegrationEvent CreateIntegrationEvent(IMessage protobufMessage, Guid eventIdentification, string eventName, int eventMinorVersion)
        {
            return new IntegrationEvent(eventIdentification, eventName, eventMinorVersion, protobufMessage);
        }
    }
}
