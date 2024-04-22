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

using AutoFixture;
using Energinet.DataHub.Core.TestCommon;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.DurableTask;

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
}
