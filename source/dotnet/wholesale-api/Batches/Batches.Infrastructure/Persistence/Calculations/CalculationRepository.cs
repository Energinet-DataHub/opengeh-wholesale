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

using Energinet.DataHub.Wholesale.Batches.Application;
using Energinet.DataHub.Wholesale.Batches.Application.Model;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Calculations;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence.Calculations;

public class CalculationRepository : ICalculationRepository
{
    private readonly IDatabaseContext _context;

    public CalculationRepository(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Calculation calculation)
    {
        await _context.Calculations.AddAsync(calculation).ConfigureAwait(false);
    }

    public async Task<Calculation> GetAsync(Guid batchId)
    {
        return await _context.Calculations.FirstAsync(x => x.Id == batchId).ConfigureAwait(false);
    }

    public Task<List<Calculation>> GetCreatedAsync() => GetByStateAsync(CalculationExecutionState.Created);

    public async Task<List<Calculation>> GetByStatesAsync(IEnumerable<CalculationExecutionState> states)
    {
        return await _context
            .Calculations
            .Where(b => states.Contains(b.ExecutionState))
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Calculation>> GetCompletedAfterAsync(Instant? completedTime)
    {
        return await _context
            .Calculations
            .Where(b => b.ExecutionState == CalculationExecutionState.Completed)
            .Where(b => completedTime == null || b.ExecutionTimeEnd > completedTime)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<Calculation>> SearchAsync(
        IReadOnlyCollection<GridAreaCode> filterByGridAreaCode,
        IReadOnlyCollection<CalculationExecutionState> filterByExecutionState,
        Instant? minExecutionTimeStart,
        Instant? maxExecutionTimeStart,
        Instant? periodStart,
        Instant? periodEnd)
    {
        var query = _context
            .Calculations
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

    private async Task<List<Calculation>> GetByStateAsync(CalculationExecutionState state)
    {
        return await _context
            .Calculations
            .Where(b => b.ExecutionState == state)
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
