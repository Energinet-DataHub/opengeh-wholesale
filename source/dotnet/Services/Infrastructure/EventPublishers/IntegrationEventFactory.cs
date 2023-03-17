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

using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Integration;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Infrastructure.EventPublishers
{
    public class IntegrationEventFactory : IIntegrationEventFactory
    {
        private readonly IClock _systemDateTimeProvider;
        private readonly ICalculationResultReadyIntegrationEventFactory _calculationResultReadyIntegrationEventFactory;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IIntegrationEventTypeMapper _integrationEventTypeMapper;

        public IntegrationEventFactory(
            IClock systemDateTimeProvider,
            ICalculationResultReadyIntegrationEventFactory calculationResultReadyIntegrationEventFactory,
            IJsonSerializer jsonSerializer,
            IIntegrationEventTypeMapper integrationEventTypeMapper)
        {
            _systemDateTimeProvider = systemDateTimeProvider;
            _calculationResultReadyIntegrationEventFactory = calculationResultReadyIntegrationEventFactory;
            _jsonSerializer = jsonSerializer;
            _integrationEventTypeMapper = integrationEventTypeMapper;
        }

        public IntegrationEventDto CreateIntegrationEventForCalculationResultForEnergySupplier(ProcessStepResult processStepResult, ProcessCompletedEventDto processCompletedEventDto, string energySupplierGln)
        {
            var result = _calculationResultReadyIntegrationEventFactory.CreateCalculationResultCompletedForEnergySupplier(processStepResult, processCompletedEventDto, energySupplierGln);
            return CreateIntegrationEvent(result);
        }

        public IntegrationEventDto CreateIntegrationEventForCalculationResultForBalanceResponsibleParty(ProcessStepResult processStepResultDto, ProcessCompletedEventDto processCompletedEventDto, string balanceResponsiblePartyGln)
        {
            var result = _calculationResultReadyIntegrationEventFactory.CreateCalculationResultCompletedForBalanceResponsibleParty(processStepResultDto, processCompletedEventDto, balanceResponsiblePartyGln);
            return CreateIntegrationEvent(result);
        }

        public IntegrationEventDto CreateIntegrationEventForCalculationResultForTotalGridArea(ProcessStepResult processStepResult, ProcessCompletedEventDto processCompletedEventDto)
        {
            var result = _calculationResultReadyIntegrationEventFactory.CreateCalculationResultCompletedForGridArea(processStepResult, processCompletedEventDto);
            return CreateIntegrationEvent(result);
        }

        public IntegrationEventDto CreateIntegrationEventForCalculationResultForEnergySupplierByBalanceResponsibleParty(ProcessStepResult processStepResultDto, ProcessCompletedEventDto processCompletedEvent, string energySupplierGln, string brpGln)
        {
            var result = _calculationResultReadyIntegrationEventFactory.CreateCalculationResultForEnergySupplierByBalanceResponsibleParty(processStepResultDto, processCompletedEvent, energySupplierGln, brpGln);
            return CreateIntegrationEvent(result);
        }

        public IntegrationEventDto CreateProcessCompletedIntegrationEvent(ProcessCompletedEventDto processCompletedEvent)
        {
            return CreateIntegrationEvent(processCompletedEvent);
        }

        private IntegrationEventDto CreateIntegrationEvent<TIntegrationEvent>(TIntegrationEvent integrationEvent)
        {
            var messageType = _integrationEventTypeMapper.GetEventName(typeof(TIntegrationEvent));
            var serializedIntegrationEvent = _jsonSerializer.Serialize(integrationEvent);
            return new IntegrationEventDto(messageType, serializedIntegrationEvent, _systemDateTimeProvider.GetCurrentInstant());
        }
    }
}
