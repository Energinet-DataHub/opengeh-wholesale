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
using NodaTime;

namespace Energinet.DataHub.Wholesale.Batches.Interfaces;

public interface ICalculationsClient
{
    Task<IEnumerable<CalculationDto>> GetBatchesCompletedAfterAsync(Instant? completedTime);

    Task<CalculationDto> GetAsync(Guid batchId);

    Task<IEnumerable<CalculationDto>> SearchAsync(
        IEnumerable<string> filterByGridAreaCodes,
        CalculationState? filterByExecutionState,
        DateTimeOffset? minExecutionTime,
        DateTimeOffset? maxExecutionTime,
        DateTimeOffset? periodStart,
        DateTimeOffset? periodEnd);

    Task<IEnumerable<long>> GetNewestCalculationIdsForPeriodAsync(
        IEnumerable<string> filterByGridAreaCodes,
        CalculationState filterByExecutionState,
        Instant periodStart,
        Instant periodEnd);
}
