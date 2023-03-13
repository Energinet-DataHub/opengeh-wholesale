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
        private readonly IProcessCompletedIntegrationEventMapper _processCompletedIntegrationEventMapper;
        private readonly ICalculationResultReadyIntegrationEventFactory _calculationResultReadyIntegrationEventFactory;
        private readonly IServiceBusMessageFactory _serviceBusMessageFactory;

        public OutboxMessageFactory(
            IClock systemDateTimeProvider,
            IProcessCompletedIntegrationEventMapper processCompletedIntegrationEventMapper,
            ICalculationResultReadyIntegrationEventFactory calculationResultReadyIntegrationEventFactory,
            IServiceBusMessageFactory serviceBusMessageFactory)
        {
            _systemDateTimeProvider = systemDateTimeProvider;
            _processCompletedIntegrationEventMapper = processCompletedIntegrationEventMapper;
            _calculationResultReadyIntegrationEventFactory = calculationResultReadyIntegrationEventFactory;
            _serviceBusMessageFactory = serviceBusMessageFactory;
        }

        public OutboxMessage CreateFrom(ProcessCompletedEventDto processCompletedEventDto)
        {
            var integrationEvent = _processCompletedIntegrationEventMapper.MapFrom(processCompletedEventDto);
            var messageType = GetMessageTypeForCalculationResultCompletedEvent(processCompletedEventDto.ProcessType);
            return new OutboxMessage(messageType, integrationEvent.ToByteArray(), _systemDateTimeProvider.GetCurrentInstant());
        }

        public OutboxMessage CreateFrom(ProcessStepResult processStepResult, ProcessCompletedEventDto processCompletedEventDto)
        {
            var integrationEvent = _calculationResultReadyIntegrationEventFactory
                .CreateCalculationResultCompletedForGridArea(processStepResult, processCompletedEventDto);
            var messageType = GetMessageTypeForCalculationResultCompletedEvent(processCompletedEventDto.ProcessType);
            return new OutboxMessage(messageType, integrationEvent.ToByteArray(), _systemDateTimeProvider.GetCurrentInstant());
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
