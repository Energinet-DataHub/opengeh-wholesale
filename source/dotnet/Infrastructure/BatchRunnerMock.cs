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

using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;

namespace Energinet.DataHub.Wholesale.Infrastructure;

public class BatchRunnerMock : IBatchRunner
{
    public Task BeginExecuteAsync(List<Batch> requestedBatches)
    {
        // Future work will initiate the actual execution of the batch processes in Databricks here
        return Task.CompletedTask;
    }

    public Task<List<Batch>> GetCompletedAsync(List<Batch> candidateBatches)
    {
        if (candidateBatches.Any(b => b.ExecutionState != BatchExecutionState.Executing))
            throw new ArgumentException("Only batches with status executing should be checked for completeness.");

        // Currently we do not execute batches but solely emulates that the first (randomly selected) batch has completed
        return Task.FromResult(new List<Batch> { candidateBatches.First() });
    }
}
