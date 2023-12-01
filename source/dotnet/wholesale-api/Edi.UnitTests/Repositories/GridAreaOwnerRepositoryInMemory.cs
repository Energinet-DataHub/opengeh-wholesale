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

using Energinet.DataHub.Wholesale.Batches.Application.GridArea;
using NodaTime;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Repositories;

public class GridAreaOwnerRepositoryInMemory : IGridAreaOwnerRepository
{
    private IList<GridAreaOwner> _gridAreaOwnersInMemory = new List<GridAreaOwner>();

    public Task AddAsync(string code, string ownerActorNumber, Instant validFrom, int sequenceNumber)
    {
        _gridAreaOwnersInMemory.Add(new GridAreaOwner(Guid.NewGuid(), code, ownerActorNumber, validFrom, sequenceNumber));
        return Task.CompletedTask;
    }

    public Task<GridAreaOwner> GetCurrentOwnerAsync(string code, CancellationToken cancellationToken)
    {
        var now = SystemClock.Instance.GetCurrentInstant();
        return Task.FromResult(_gridAreaOwnersInMemory.Where(gao => gao.GridAreaCode.Equals(code) && gao.ValidFrom <= now)
            .OrderByDescending(gao => gao.SequenceNumber)
            .First());
    }
}
