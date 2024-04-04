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

using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.Options;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence.CompletedCalculations;

public class CompletedCalculationRepository : ICompletedCalculationRepository
{
    private readonly IEventsDatabaseContext _context;
    private readonly IClock _clock;
    private readonly IntegrationEventsOptions _options;

    public CompletedCalculationRepository(
        IEventsDatabaseContext context,
        IOptions<IntegrationEventsOptions> options,
        IClock clock)
    {
        _context = context;
        _clock = clock;
        _options = options.Value;
    }

    public async Task AddAsync(IEnumerable<CompletedCalculation> completedCalculations)
    {
        var calculations = completedCalculations.ToList();
        if (_options.DoNotPublishCalculationResults)
        {
            foreach (var completedCalculation in calculations)
            {
                completedCalculation.PublishedTime = _clock.GetCurrentInstant();
            }
        }

        await _context.CompletedCalculations.AddRangeAsync(calculations).ConfigureAwait(false);
    }

    public async Task<CompletedCalculation?> GetLastCompletedOrNullAsync()
    {
        return await _context.CompletedCalculations
            .OrderByDescending(x => x.CompletedTime)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<CompletedCalculation?> GetNextUnpublishedOrNullAsync()
    {
        return await _context.CompletedCalculations
            .OrderBy(x => x.CompletedTime)
            .Where(x => x.PublishedTime == null)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }
}
