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

using System.Text;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Domain;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Integration;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using Google.Protobuf;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox
{
    public class OutboxMessageFactory : IOutboxMessageFactory
    {
        private readonly IClock _systemDateTimeProvider;
        private readonly ICalculationResultReadyIntegrationEventFactory _calculationResultReadyIntegrationEventFactory;
        private readonly IServiceBusMessageFactory _serviceBusMessageFactory;
        private readonly IJsonSerializer _jsonSerializer;

        public OutboxMessageFactory(
            IClock systemDateTimeProvider,
            ICalculationResultReadyIntegrationEventFactory calculationResultReadyIntegrationEventFactory,
            IServiceBusMessageFactory serviceBusMessageFactory,
            IJsonSerializer jsonSerializer)
        {
            _systemDateTimeProvider = systemDateTimeProvider;
            _calculationResultReadyIntegrationEventFactory = calculationResultReadyIntegrationEventFactory;
            _serviceBusMessageFactory = serviceBusMessageFactory;
            _jsonSerializer = jsonSerializer;
        }

        public OutboxMessage CreateFrom(ProcessStepResult processStepResult, ProcessCompletedEventDto processCompletedEventDto)
        {
            var integrationEvent = _calculationResultReadyIntegrationEventFactory.CreateCalculationResultCompletedForGridArea(processStepResult, processCompletedEventDto);
            return CreateOutboxMessage(processCompletedEventDto, integrationEvent);
        }

        public OutboxMessage CreateMessageCalculationResultForEnergySupplier(ProcessStepResult processStepResult, ProcessCompletedEventDto processCompletedEventDto, string energySupplierGln)
        {
            var integrationEvent = _calculationResultReadyIntegrationEventFactory.CreateCalculationResultCompletedForEnergySupplier(processStepResult, processCompletedEventDto, energySupplierGln);
            return CreateOutboxMessage(processCompletedEventDto, integrationEvent);
        }

        public OutboxMessage CreateMessageForCalculationResultForBalanceResponsibleParty(ProcessStepResult processStepResultDto, ProcessCompletedEventDto processCompletedEventDto, string balanceResponsiblePartyGln)
        {
            var integrationEvent = _calculationResultReadyIntegrationEventFactory.CreateCalculationResultCompletedForBalanceResponsibleParty(processStepResultDto, processCompletedEventDto, balanceResponsiblePartyGln);
            return CreateOutboxMessage(processCompletedEventDto, integrationEvent);
        }

        public OutboxMessage CreateMessageCalculationResultForTotalGridArea(ProcessStepResult processStepResult, ProcessCompletedEventDto processCompletedEventDto)
        {
            var integrationEvent = _calculationResultReadyIntegrationEventFactory.CreateCalculationResultCompletedForGridArea(processStepResult, processCompletedEventDto);
            return CreateOutboxMessage(processCompletedEventDto, integrationEvent);
        }

        private OutboxMessage CreateOutboxMessage(ProcessCompletedEventDto processCompletedEventDto, IMessage integrationEvent)
        {
            var messageType = GetMessageTypeForCalculationResultCompletedEvent(processCompletedEventDto.ProcessType);
            var serviceBusMessage = _serviceBusMessageFactory.CreateProcessCompleted(integrationEvent.ToByteArray(), messageType);
            var serialized = _jsonSerializer.Serialize(serviceBusMessage);
            var bytes = Encoding.UTF8.GetBytes(serialized);
            // TODO AJH var bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(serviceBusMessage);
            return new OutboxMessage(messageType, bytes, _systemDateTimeProvider.GetCurrentInstant());
        }

        private static string GetMessageTypeForCalculationResultCompletedEvent(Energinet.DataHub.Wholesale.Contracts.ProcessType processType) =>
            processType switch
            {
                Contracts.ProcessType.BalanceFixing => CalculationResultCompleted.BalanceFixingEventName,
                Contracts.ProcessType.Aggregation => CalculationResultCompleted.AggregationEventName,
                _ => throw new NotImplementedException($"Process type '{processType}' not implemented"),
            };
    }
}
