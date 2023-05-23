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

using Energinet.DataHub.Wholesale.IntegrationEventsPublishing.Application.BatchAggregate;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Wholesale.IntegrationEventsPublishing.Infrastructure.Persistence.Batches;

public class CompletedBatchRepository : ICompletedBatchRepository
{
    private readonly IIntegrationEventPublishingDatabaseContext _context;

    public CompletedBatchRepository(IIntegrationEventPublishingDatabaseContext context)
    {
        _context = context;
    }

    public async Task AddAsync(IEnumerable<CompletedBatch> completedBatches)
    {
        await _context.CompletedBatches.AddRangeAsync(completedBatches).ConfigureAwait(false);
    }

    public async Task<CompletedBatch?> GetLastCompletedOrNullAsync()
    {
        return await _context.CompletedBatches
            .OrderByDescending(x => x.CompletedTime)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<CompletedBatch?> GetNextUnpublishedOrNullAsync()
    {
        return await _context.CompletedBatches
            .OrderBy(x => x.CompletedTime)
            .Where(x => !x.IsPublished)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }
}
