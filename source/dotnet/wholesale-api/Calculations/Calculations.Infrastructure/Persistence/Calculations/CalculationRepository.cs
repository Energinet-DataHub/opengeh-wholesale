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

using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.Calculations;

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

    public async Task<Calculation> GetAsync(Guid calculationId)
    {
        return await _context.Calculations.FirstAsync(x => x.Id == calculationId).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<Calculation>> SearchAsync(
        IReadOnlyCollection<GridAreaCode> filterByGridAreaCode,
        IReadOnlyCollection<CalculationExecutionState> filterByExecutionState,
        Instant? minExecutionTimeStart,
        Instant? maxExecutionTimeStart,
        Instant? periodStart,
        Instant? periodEnd,
        CalculationType? calculationType)
    {
        var query = _context
            .Calculations
            .Where(b => minExecutionTimeStart == null || b.ExecutionTimeStart >= minExecutionTimeStart)
            .Where(b => maxExecutionTimeStart == null || b.ExecutionTimeStart <= maxExecutionTimeStart)
            .Where(b => periodEnd == null || b.PeriodStart < periodEnd)
            .Where(b => periodStart == null || b.PeriodEnd > periodStart)
            .Where(b => filterByExecutionState.Count == 0 || filterByExecutionState.Contains(b.ExecutionState))
            .Where(b => calculationType == null || b.CalculationType == calculationType);

        var foundCalculations = await query.ToListAsync().ConfigureAwait(false);

        return foundCalculations
            .Where(b => filterByGridAreaCode.Count == 0 || b.GridAreaCodes.Any(filterByGridAreaCode.Contains))
            .ToList();
    }

    public async Task<IReadOnlyCollection<Calculation>> GetScheduledCalculationsAsync(Instant scheduledBefore)
    {
        var foundCalculations = await _context
            .Calculations
            .Where(c => c.OrchestrationState == CalculationOrchestrationState.Scheduled)
            .Where(c => c.ScheduledAt <= scheduledBefore)
            .ToListAsync()
            .ConfigureAwait(false);

        return foundCalculations
            .Where(c => c.ShouldStartNow(scheduledBefore))
            .ToList();
    }
}
