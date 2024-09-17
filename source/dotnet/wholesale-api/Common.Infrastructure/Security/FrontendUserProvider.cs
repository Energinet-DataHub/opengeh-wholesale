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

using System.Security.Claims;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;

namespace Energinet.DataHub.Wholesale.Common.Infrastructure.Security;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class FrontendUserProvider : IUserProvider<FrontendUser>
{
    private const string ActorNumberClaim = "actornumber";
    private const string MarketRolesClaim = "marketroles";

    public Task<FrontendUser?> ProvideUserAsync(
        Guid userId,
        Guid actorId,
        bool multiTenancy,
        IEnumerable<Claim> claims)
    {
        var enumeratedClaims = claims.ToList();
        var frontendActor = new FrontendActor(
            actorId,
            GetActorNumber(enumeratedClaims),
            GetMarketRole(enumeratedClaims),
            GetPermissions(enumeratedClaims));
        var frontendUser = new FrontendUser(userId, multiTenancy, frontendActor);

        return Task.FromResult<FrontendUser?>(frontendUser);
    }

    private IReadOnlyCollection<string> GetPermissions(List<Claim> enumeratedClaims)
    {
        return enumeratedClaims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToArray();
    }

    private static string GetActorNumber(IEnumerable<Claim> claims)
    {
        return claims.Single(claim => claim.Type == ActorNumberClaim).Value;
    }

    private static FrontendActorMarketRole GetMarketRole(IEnumerable<Claim> claims)
    {
        return claims.Single(claim => claim.Type == MarketRolesClaim).Value switch
        {
            "GridAccessProvider" => FrontendActorMarketRole.GridAccessProvider,
            "EnergySupplier" => FrontendActorMarketRole.EnergySupplier,
            "SystemOperator" => FrontendActorMarketRole.SystemOperator,
            "DataHubAdministrator" => FrontendActorMarketRole.DataHubAdministrator,
            _ => FrontendActorMarketRole.Other,
        };
    }
}
