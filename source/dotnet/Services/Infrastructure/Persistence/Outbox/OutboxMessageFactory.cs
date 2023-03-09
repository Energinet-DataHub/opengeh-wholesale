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
using NodaTime;

namespace Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox
{
    public class OutboxMessageFactory
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IClock _systemDateTimeProvider;

        public OutboxMessageFactory(IJsonSerializer jsonSerializer, IClock systemDateTimeProvider)
        {
            _jsonSerializer = jsonSerializer;
            _systemDateTimeProvider = systemDateTimeProvider;
        }

        public OutboxMessage CreateFrom<T>(T message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var type = message.GetType().FullName;
            if (string.IsNullOrEmpty(type))
            {
                throw new OutboxMessageException("Failed to extract message type name.");
            }

            var data = _jsonSerializer.Serialize(message);

            return new OutboxMessage(type, data, _systemDateTimeProvider.GetCurrentInstant());
        }

        public OutboxMessage CreateFrom(string message, string messageType)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (messageType == null) throw new ArgumentNullException(nameof(messageType));

            return new OutboxMessage(messageType, message, _systemDateTimeProvider.GetCurrentInstant());
        }
    }
}
