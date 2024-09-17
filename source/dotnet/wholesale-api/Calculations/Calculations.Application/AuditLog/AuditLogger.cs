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
using Energinet.DataHub.Wholesale.Calculations.Interfaces.AuditLog;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Calculations.Application.AuditLog;

public class AuditLogger(
    IJsonSerializer jsonSerializer,
    IClock clock,
    IOutboxClient outboxClient,
    IUnitOfWork unitOfWork,
    IAuditUserContext auditUserContext)
    : IAuditLogger
{
    private static readonly Guid _wholesaleSystemId = Guid.Parse("467ab87d-9494-4add-bf01-703540067b9e");

    private readonly IJsonSerializer _jsonSerializer = jsonSerializer;
    private readonly IClock _clock = clock;
    private readonly IOutboxClient _outboxClient = outboxClient;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IAuditUserContext _auditUserContext = auditUserContext
                ?? throw new NullReferenceException("IAuditUserContext is required to use AuditLogger. " +
                                                    "Did you register an implementation of it in the DI container?");

    public async Task LogWithCommitAsync(
        AuditLogActivity activity,
        string origin,
        object? payload,
        AuditLogEntityType? affectedEntityType,
        Guid? affectedEntityKey)
    {
        var auditLogUser = _auditUserContext.CurrentUser;

        var payloadAsJson = payload switch
        {
            null => string.Empty,
            string p => p,
            _ => _jsonSerializer.Serialize(payload),
        };

        var auditLogOutboxMessage = new AuditLogOutboxMessageV1(
            serializer: _jsonSerializer,
            systemId: _wholesaleSystemId,
            activity: activity.Identifier,
            origin: origin,
            payload: payloadAsJson,
            logId: Guid.NewGuid(),
            occuredOn: _clock.GetCurrentInstant(),
            userId: auditLogUser.UserId,
            actorId: auditLogUser.ActorId,
            actorNumber: auditLogUser.ActorNumber,
            marketRoles: auditLogUser.MarketRoles,
            permissions: auditLogUser.Permissions,
            affectedEntityType: affectedEntityType?.Identifier,
            affectedEntityKey: affectedEntityKey?.ToString());

        await _outboxClient.AddToOutboxAsync(auditLogOutboxMessage)
            .ConfigureAwait(false);

        await _unitOfWork.CommitAsync()
            .ConfigureAwait(false);
    }
}
