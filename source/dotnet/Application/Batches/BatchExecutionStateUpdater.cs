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
// limitations under the License.using Energinet.DataHub.Wholesale.Application.JobRunner;

using Energinet.DataHub.Wholesale.Application.JobRunner;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;

namespace Energinet.DataHub.Wholesale.Application.Batches;

public class BatchExecutionStateUpdater
{
    private IEnumerable<Batch> _completedBatches = new List<Batch>();

    public async Task UpdateExecutionStatesAsync(IBatchRepository batchRepository, ICalculatorJobRunner calculatorJobRunner)
    {
        var pendingBatches = await batchRepository.GetPendingAsync().ConfigureAwait(false);
        var executingBatches = await batchRepository.GetExecutingAsync().ConfigureAwait(false);

        await UpdateFromPendingAsync(pendingBatches, calculatorJobRunner).ConfigureAwait(false);
        _completedBatches = await UpdateFromExecutingAsync(executingBatches, calculatorJobRunner).ConfigureAwait(false);
    }

    public IEnumerable<Batch> GetCompletedBatches()
    {
        return _completedBatches;
    }

    private static async Task<IEnumerable<Batch>> UpdateFromExecutingAsync(IEnumerable<Batch> executingBatches, ICalculatorJobRunner calculatorJobRunner)
    {
        if (!executingBatches.Any())
            return new List<Batch>();

        var completedBatches = new List<Batch>();

        foreach (var batch in executingBatches)
        {
            // The batch will have received a RunId when the batch have started.
            var runId = batch.RunId!;

            var state = await calculatorJobRunner
                .GetJobStateAsync(runId)
                .ConfigureAwait(false);

            switch (state)
            {
                case JobState.Completed:
                    batch.MarkAsCompleted();
                    completedBatches.Add(batch);
                    break;
                case JobState.Failed:
                    batch.MarkAsFailed();
                    break;
            }
        }

        return completedBatches;
    }

    private static async Task UpdateFromPendingAsync(IEnumerable<Batch> pendingBatches, ICalculatorJobRunner calculatorJobRunner)
    {
        foreach (var batch in pendingBatches)
        {
            // The batch will have received a RunId when the batch have started.
            var runId = batch.RunId!;

            var state = await calculatorJobRunner
                .GetJobStateAsync(runId)
                .ConfigureAwait(false);

            switch (state)
            {
                case JobState.Running:
                    batch.MarkAsExecuting();
                    break;
                case JobState.Failed:
                    batch.MarkAsFailed();
                    break;
            }
        }
    }
}
