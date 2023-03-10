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

using Energinet.DataHub.Wholesale.Domain;
using Google.Protobuf;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox
{
    public class OutboxMessageFactory : IOutboxMessageFactory
    {
        private readonly IClock _systemDateTimeProvider;

        public OutboxMessageFactory(IClock systemDateTimeProvider)
        {
            _systemDateTimeProvider = systemDateTimeProvider;
        }

        public OutboxMessage CreateFrom(IMessage message)
        {
            // TODO AWJ
            return new OutboxMessage(message.ToByteArray(), _systemDateTimeProvider.GetCurrentInstant());
        }
    }
}
