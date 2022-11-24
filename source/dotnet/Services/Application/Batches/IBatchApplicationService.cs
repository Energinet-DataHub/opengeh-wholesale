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

using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;

namespace Energinet.DataHub.Wholesale.Application.Batches;

public interface IBatchApplicationService
{
    /// <summary>
    /// Create a new batch with state <see cref="BatchExecutionState.Created"/>.
    /// </summary>
    Task<Guid> CreateAsync(BatchRequestDto batchRequestDto);

    /// <summary>
    /// Create and start all processes of batches with state <see cref="BatchExecutionState.Submitted"/>.
    /// </summary>
    Task StartSubmittingAsync();

    Task UpdateExecutionStateAsync();

    Task<IEnumerable<BatchDto>> SearchAsync(BatchSearchDto batchSearchDto);

    Task<BatchDto> GetAsync(Guid batchId);
}
