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
using NodaTime;

namespace Energinet.DataHub.Wholesale.Calculations.Application.AuditLog;

public class AuditLogOutboxMessageV1
    : IOutboxMessage<AuditLogOutboxMessageV1Payload>
{
    public const string OutboxMessageType = "AuditLogOutboxMessageV1";

    private readonly IJsonSerializer _serializer;

    public AuditLogOutboxMessageV1(
        IJsonSerializer serializer,
        Guid systemId,
        string activity,
        string origin,
        string payload,
        Guid logId,
        Instant occuredOn,
        Guid userId,
        Guid actorId,
        string? actorNumber,
        string? marketRoles,
        string? permissions,
        string? affectedEntityType,
        string? affectedEntityKey)
    {
        _serializer = serializer;
        Payload = new AuditLogOutboxMessageV1Payload(
            systemId,
            activity,
            origin,
            payload,
            logId,
            occuredOn,
            userId,
            actorId,
            actorNumber,
            marketRoles,
            permissions,
            affectedEntityType,
            affectedEntityKey);
    }

    public string Type => OutboxMessageType;

    public AuditLogOutboxMessageV1Payload Payload { get; }

    public Task<string> SerializeAsync()
    {
        var serialized = _serializer.Serialize(Payload);

        return Task.FromResult(serialized);
    }
}
