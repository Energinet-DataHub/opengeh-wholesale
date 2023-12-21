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

using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence.CompletedCalculations;

public class CompletedCalculationRepository : ICompletedCalculationRepository
{
    private readonly IEventsDatabaseContext _context;

    public CompletedCalculationRepository(IEventsDatabaseContext context)
    {
        _context = context;
    }

    public async Task AddAsync(IEnumerable<CompletedCalculation> completedBatches)
    {
        await _context.CompletedBatches.AddRangeAsync(completedBatches).ConfigureAwait(false);
    }

    public async Task<CompletedCalculation?> GetLastCompletedOrNullAsync()
    {
        return await _context.CompletedBatches
            .OrderByDescending(x => x.CompletedTime)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<CompletedCalculation?> GetNextUnpublishedOrNullAsync()
    {
        return await _context.CompletedBatches
            .OrderBy(x => x.CompletedTime)
            .Where(x => x.PublishedTime == null)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }
}
