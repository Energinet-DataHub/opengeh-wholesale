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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application.IntegrationEventsManagement;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application.Processes;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application.Processes.Model;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Infrastructure.Integration;
using Google.Protobuf;
using NodaTime;

namespace Energinet.DataHub.Wholesale.IntegrationEventPublishing.Infrastructure.EventPublishers
{
    public class CalculationResultCompletedToIntegrationEventFactory : ICalculationResultCompletedFactory
    {
        private readonly IClock _systemDateTimeProvider;
        private readonly ICalculationResultCompletedIntegrationEventFactory _calculationResultCompletedIntegrationEventFactory;
        private readonly IIntegrationEventTypeMapper _integrationEventTypeMapper;

        public CalculationResultCompletedToIntegrationEventFactory(
            IClock systemDateTimeProvider,
            ICalculationResultCompletedIntegrationEventFactory calculationResultCompletedIntegrationEventFactory,
            IIntegrationEventTypeMapper integrationEventTypeMapper)
        {
            _systemDateTimeProvider = systemDateTimeProvider;
            _calculationResultCompletedIntegrationEventFactory = calculationResultCompletedIntegrationEventFactory;
            _integrationEventTypeMapper = integrationEventTypeMapper;
        }

        public IntegrationEventDto CreateForEnergySupplier(ProcessStepResult processStepResult, ProcessCompletedEventDto processCompletedEventDto, string energySupplierGln)
        {
            var result = _calculationResultCompletedIntegrationEventFactory.CreateForEnergySupplier(processStepResult, processCompletedEventDto, energySupplierGln);
            return CreateIntegrationEvent(result);
        }

        public IntegrationEventDto CreateForBalanceResponsibleParty(ProcessStepResult processStepResultDto, ProcessCompletedEventDto processCompletedEventDto, string balanceResponsiblePartyGln)
        {
            var result = _calculationResultCompletedIntegrationEventFactory.CreateForBalanceResponsibleParty(processStepResultDto, processCompletedEventDto, balanceResponsiblePartyGln);
            return CreateIntegrationEvent(result);
        }

        public IntegrationEventDto CreateForTotalGridArea(ProcessStepResult processStepResult, ProcessCompletedEventDto processCompletedEventDto)
        {
            var result = _calculationResultCompletedIntegrationEventFactory.CreateForGridArea(processStepResult, processCompletedEventDto);
            return CreateIntegrationEvent(result);
        }

        public IntegrationEventDto CreateForEnergySupplierByBalanceResponsibleParty(ProcessStepResult processStepResultDto, ProcessCompletedEventDto processCompletedEvent, string energySupplierGln, string brpGln)
        {
            var result = _calculationResultCompletedIntegrationEventFactory.CreateForEnergySupplierByBalanceResponsibleParty(processStepResultDto, processCompletedEvent, energySupplierGln, brpGln);
            return CreateIntegrationEvent(result);
        }

        private IntegrationEventDto CreateIntegrationEvent(IMessage integrationEvent)
        {
            var messageType = _integrationEventTypeMapper.GetMessageType(integrationEvent.GetType());
            var eventData = integrationEvent.ToByteArray();
            return new IntegrationEventDto(eventData, messageType, _systemDateTimeProvider.GetCurrentInstant());
        }
    }
}
