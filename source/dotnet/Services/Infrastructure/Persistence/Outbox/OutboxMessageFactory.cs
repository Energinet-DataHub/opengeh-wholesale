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
using Energinet.DataHub.Wholesale.Infrastructure.Integration;
using Google.Protobuf;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox
{
    public class OutboxMessageFactory : IOutboxMessageFactory
    {
        private readonly IClock _systemDateTimeProvider;
        private readonly IProcessCompletedIntegrationEventMapper _processCompletedIntegrationEventMapper;

        public OutboxMessageFactory(IClock systemDateTimeProvider, IProcessCompletedIntegrationEventMapper processCompletedIntegrationEventMapper)
        {
            _systemDateTimeProvider = systemDateTimeProvider;
            _processCompletedIntegrationEventMapper = processCompletedIntegrationEventMapper;
        }

        public OutboxMessage CreateFrom(ProcessCompletedEventDto processCompletedEventDto)
        {
            var integrationEvent = _processCompletedIntegrationEventMapper.MapFrom(processCompletedEventDto);
            var messageType = GetMessageType(processCompletedEventDto.ProcessType);
            return new OutboxMessage(messageType, integrationEvent.ToByteArray(), _systemDateTimeProvider.GetCurrentInstant());
        }

        private static string GetMessageType(Energinet.DataHub.Wholesale.Contracts.ProcessType processType) =>
            processType switch
            {
                Contracts.ProcessType.BalanceFixing => ProcessCompleted.BalanceFixingProcessType,
                Contracts.ProcessType.Aggregation => ProcessCompleted.AggregationProcessType,
                _ => throw new NotImplementedException($"Process type '{processType}' not implemented"),
            };
    }
}
