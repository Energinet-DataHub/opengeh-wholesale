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

using Energinet.DataHub.Wholesale.Calculations.Interfaces.GridArea;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.GridArea;

public class GridAreaOwnerRepository : IGridAreaOwnerRepository
{
    private readonly IDatabaseContext _context;

    public GridAreaOwnerRepository(IDatabaseContext context)
    {
        _context = context;
    }

    public void Add(string code, string ownerActorNumber, Instant validFrom, int sequenceNumber)
    {
        _context.GridAreaOwners.Add(new GridAreaOwner(
            Guid.NewGuid(),
            code,
            ownerActorNumber,
            validFrom,
            sequenceNumber));
    }

    public async Task<IEnumerable<string>> GetOwnedByAsync(string actorNumber)
    {
        var now = SystemClock.Instance.GetCurrentInstant();

        var latestGridAreaOwner =
            from gridAreaOwner in _context.GridAreaOwners
            where gridAreaOwner.ValidFrom <= now
            group gridAreaOwner by gridAreaOwner.GridAreaCode into gridAreaGroup
            select new
            {
                GridAreaCode = gridAreaGroup.Key,
                SequenceNumber = gridAreaGroup.Max(gao => gao.SequenceNumber),
            };

        var ownedBy =
            from gridAreaOwner in _context.GridAreaOwners
            join latestGao in latestGridAreaOwner on new { gridAreaOwner.GridAreaCode, gridAreaOwner.SequenceNumber } equals new { latestGao.GridAreaCode, latestGao.SequenceNumber }
            where gridAreaOwner.OwnerActorNumber == actorNumber
            select gridAreaOwner.GridAreaCode;

        return await ownedBy
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public Task<GridAreaOwner?> GetCurrentOwnerAsync(string code, CancellationToken cancellationToken)
    {
        var now = SystemClock.Instance.GetCurrentInstant();
        return _context.GridAreaOwners
            .Where(gao =>
                gao.GridAreaCode.Equals(code)
                && gao.ValidFrom <= now)
            .OrderByDescending(gao => gao.SequenceNumber)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
