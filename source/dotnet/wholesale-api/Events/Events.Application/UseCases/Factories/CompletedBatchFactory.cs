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

using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.Events.Application.UseCases.Factories;

public class CompletedBatchFactory : ICompletedBatchFactory
{
    public IEnumerable<CompletedBatch> CreateFromBatches(IEnumerable<BatchDto> completedBatchDtos)
    {
        return completedBatchDtos.Select(CreateFromBatch);
    }

    public CompletedBatch CreateFromBatch(BatchDto completedBatchDto)
    {
        if (completedBatchDto.ExecutionTimeEnd == null)
            throw new ArgumentNullException($"{nameof(BatchDto.ExecutionTimeEnd)} should not be null for a completed batch.");

        return new CompletedBatch(
            completedBatchDto.BatchId,
            completedBatchDto.GridAreaCodes.ToList(),
            completedBatchDto.ProcessType,
            completedBatchDto.PeriodStart.ToInstant(),
            completedBatchDto.PeriodEnd.ToInstant(),
            completedTime: completedBatchDto.ExecutionTimeEnd.Value.ToInstant());
    }
}
