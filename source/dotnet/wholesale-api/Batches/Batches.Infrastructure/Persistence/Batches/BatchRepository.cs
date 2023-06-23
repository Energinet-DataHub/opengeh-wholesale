﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Wholesale.Batches.Application;
using Energinet.DataHub.Wholesale.Batches.Application.Model;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Batches;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence.Batches;

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

    public async Task<List<Batch>> GetByStatesAsync(IEnumerable<BatchExecutionState> states)
    {
        return await _context
            .Batches
            .Where(b => states.Contains(b.ExecutionState))
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Batch>> GetCompletedAfterAsync(Instant? completedTime)
    {
        return await _context
            .Batches
            .Where(b => b.ExecutionState == BatchExecutionState.Completed)
            .Where(b => completedTime == null || b.ExecutionTimeEnd > completedTime)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<Batch>> SearchAsync(
        IReadOnlyCollection<GridAreaCode> filterByGridAreaCode,
        IReadOnlyCollection<BatchExecutionState> filterByExecutionState,
        Instant? minExecutionTimeStart,
        Instant? maxExecutionTimeStart,
        Instant? periodStart,
        Instant? periodEnd)
    {
        var query = _context
            .Batches
            .Where(b => minExecutionTimeStart == null || b.ExecutionTimeStart >= minExecutionTimeStart)
            .Where(b => maxExecutionTimeStart == null || b.ExecutionTimeStart <= maxExecutionTimeStart)
            .Where(b => periodEnd == null || b.PeriodStart < periodEnd)
            .Where(b => periodStart == null || b.PeriodEnd > periodStart)
            .Where(b => filterByExecutionState.Count == 0 || filterByExecutionState.Contains(b.ExecutionState));

        var foundBatches = await query.ToListAsync().ConfigureAwait(false);

        return foundBatches
            .Where(b => filterByGridAreaCode.Count == 0 || b.GridAreaCodes.Any(filterByGridAreaCode.Contains))
            .ToList();
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
