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

using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.ScheduleCalculation;

public class CalculationScheduler(
    ILogger<CalculationScheduler> logger,
    IClock clock,
    ICalculationsClient calculationsClient,
    CalculationStarter calculationStarter)
{
    private readonly ILogger _logger = logger;
    private readonly IClock _clock = clock;
    private readonly ICalculationsClient _calculationsClient = calculationsClient;
    private readonly CalculationStarter _calculationStarter = calculationStarter;

    public async Task StartScheduledCalculationsAsync(DurableTaskClient durableTaskClient)
    {
        var now = _clock.GetCurrentInstant();
        var scheduledCalculationIds = await _calculationsClient
            .GetScheduledCalculationsToRunAsync(scheduledToRunBefore: now)
            .ConfigureAwait(false);

        foreach (var calculationIdToStart in scheduledCalculationIds)
        {
            var calculationIsStarted = await _calculationStarter
                .StartCalculationAsync(durableTaskClient, calculationIdToStart)
                .ConfigureAwait(false);

            if (!calculationIsStarted)
            {
                // Log error if orchestration did not start within the timeout. Does not throw exception since we
                // want to continue processing the next scheduled calculations.
                _logger.LogError(
                    "Calculation with id = {calculationId} did not start within the timeout",
                    calculationIdToStart.Id);
            }
        }
    }
}
