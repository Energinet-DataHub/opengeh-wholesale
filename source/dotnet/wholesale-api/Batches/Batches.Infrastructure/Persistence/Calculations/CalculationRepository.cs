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
    private readonly DateTimeZone _dateTimeZone;

    public CalculationRepository(IDatabaseContext context, DateTimeZone dateTimeZone)
    {
        _context = context;
        _dateTimeZone = dateTimeZone;
    }

    public async Task AddAsync(Calculation calculation)
    {
        await _context.Batches.AddAsync(calculation).ConfigureAwait(false);
    }

    public async Task<Calculation> GetAsync(Guid batchId)
    {
        return await _context.Batches.FirstAsync(x => x.Id == batchId).ConfigureAwait(false);
    }

    public Task<List<Calculation>> GetCreatedAsync() => GetByStateAsync(CalculationExecutionState.Created);

    public async Task<List<Calculation>> GetByStatesAsync(IEnumerable<CalculationExecutionState> states)
    {
        return await _context
            .Batches
            .Where(b => states.Contains(b.ExecutionState))
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Calculation>> GetCompletedAfterAsync(Instant? completedTime)
    {
        return await _context
            .Batches
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

    public async Task<IReadOnlyCollection<CalculationId>> GetNewestCalculationIdsForPeriodAsync(
        IReadOnlyCollection<GridAreaCode> filterByGridAreaCodes,
        IReadOnlyCollection<CalculationExecutionState> filterByExecutionState,
        Instant periodStart,
        Instant periodEnd)
    {
        //All batches that overlap the period
        var query = _context
            .Batches
            .Where(b => b.PeriodStart < periodEnd)
            .Where(b => b.PeriodEnd > periodStart)
            .Where(b => filterByExecutionState.Count == 0 || filterByExecutionState.Contains(b.ExecutionState))
            .Where(b => filterByGridAreaCodes.Count == 0 || b.GridAreaCodes.Any(filterByGridAreaCodes.Contains))
            .Where(b => b.CalculationId != null);

        var foundCalculations = await query.ToListAsync().ConfigureAwait(false);

        //period in days
        var periodStartInTimeZone = new ZonedDateTime(periodStart, _dateTimeZone);
        var periodEndInTimeZone = new ZonedDateTime(periodEnd, _dateTimeZone);
        var period = Period.Between(periodStartInTimeZone.LocalDateTime, periodEndInTimeZone.LocalDateTime, PeriodUnits.Days);

        var datesInPeriod = new List<Instant>();
        for (var days = 1; days <= period.Days; days++)
        {
            datesInPeriod.Add(periodStart.Plus(Duration.FromDays(days)));
        }

        var newestCalculationIdsPerDay = new List<Calculation>();
        foreach (var newestCalculation in foundCalculations.OrderByDescending(x => x.CalculationId!.Id))
        {
            if (datesInPeriod.Count == 0)
                break;

            newestCalculationIdsPerDay.Add(newestCalculation);
            datesInPeriod
                .RemoveAll(date => date >= newestCalculation.PeriodStart && date <= newestCalculation.PeriodEnd);
        }

        return newestCalculationIdsPerDay
            .Select(x => x.CalculationId!)
            .ToList();
    }

    private async Task<List<Calculation>> GetByStateAsync(CalculationExecutionState state)
    {
        return await _context
            .Batches
            .Where(b => b.ExecutionState == state)
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
