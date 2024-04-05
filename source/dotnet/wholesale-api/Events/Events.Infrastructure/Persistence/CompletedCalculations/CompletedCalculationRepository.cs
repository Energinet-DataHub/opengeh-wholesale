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

using Energinet.DataHub.Wholesale.Common.Infrastructure.FeatureFlag;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence.CompletedCalculations;

public class CompletedCalculationRepository(
    IEventsDatabaseContext context,
    IFeatureFlagManager featureFlagManager,
    IClock clock)
    : ICompletedCalculationRepository
{
    public async Task AddAsync(IEnumerable<CompletedCalculation> completedCalculations)
    {
        var calculations = completedCalculations.ToList();
        if (!await featureFlagManager.UsePublishCalculationResultsAsync().ConfigureAwait(false))
        {
            foreach (var completedCalculation in calculations)
            {
                completedCalculation.PublishedTime = clock.GetCurrentInstant();
            }
        }

        await context.CompletedCalculations.AddRangeAsync(calculations).ConfigureAwait(false);
    }

    public async Task<CompletedCalculation?> GetLastCompletedOrNullAsync()
    {
        return await context.CompletedCalculations
            .OrderByDescending(x => x.CompletedTime)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<CompletedCalculation?> GetNextUnpublishedOrNullAsync()
    {
        return await context.CompletedCalculations
            .OrderBy(x => x.CompletedTime)
            .Where(x => x.PublishedTime == null)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }
}
