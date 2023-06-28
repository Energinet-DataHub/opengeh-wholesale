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

using System.Security.Cryptography;
using System.Text;
using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Events.Application.CalculationResultPublishing;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;
using Google.Protobuf;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.EventPublishers
{
    public class CalculationResultIntegrationEventToIntegrationEventFactory : ICalculationResultIntegrationEventFactory
    {
        private readonly IClock _systemDateTimeProvider;
        private readonly ICalculationResultCompletedIntegrationEventFactory _calculationResultCompletedIntegrationEventFactory;

        public CalculationResultIntegrationEventToIntegrationEventFactory(
            IClock systemDateTimeProvider,
            ICalculationResultCompletedIntegrationEventFactory calculationResultCompletedIntegrationEventFactory)
        {
            _systemDateTimeProvider = systemDateTimeProvider;
            _calculationResultCompletedIntegrationEventFactory = calculationResultCompletedIntegrationEventFactory;
        }

        public IntegrationEvent CreateForEnergySupplier(CalculationResult result)
        {
            var @event = _calculationResultCompletedIntegrationEventFactory.CreateForEnergySupplier(result, result.EnergySupplierId!);
            return CreateIntegrationEvent(@event, result);
        }

        public IntegrationEvent CreateForBalanceResponsibleParty(CalculationResult result)
        {
            var @event = _calculationResultCompletedIntegrationEventFactory.CreateForBalanceResponsibleParty(result, result.BalanceResponsibleId!);
            return CreateIntegrationEvent(@event, result);
        }

        public IntegrationEvent CreateForTotalGridArea(CalculationResult result)
        {
            var @event = _calculationResultCompletedIntegrationEventFactory.CreateForGridArea(result);
            return CreateIntegrationEvent(@event, result);
        }

        public IntegrationEvent CreateForEnergySupplierByBalanceResponsibleParty(
            CalculationResult result)
        {
            var @event = _calculationResultCompletedIntegrationEventFactory.CreateForEnergySupplierByBalanceResponsibleParty(
                result,
                result.EnergySupplierId!,
                result.BalanceResponsibleId!);
            return CreateIntegrationEvent(@event, result);
        }

        /// <summary>
        /// Only made public for testing purposes while waiting to be replaced by CalculationResult.Id.
        /// </summary>
        public static Guid GetEventIdentification(CalculationResult calculationResult)
        {
            var uniqueConsistentIdentifier = $"{calculationResult.BatchId}-{calculationResult.GridArea}-{calculationResult.EnergySupplierId}-{calculationResult.BalanceResponsibleId}{calculationResult.TimeSeriesType}";
            using var hasher = SHA256.Create();
            var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(uniqueConsistentIdentifier));
            return new Guid(hash.Take(16).ToArray());
        }

        private IntegrationEvent CreateIntegrationEvent(IMessage protobufMessage, CalculationResult calculationResult)
        {
            var eventIdentification = GetEventIdentification(calculationResult);
            var messageName = CalculationResultCompleted.MessageName;
            var messageVersion = CalculationResultCompleted.MessageVersion;
            var operationTimeStamp = _systemDateTimeProvider.GetCurrentInstant();
            return new IntegrationEvent(eventIdentification, messageName, operationTimeStamp, messageVersion, protobufMessage);
        }
    }
}
