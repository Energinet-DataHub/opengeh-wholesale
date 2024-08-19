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
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.ScheduleCalculation;

public class CalculationStarter(
    ILogger logger,
    IClock clock,
    IOptions<CalculationOrchestrationMonitorOptions> orchestrationMonitorOptions)
{
    private readonly ILogger _logger = logger;
    private readonly IClock _clock = clock;
    private readonly CalculationOrchestrationMonitorOptions _orchestrationMonitorOptions = orchestrationMonitorOptions.Value;

    /// <summary>
    /// Start a calculation orchestration for the given calculation ID. Starting an orchestration is an asynchronous,
    /// so the method will wait for the orchestration to start before returning, within the given timeout.
    /// </summary>
    /// <param name="durableTaskClient">The durable task client used to communicate with orchestrator.</param>
    /// <param name="calculationIdToStart">The calculation id to start.</param>
    /// <param name="timeoutAfter">Optional amount of time to wait for the orchestration to start. Will time out after a default value if not provided.</param>
    /// <returns>Returns true if the calculation was started, false if it failed to start within the given timeout</returns>
    public async Task<bool> StartCalculationAsync(
        DurableTaskClient durableTaskClient,
        CalculationId calculationIdToStart,
        Instant? timeoutAfter = null)
    {
        var orchestrationInput = new CalculationOrchestrationInput(
            _orchestrationMonitorOptions,
            calculationIdToStart);

        var orchestrationInstanceId = await durableTaskClient.ScheduleNewOrchestrationInstanceAsync(nameof(CalculationOrchestration), orchestrationInput).ConfigureAwait(false);
        _logger.LogInformation(
            "Started new orchestration for calculation ID = {calculationId} with instance ID = {instanceId}",
            calculationIdToStart,
            orchestrationInstanceId);

        // Ensure calculation is started succesfully by waiting for orchestration instance to start
        var calculationIsStarted = await WaitForCalculationOrchestrationStartedAsync(
                durableTaskClient,
                orchestrationInstanceId,
                timeoutAt: timeoutAfter ?? _clock.GetCurrentInstant().Plus(Duration.FromMinutes(5)))
            .ConfigureAwait(false);

        return calculationIsStarted;
    }

    private async Task<bool> WaitForCalculationOrchestrationStartedAsync(
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
