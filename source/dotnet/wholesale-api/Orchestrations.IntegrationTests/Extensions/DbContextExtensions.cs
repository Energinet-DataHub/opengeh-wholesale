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

using Energinet.DataHub.Core.Outbox.Domain;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions;

public static class DbContextExtensions
{
    public static async Task<(bool Success, CalculationOrchestrationState ActualState)> WaitForCalculationWithOneOfStatesAsync(
        this DatabaseContext dbContext,
        Guid id,
        CalculationOrchestrationState[] states,
        ITestDiagnosticsLogger logger,
        TimeSpan? timeLimit = null,
        CalculationOrchestrationState[]? disallowedStates = null)
    {
        var waitForStates = string.Join(", ", states);
        var wasInDisallowedState = false;
        var success = await Awaiter.TryWaitUntilConditionAsync(
            async () =>
            {
                var calculation = await dbContext.Calculations
                    .AsNoTracking() // .AsNoTracking() is important, else the result is cached
                    .SingleAsync(c => c.Id == id);

                // If the calculation is in a disallowed state
                if (disallowedStates != null && disallowedStates.Contains(calculation.OrchestrationState))
                    wasInDisallowedState = true;

                logger.WriteLine($"Waiting for one of calculation states: [{waitForStates}], current state is: {calculation.OrchestrationState}, calculation id: {id}");
                return states.Contains(calculation.OrchestrationState);
            },
            timeLimit: timeLimit ?? TimeSpan.FromSeconds(30),
            delay: TimeSpan.FromSeconds(1));

        var calculation = await dbContext.Calculations
            .AsNoTracking()
            .SingleAsync(c => c.Id == id);

        if (!success)
            logger.WriteLine($"Timeout while waiting for one of calculation orchestration states: [{waitForStates}], current orchestration state is: {calculation.OrchestrationState} (execution state: {calculation.ExecutionState})");

        return (!wasInDisallowedState && success, calculation.OrchestrationState);
    }

    public static Task<(bool Success, CalculationOrchestrationState ActualState)> WaitForCalculationWithStateAsync(
        this DatabaseContext dbContext,
        Guid id,
        CalculationOrchestrationState state,
        ITestDiagnosticsLogger logger,
        TimeSpan? timeLimit = null,
        CalculationOrchestrationState[]? disallowedStates = null) =>
        WaitForCalculationWithOneOfStatesAsync(
            dbContext,
            id,
            [state],
            logger,
            timeLimit,
            disallowedStates);

    public static async Task<(bool Success, OutboxMessage? OutboxMessage)> WaitForPublishedOutboxMessageAsync(
        this DatabaseContext dbContext,
        Instant createdAfter,
        ITestDiagnosticsLogger logger,
        TimeSpan? timeLimit = null)
    {
        OutboxMessage? outboxMessage = null;
        var success = await Awaiter.TryWaitUntilConditionAsync(
            async () =>
            {
                outboxMessage = await dbContext.Outbox
                    .AsNoTracking() // .AsNoTracking() is important, else the result is cached
                    .SingleAsync(c => c.CreatedAt > createdAfter);

                logger.WriteLine($"Waiting for outbox message to be published, current state is: " +
                                 $"Id={outboxMessage.Id.Id}, " +
                                 $"PublishedAt={outboxMessage.PublishedAt}, " +
                                 $"ProcessingAt={outboxMessage.ProcessingAt}, " +
                                 $"FailedAt={outboxMessage.FailedAt}, " +
                                 $"ErrorMessage={outboxMessage.ErrorMessage}");

                return outboxMessage.PublishedAt != null;
            },
            timeLimit: timeLimit ?? TimeSpan.FromSeconds(30),
            delay: TimeSpan.FromSeconds(1));

        if (!success)
        {
            logger.WriteLine($"Timeout waiting for outbox message to be published, current state is: " +
                             $"Id={outboxMessage?.Id.Id}, " +
                             $"PublishedAt={outboxMessage?.PublishedAt}, " +
                             $"ProcessingAt={outboxMessage?.ProcessingAt}, " +
                             $"FailedAt={outboxMessage?.FailedAt}, " +
                             $"ErrorMessage={outboxMessage?.ErrorMessage}");
        }

        return (success, outboxMessage);
    }
}
