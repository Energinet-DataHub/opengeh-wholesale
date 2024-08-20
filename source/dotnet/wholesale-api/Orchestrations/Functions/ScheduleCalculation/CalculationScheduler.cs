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

using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Orchestrations.Extensions.Options;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.ScheduleCalculation;

public class CalculationScheduler(
    ILogger<CalculationScheduler> logger,
    IOptions<CalculationOrchestrationMonitorOptions> orchestrationMonitorOptions,
    IClock clock,
    ICalculationRepository calculationRepository)
{
    private readonly ILogger _logger = logger;
    private readonly CalculationOrchestrationMonitorOptions _orchestrationMonitorOptions = orchestrationMonitorOptions.Value;
    private readonly IClock _clock = clock;
    private readonly ICalculationRepository _calculationRepository = calculationRepository;

    public async Task StartScheduledCalculationsAsync(DurableTaskClient durableTaskClient)
    {
        var now = _clock.GetCurrentInstant();
        var scheduledCalculationIds = await _calculationRepository
            .GetScheduledCalculationsAsync(scheduledToRunBefore: now)
            .ConfigureAwait(false);

        var calculationStarter = new CalculationStarter(
            _logger,
            _orchestrationMonitorOptions,
            durableTaskClient);

        foreach (var calculationToStart in scheduledCalculationIds)
        {
            try
            {
                await calculationStarter
                    .StartCalculationAsync(calculationToStart)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // Log error if orchestration did not start successfully.
                // Does not throw exception since we want to continue processing the next scheduled calculations.
                _logger.LogError(
                    e,
                    "Failed to start calculation with id = {CalculationId} and orchestration instance id = {OrchestrationInstanceId}",
                    calculationToStart.CalculationId.Id,
                    calculationToStart.OrchestrationInstanceId.Id);
            }
        }
    }
}
