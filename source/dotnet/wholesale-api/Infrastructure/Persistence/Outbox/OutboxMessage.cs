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
using NodaTime;

namespace Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox
{
    public class OutboxMessage
    {
        public OutboxMessage(IntegrationEventDto integrationEventDto)
        {
            Id = Guid.NewGuid();
            Data = integrationEventDto.ProtobufEventData;
            CreationDate = integrationEventDto.CreationDate;
            MessageType = integrationEventDto.EventMessageType;
        }

        public Guid Id { get; }

        public string MessageType { get; }

        public byte[] Data { get; }

        public Instant CreationDate { get; }

        public Instant? ProcessedDate { get; private set; }

        public void SetProcessed(Instant when)
        {
            ProcessedDate = when;
        }

        /// <summary>
        /// Required by Entity Framework
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private OutboxMessage(
            Guid id,
            byte[] data,
            string messageType,
            Instant creationDate,
            Instant? processedDate)
        {
            Id = id;
            Data = data;
            MessageType = messageType;
            CreationDate = creationDate;
            ProcessedDate = processedDate;
        }
    }
}
