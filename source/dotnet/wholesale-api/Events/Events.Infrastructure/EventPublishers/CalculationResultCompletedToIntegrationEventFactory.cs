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
using Energinet.DataHub.Wholesale.Events.Application.CalculationResultPublishing.Model;
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

        public IntegrationEventDto CreateForEnergySupplier(CalculationResult calculationResult, BatchGridAreaInfo batchGridAreaInfo, string energySupplierGln)
        {
            var result = _calculationResultCompletedIntegrationEventFactory.CreateForEnergySupplier(calculationResult, batchGridAreaInfo, energySupplierGln);
            return CreateIntegrationEvent(result);
        }

        public IntegrationEventDto CreateForBalanceResponsibleParty(CalculationResult calculationResultDto, BatchGridAreaInfo batchGridAreaInfo, string balanceResponsiblePartyGln)
        {
            var result = _calculationResultCompletedIntegrationEventFactory.CreateForBalanceResponsibleParty(calculationResultDto, batchGridAreaInfo, balanceResponsiblePartyGln);
            return CreateIntegrationEvent(result);
        }

        public IntegrationEventDto CreateForTotalGridArea(CalculationResult calculationResult, BatchGridAreaInfo batchGridAreaInfo)
        {
            var result = _calculationResultCompletedIntegrationEventFactory.CreateForGridArea(calculationResult, batchGridAreaInfo);
            return CreateIntegrationEvent(result);
        }

        public IntegrationEventDto CreateForEnergySupplierByBalanceResponsibleParty(CalculationResult calculationResultDto, BatchGridAreaInfo batchGridAreaInfo, string energySupplierGln, string brpGln)
        {
            var result = _calculationResultCompletedIntegrationEventFactory.CreateForEnergySupplierByBalanceResponsibleParty(calculationResultDto, batchGridAreaInfo, energySupplierGln, brpGln);
            return CreateIntegrationEvent(result);
        }

        private IntegrationEventDto CreateIntegrationEvent(IMessage integrationEvent)
        {
            var messageType = CalculationResultCompleted.MessageType;
            var eventData = integrationEvent.ToByteArray();
            return new IntegrationEventDto(eventData, messageType, _systemDateTimeProvider.GetCurrentInstant());
        }
    }
}
