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
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Google.Protobuf;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories
{
    public class CalculationResultIntegrationEventFactory : ICalculationResultIntegrationEventFactory
    {
        private readonly IClock _systemDateTimeProvider;
        private readonly ICalculationResultCompletedIntegrationEventFactory _calculationResultCompletedIntegrationEventFactory;

        public CalculationResultIntegrationEventFactory(
            IClock systemDateTimeProvider,
            ICalculationResultCompletedIntegrationEventFactory calculationResultCompletedIntegrationEventFactory)
        {
            _systemDateTimeProvider = systemDateTimeProvider;
            _calculationResultCompletedIntegrationEventFactory = calculationResultCompletedIntegrationEventFactory;
        }

        public IntegrationEvent Create(CalculationResult calculationResult)
        {
            if (calculationResult.EnergySupplierId == null && calculationResult.BalanceResponsibleId == null)
                return CreateForTotalGridArea(calculationResult);

            if (calculationResult.EnergySupplierId != null && calculationResult.BalanceResponsibleId == null)
                return CreateForEnergySupplier(calculationResult);

            if (calculationResult.EnergySupplierId == null && calculationResult.BalanceResponsibleId != null)
                return CreateForBalanceResponsibleParty(calculationResult);

            return CreateForEnergySupplierByBalanceResponsibleParty(calculationResult);
        }

        private IntegrationEvent CreateForEnergySupplier(CalculationResult result)
        {
            var @event = _calculationResultCompletedIntegrationEventFactory.CreateForEnergySupplier(result, result.EnergySupplierId!);
            return CreateIntegrationEvent(@event, result);
        }

        private IntegrationEvent CreateForBalanceResponsibleParty(CalculationResult result)
        {
            var @event = _calculationResultCompletedIntegrationEventFactory.CreateForBalanceResponsibleParty(result, result.BalanceResponsibleId!);
            return CreateIntegrationEvent(@event, result);
        }

        private IntegrationEvent CreateForTotalGridArea(CalculationResult result)
        {
            var @event = _calculationResultCompletedIntegrationEventFactory.CreateForGridArea(result);
            return CreateIntegrationEvent(@event, result);
        }

        private IntegrationEvent CreateForEnergySupplierByBalanceResponsibleParty(
            CalculationResult result)
        {
            var @event = _calculationResultCompletedIntegrationEventFactory.CreateForEnergySupplierByBalanceResponsibleParty(
                result,
                result.EnergySupplierId!,
                result.BalanceResponsibleId!);
            return CreateIntegrationEvent(@event, result);
        }

        private IntegrationEvent CreateIntegrationEvent(IMessage protobufMessage, CalculationResult calculationResult)
        {
            var eventIdentification = calculationResult.Id;
            var messageName = CalculationResultCompleted.MessageName;
            var messageVersion = CalculationResultCompleted.MessageVersion;
            var operationTimeStamp = _systemDateTimeProvider.GetCurrentInstant();
            return new IntegrationEvent(eventIdentification, messageName, operationTimeStamp, messageVersion, protobufMessage);
        }
    }
}
