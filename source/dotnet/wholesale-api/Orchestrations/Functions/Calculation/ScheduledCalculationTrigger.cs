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
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Extensions.Options;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation;

internal class ScheduledCalculationTrigger(
    IClock clock,
    ILogger<ScheduledCalculationTrigger> logger,
    ICalculationsClient calculationsClient,
    CalculationOrchestrationMonitorOptions orchestrationMonitorOptions)
{
    private readonly IClock _clock = clock;
    private readonly ILogger<ScheduledCalculationTrigger> _logger = logger;
    private readonly ICalculationsClient _calculationsClient = calculationsClient;
    private readonly CalculationOrchestrationMonitorOptions _orchestrationMonitorOptions = orchestrationMonitorOptions;

    [Function(nameof(RunScheduledCalculations))]
    public async Task RunScheduledCalculations(
        [TimerTrigger("*/10 * * * * *")] TimerInfo timerTimerInfo, // Runs every 10 seconds
        [DurableClient] DurableTaskClient durableTaskClient,
        FunctionContext context)
    {
        var scheduledBefore = _clock.GetCurrentInstant();
        var scheduledCalculationIds = await _calculationsClient.GetScheduledCalculationsToStartAsync(scheduledBefore);

        foreach (var calculationIdToStart in scheduledCalculationIds)
        {
            var orchestrationInput = new CalculationOrchestrationInput(
                _orchestrationMonitorOptions,
                calculationIdToStart);

            var orchestrationInstanceId = await durableTaskClient.ScheduleNewOrchestrationInstanceAsync(nameof(CalculationOrchestration), orchestrationInput).ConfigureAwait(false);
            _logger.LogInformation(
                "Started new orchestration for calculation ID = {calculationId} with instance ID = {instanceId}",
                calculationIdToStart,
                orchestrationInstanceId);

            // Ensure orchestration is running by waiting for instance to start and the metadata to contain calculation id
            var calculationIsStarted = await WaitForCalculationStartedAsync(
                    durableTaskClient,
                    orchestrationInstanceId,
                    timeoutAt: _clock.GetCurrentInstant().Plus(Duration.FromMinutes(5)))
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

    private async Task<bool> WaitForCalculationStartedAsync(
        DurableTaskClient durableTaskClient,
        string orchestrationInstanceId,
        Instant timeoutAt)
    {
        while (_clock.GetCurrentInstant() < timeoutAt)
        {
            await Task.Delay(200).ConfigureAwait(false);

            var orchestrationMetadata = await durableTaskClient.GetInstanceAsync(
                    orchestrationInstanceId,
                    getInputsAndOutputs: true) // getInputsAndOutputs is required to get custom status
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(orchestrationMetadata?.SerializedCustomStatus))
                continue;

            var calculationMetadata = orchestrationMetadata.ReadCustomStatusAs<CalculationMetadata>();
            if (calculationMetadata == null)
                continue;

            if (calculationMetadata.IsStarted)
                return true;
        }

        return false;
    }
}
