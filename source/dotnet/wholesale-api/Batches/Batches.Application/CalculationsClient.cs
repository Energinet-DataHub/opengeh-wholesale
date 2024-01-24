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

using Energinet.DataHub.Wholesale.Batches.Application.Model;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using NodaTime;
using CalculationState = Energinet.DataHub.Wholesale.Batches.Interfaces.Models.CalculationState;

namespace Energinet.DataHub.Wholesale.Batches.Application;

public class CalculationsClient : ICalculationsClient
{
    private readonly ICalculationRepository _calculationRepository;
    private readonly ICalculationDtoMapper _calculationDtoMapper;

    public CalculationsClient(ICalculationRepository calculationRepository, ICalculationDtoMapper calculationDtoMapper)
    {
        _calculationRepository = calculationRepository;
        _calculationDtoMapper = calculationDtoMapper;
    }

    public async Task<IEnumerable<CalculationDto>> GetBatchesCompletedAfterAsync(Instant? completedTime)
    {
        var batches = await _calculationRepository.GetCompletedAfterAsync(completedTime).ConfigureAwait(false);
        return batches.Select(_calculationDtoMapper.Map);
    }

    public async Task<CalculationDto> GetAsync(Guid batchId)
    {
        var batch = await _calculationRepository.GetAsync(batchId).ConfigureAwait(false);
        return _calculationDtoMapper.Map(batch);
    }

    public async Task<IEnumerable<CalculationDto>> SearchAsync(
        IEnumerable<string> filterByGridAreaCodes,
        CalculationState? filterByExecutionState,
        DateTimeOffset? minExecutionTime,
        DateTimeOffset? maxExecutionTime,
        DateTimeOffset? periodStart,
        DateTimeOffset? periodEnd)
    {
        var executionStateFilter = GetCalculationExecutionStates(filterByExecutionState);

        var gridAreaFilter = filterByGridAreaCodes
            .Select(g => new GridAreaCode(g))
            .ToList();

        var minExecutionTimeStart = ConvertToInstant(minExecutionTime);
        var maxExecutionTimeStart = ConvertToInstant(maxExecutionTime);
        var periodStartInstant = ConvertToInstant(periodStart);
        var periodEndInstant = ConvertToInstant(periodEnd);

        var batches = await _calculationRepository
            .SearchAsync(
                gridAreaFilter,
                executionStateFilter,
                minExecutionTimeStart,
                maxExecutionTimeStart,
                periodStartInstant,
                periodEndInstant)
            .ConfigureAwait(false);

        return batches.Select(_calculationDtoMapper.Map);
    }

    public async Task<IEnumerable<long>> GetNewestCalculationIdsForPeriodAsync(
        IEnumerable<string> filterByGridAreaCodes,
        CalculationState filterByExecutionState,
        Instant periodStart,
        Instant periodEnd)
    {
        var executionStateFilter = GetCalculationExecutionStates(filterByExecutionState);

        var gridAreaFilter = filterByGridAreaCodes
            .Select(g => new GridAreaCode(g))
            .ToList();

        var newestCalculationIdsForPeriod = await _calculationRepository.GetNewestCalculationIdsForPeriodAsync(
                filterByGridAreaCodes: gridAreaFilter,
                filterByExecutionState: executionStateFilter,
                periodStart: periodStart,
                periodEnd: periodEnd)
            .ConfigureAwait(false);

        return newestCalculationIdsForPeriod.Select(x => x.Id);
    }

    private static CalculationExecutionState[] GetCalculationExecutionStates(CalculationState? filterByExecutionState)
    {
        var executionStateFilter = filterByExecutionState switch
        {
            null => Array.Empty<CalculationExecutionState>(),
            CalculationState.Pending => new[]
            {
                CalculationExecutionState.Created, CalculationExecutionState.Submitted, CalculationExecutionState.Pending,
            },
            CalculationState.Executing => new[] { CalculationExecutionState.Executing },
            CalculationState.Completed => new[] { CalculationExecutionState.Completed },
            CalculationState.Failed => new[] { CalculationExecutionState.Failed },
            _ => throw new ArgumentOutOfRangeException(nameof(filterByExecutionState)),
        };
        return executionStateFilter;
    }

    private static Instant? ConvertToInstant(DateTimeOffset? dateTimeOffset)
    {
        return dateTimeOffset == null
            ? null
            : Instant.FromDateTimeOffset(dateTimeOffset.Value);
    }
}
