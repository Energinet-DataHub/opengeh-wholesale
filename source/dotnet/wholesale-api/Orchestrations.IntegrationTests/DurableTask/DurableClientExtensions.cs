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

using Energinet.DataHub.Core.TestCommon;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.DurableTask;

// TODO:
// We should move this class to TestCommon, maybe to a new project named DurableFunctionApp.TestCommon.
// It should recide in the namespace "DurableTask" like it does here.
public static class DurableClientExtensions
{
    /// <summary>
    /// Search for an orchestration that is either running or completed,
    /// and which was started at, or later, than given <paramref name="createdTimeFrom"/>.
    ///
    /// If more than one orchestration exists an exception is thrown.
    /// </summary>
    public static async Task<DurableOrchestrationStatus> FindOrchestationStatusAsync(
        this IDurableClient client,
        DateTime createdTimeFrom)
    {
        var filter = new OrchestrationStatusQueryCondition()
        {
            CreatedTimeFrom = createdTimeFrom,
            RuntimeStatus =
            [
                OrchestrationRuntimeStatus.Running,
                OrchestrationRuntimeStatus.Completed,
            ],
        };
        var queryResult = await client.ListInstancesAsync(filter, CancellationToken.None);

        return queryResult.DurableOrchestrationState.Single();
    }

    /// <summary>
    /// Wait for orchestration instance to be completed within given <paramref name="waitTimeLimit"/>.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="instanceId"></param>
    /// <param name="waitTimeLimit">Max time to wait for completion. If not specified it defaults to 30 seconds.</param>
    /// <returns>If completed within given <paramref name="waitTimeLimit"/> it returns the orchestration status including history; otherwise it throws an exception.</returns>
    public static async Task<DurableOrchestrationStatus> WaitForInstanceCompletedAsync(
        this IDurableClient client,
        string instanceId,
        TimeSpan? waitTimeLimit = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);

        var isCompleted = await Awaiter.TryWaitUntilConditionAsync(
            async () =>
            {
                // Do not retrieve history here as it could be expensive
                var completeOrchestrationStatus = await client.GetStatusAsync(instanceId);
                return completeOrchestrationStatus.RuntimeStatus == OrchestrationRuntimeStatus.Completed;
            },
            waitTimeLimit ?? TimeSpan.FromSeconds(30),
            delay: TimeSpan.FromSeconds(5));

        return isCompleted
            ? await client.GetStatusAsync(instanceId, showHistory: true, showHistoryOutput: true)
            : throw new Exception($"Orchestration instance '{instanceId}' did not complete within configured wait time limit.");
    }

    /// <summary>
    /// Wait for orchestration instance to have a custom status where the given <paramref name="matchFunction"/> returns true
    /// </summary>
    /// <exception cref="InvalidCastException">Throws InvalidCastException if CustomStatus property cannot be parsed to given type</exception>
    public static async Task<DurableOrchestrationStatus> WaitForCustomStatusAsync<TCustomStatus>(
        this IDurableClient client,
        string instanceId,
        Func<TCustomStatus, bool> matchFunction,
        TimeSpan? waitTimeLimit = null)
    {
        var matchesCustomStatus = await Awaiter.TryWaitUntilConditionAsync(
            async () =>
            {
                // Do not retrieve history here as it could be expensive
                var orchestrationStatus = await client.GetStatusAsync(instanceId);

                var customStatus = orchestrationStatus.CustomStatus.ToObject<TCustomStatus>()
                    ?? throw new InvalidCastException($"Cannot cast CustomStatus to {typeof(TCustomStatus).Name}");

                var isMatch = matchFunction(customStatus);

                return isMatch;
            },
            waitTimeLimit ?? TimeSpan.FromSeconds(30),
            delay: TimeSpan.FromSeconds(5));

        return matchesCustomStatus
            ? await client.GetStatusAsync(instanceId, showHistory: true, showHistoryOutput: true)
            : throw new Exception($"Orchestration instance '{instanceId}' did not match custom status- within configured wait time limit.");
    }
}
