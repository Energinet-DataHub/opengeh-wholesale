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

using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Orchestrations.Extensions.Options;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.ScheduleCalculation;

public class CalculationStarter(
    ILogger logger,
    CalculationOrchestrationMonitorOptions orchestrationMonitorOptions,
    DurableTaskClient durableTaskClient)
{
    private readonly ILogger _logger = logger;
    private readonly CalculationOrchestrationMonitorOptions _orchestrationMonitorOptions = orchestrationMonitorOptions;
    private readonly DurableTaskClient _durableTaskClient = durableTaskClient;

    /// <summary>
    /// Start a calculation orchestration for the given calculation ID. Starting an orchestration is an asynchronous,
    /// so the method will wait for the orchestration to start before returning, within the given timeout.
    /// </summary>
    /// <param name="calculationToStart">The calculation to start.</param>
    /// <returns>Returns true if the calculation was started, false if it failed to start within the given timeout</returns>
    public async Task StartCalculationAsync(ScheduledCalculation calculationToStart)
    {
        var orchestrationInput = new CalculationOrchestrationInput(
            _orchestrationMonitorOptions,
            calculationToStart.CalculationId);

        var alreadyStarted = await OrchestrationIsAlreadyStartedAsync(calculationToStart.OrchestrationInstanceId)
            .ConfigureAwait(false);

        if (alreadyStarted)
        {
            throw new InvalidOperationException($"Cannot start already existing calculation orchestration " +
                                                $"(calculation id = {calculationToStart.CalculationId.Id}, " +
                                                $"orchestration instance id = {calculationToStart.OrchestrationInstanceId.Id})");
        }

        var orchestrationInstanceId = await _durableTaskClient
            .ScheduleNewOrchestrationInstanceAsync(
                nameof(CalculationOrchestration),
                orchestrationInput,
                new StartOrchestrationOptions(calculationToStart.OrchestrationInstanceId.Id))
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Started new orchestration for calculation id = {calculationId} with instance id = {instanceId}",
            calculationToStart.CalculationId.Id,
            orchestrationInstanceId);
    }

    private async Task<bool> OrchestrationIsAlreadyStartedAsync(OrchestrationInstanceId orchestrationInstanceId)
    {
        var existingInstance = await _durableTaskClient.GetInstanceAsync(orchestrationInstanceId.Id)
            .ConfigureAwait(false);

        return existingInstance != null;
    }
}
