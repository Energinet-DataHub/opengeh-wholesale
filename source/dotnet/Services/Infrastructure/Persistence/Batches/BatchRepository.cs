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

using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Infrastructure.Persistence.Batches;

public class BatchRepository : IBatchRepository
{
    private readonly IDatabaseContext _context;

    public BatchRepository(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Batch batch)
    {
        await _context.Batches.AddAsync(batch).ConfigureAwait(false);
    }

    public async Task<Batch> GetAsync(Guid batchId)
    {
        return await _context.Batches.FirstAsync(x => x.Id == batchId).ConfigureAwait(false);
    }

    public Task<List<Batch>> GetCreatedAsync() => GetByStateAsync(BatchExecutionState.Created);

    public Task<List<Batch>> GetPendingAsync() => GetByStateAsync(BatchExecutionState.Pending);

    public Task<List<Batch>> GetExecutingAsync() => GetByStateAsync(BatchExecutionState.Executing);

    public async Task<List<Batch>> GetByStatesAsync(IEnumerable<BatchExecutionState> states)
    {
        return await _context
            .Batches
            .Where(b => states.Contains(b.ExecutionState))
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public Task<List<Batch>> GetCompletedAsync() => GetByStateAsync(BatchExecutionState.Completed);

    public async Task<List<Batch>> GetAsync(Instant minExecutionTimeStart, Instant maxExecutionTimeStart)
    {
        return await _context
            .Batches
            .Where(b => b.ExecutionTimeStart >= minExecutionTimeStart && b.ExecutionTimeStart <= maxExecutionTimeStart)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task UpdateBatchHasBeenZippedToTrueAsync(Guid batchId)
    {
        var batch = _context.Batches.Single(b => b.Id == batchId);
        batch.BatchHasBeenZipped = true;
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    private async Task<List<Batch>> GetByStateAsync(BatchExecutionState state)
    {
        return await _context
            .Batches
            .Where(b => b.ExecutionState == state)
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
