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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Events.Application.CalculationResultPublishing;
using Energinet.DataHub.Wholesale.Events.Application.IntegrationEventsManagement;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;
using Google.Protobuf;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.EventPublishers
{
    public class CalculationResultCompletedToIntegrationEventFactory : ICalculationResultCompletedFactory
    {
        private readonly IClock _systemDateTimeProvider;
        private readonly ICalculationResultCompletedIntegrationEventFactory _calculationResultCompletedIntegrationEventFactory;

        public CalculationResultCompletedToIntegrationEventFactory(
            IClock systemDateTimeProvider,
            ICalculationResultCompletedIntegrationEventFactory calculationResultCompletedIntegrationEventFactory)
        {
            _systemDateTimeProvider = systemDateTimeProvider;
            _calculationResultCompletedIntegrationEventFactory = calculationResultCompletedIntegrationEventFactory;
        }

        public IntegrationEventDto CreateForEnergySupplier(CalculationResult result)
        {
            var @event = _calculationResultCompletedIntegrationEventFactory.CreateForEnergySupplier(result, result.EnergySupplierId!);
            return CreateIntegrationEvent(@event);
        }

        public IntegrationEventDto CreateForBalanceResponsibleParty(CalculationResult result)
        {
            var @event = _calculationResultCompletedIntegrationEventFactory.CreateForBalanceResponsibleParty(result, result.BalanceResponsibleId!);
            return CreateIntegrationEvent(@event);
        }

        public IntegrationEventDto CreateForTotalGridArea(CalculationResult result)
        {
            var @event = _calculationResultCompletedIntegrationEventFactory.CreateForGridArea(result);
            return CreateIntegrationEvent(@event);
        }

        public IntegrationEventDto CreateForEnergySupplierByBalanceResponsibleParty(
            CalculationResult result)
        {
            var @event = _calculationResultCompletedIntegrationEventFactory.CreateForEnergySupplierByBalanceResponsibleParty(
                result,
                result.EnergySupplierId!,
                result.BalanceResponsibleId!);
            return CreateIntegrationEvent(@event);
        }

        private IntegrationEventDto CreateIntegrationEvent(IMessage integrationEvent)
        {
            var messageType = CalculationResultCompleted.MessageType;
            var eventData = integrationEvent.ToByteArray();
            return new IntegrationEventDto(eventData, messageType, _systemDateTimeProvider.GetCurrentInstant());
        }
    }
}
