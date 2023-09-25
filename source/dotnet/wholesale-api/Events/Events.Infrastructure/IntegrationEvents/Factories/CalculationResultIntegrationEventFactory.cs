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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Google.Protobuf;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories
{
    public class CalculationResultIntegrationEventFactory : ICalculationResultIntegrationEventFactory
    {
        private readonly ICalculationResultCompletedFactory _calculationResultCompletedFactory;
        private readonly IEnergyResultProducedFactory _energyResultProducedFactory;

        public CalculationResultIntegrationEventFactory(ICalculationResultCompletedFactory calculationResultCompletedFactory, IEnergyResultProducedFactory energyResultProducedFactory)
        {
            _calculationResultCompletedFactory = calculationResultCompletedFactory;
            _energyResultProducedFactory = energyResultProducedFactory;
        }

        public IntegrationEvent CreateCalculationResultCompleted(EnergyResult energyResult)
        {
            var calculationResultCompleted = _calculationResultCompletedFactory.Create(energyResult);
            var eventIdentification = Guid.NewGuid();
            return CreateIntegrationEvent(calculationResultCompleted, eventIdentification, CalculationResultCompleted.EventName, CalculationResultCompleted.EventMinorVersion);
        }

        public IntegrationEvent CreateEnergyResultProduced(EnergyResult energyResult)
        {
            var calculationResultCompleted = _energyResultProducedFactory.Create(energyResult);
            var eventIdentification = Guid.NewGuid();
            return CreateIntegrationEvent(calculationResultCompleted, eventIdentification, EnergyResultProducedV1.EventName, EnergyResultProducedV1.EventMinorVersion);
        }

        private IntegrationEvent CreateIntegrationEvent(IMessage protobufMessage, Guid eventIdentification, string eventName, int eventMinorVersion)
        {
            return new IntegrationEvent(eventIdentification, eventName, eventMinorVersion, protobufMessage);
        }
    }
}
