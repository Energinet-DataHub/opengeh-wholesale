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

using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Core.Outbox.Abstractions;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.AuditLog;
using Energinet.DataHub.Wholesale.Common.Interfaces.Security;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Calculations.Application.AuditLog;

public class AuditLogger(
    IJsonSerializer jsonSerializer,
    IClock clock,
    IOutboxClient outboxClient,
    IUnitOfWork unitOfWork,
    IUserContext<FrontendUser> userContext)
    : IAuditLogger
{
    private static readonly Guid _wholesaleSystemId = Guid.Parse("467ab87d-9494-4add-bf01-703540067b9e");

    private readonly IJsonSerializer _jsonSerializer = jsonSerializer;
    private readonly IClock _clock = clock;
    private readonly IOutboxClient _outboxClient = outboxClient;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserContext<FrontendUser> _userContext = userContext;

    public async Task LogWithCommitAsync(
        AuditLogActivity activity,
        string origin,
        object? payload,
        AuditLogEntityType? affectedEntityType,
        Guid? affectedEntityKey)
    {
        var currentUser = _userContext.CurrentUser;

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
            userId: currentUser.UserId,
            actorId: currentUser.Actor.ActorId,
            actorNumber: currentUser.Actor.ActorNumber,
            marketRoles: currentUser.Actor.MarketRole.ToString(),
            permissions: string.Join(", ", currentUser.Actor.Permissions),
            affectedEntityType: affectedEntityType?.Identifier,
            affectedEntityKey: affectedEntityKey?.ToString());

        await _outboxClient.AddToOutboxAsync(auditLogOutboxMessage)
            .ConfigureAwait(false);

        await _unitOfWork.CommitAsync()
            .ConfigureAwait(false);
    }
}
