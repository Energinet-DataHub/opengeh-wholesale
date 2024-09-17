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
using Energinet.DataHub.Wholesale.Calculations.Interfaces.AuditLog;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Security;

namespace Energinet.DataHub.Wholesale.Orchestrations;

public class AuditUserContext(IUserContext<FrontendUser> userContext)
    : IAuditUserContext
{
    private readonly IUserContext<FrontendUser> _userContext = userContext;

    public AuditLogUser CurrentUser
    {
        get
        {
            var userContext = _userContext.CurrentUser;

            return new AuditLogUser(
                userContext.UserId,
                userContext.Actor.ActorId,
                userContext.Actor.ActorNumber,
                userContext.Actor.MarketRole.ToString(),
                string.Join(", ", userContext.Actor.Permissions));
        }
    }
}
