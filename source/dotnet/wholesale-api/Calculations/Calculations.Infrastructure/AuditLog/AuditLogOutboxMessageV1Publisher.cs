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
using Energinet.DataHub.Core.Outbox.Abstractions;
using Energinet.DataHub.RevisionLog.Integration;
using Energinet.DataHub.Wholesale.Calculations.Application.AuditLog;

namespace Energinet.DataHub.Wholesale.Calculations.Infrastructure.AuditLog;

public class AuditLogOutboxMessageV1Publisher
    : IOutboxPublisher
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IRevisionLogClient _revisionLogClient;

    public AuditLogOutboxMessageV1Publisher(
        IJsonSerializer jsonSerializer,
        IRevisionLogClient revisionLogClient)
    {
        _jsonSerializer = jsonSerializer;
        _revisionLogClient = revisionLogClient;
    }

    public bool CanPublish(string type) => type.Equals(AuditLogOutboxMessageV1.OutboxMessageType);

    public Task PublishAsync(string serializedPayload)
    {
        var payload = _jsonSerializer.Deserialize<AuditLogOutboxMessageV1Payload>(serializedPayload);

        return _revisionLogClient.LogAsync(new RevisionLogEntry(
            logId: payload.LogId,
            systemId: payload.SystemId,
            activity: payload.Activity,
            occurredOn: payload.OccuredOn,
            origin: payload.Origin,
            payload: payload.Payload,
            userId: payload.UserId,
            actorId: payload.ActorId,
            actorNumber: payload.ActorNumber,
            marketRoles: payload.MarketRoles,
            permissions: payload.Permissions,
            affectedEntityType: payload.AffectedEntityType,
            affectedEntityKey: payload.AffectedEntityKey));
    }
}
