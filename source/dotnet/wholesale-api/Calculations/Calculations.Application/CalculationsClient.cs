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

using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;
using CalculationState = Energinet.DataHub.Wholesale.Calculations.Interfaces.Models.CalculationState;

namespace Energinet.DataHub.Wholesale.Calculations.Application;

public class CalculationsClient : ICalculationsClient
{
    private readonly ICalculationRepository _calculationRepository;
    private readonly ICalculationDtoMapper _calculationDtoMapper;

    public CalculationsClient(ICalculationRepository calculationRepository, ICalculationDtoMapper calculationDtoMapper)
    {
        _calculationRepository = calculationRepository;
        _calculationDtoMapper = calculationDtoMapper;
    }

    public async Task<IEnumerable<CalculationDto>> GetCompletedAfterAsync(Instant? completedTime)
    {
        var calculations = await _calculationRepository.GetCompletedAfterAsync(completedTime).ConfigureAwait(false);
        return calculations.Select(_calculationDtoMapper.Map);
    }

    public async Task<CalculationDto> GetAsync(Guid calculationId)
    {
        var calculation = await _calculationRepository.GetAsync(calculationId).ConfigureAwait(false);
        return _calculationDtoMapper.Map(calculation);
    }

    public async Task<IEnumerable<CalculationDto>> SearchAsync(
        IEnumerable<string> filterByGridAreaCodes,
        CalculationState? filterByExecutionState,
        DateTimeOffset? minExecutionTime,
        DateTimeOffset? maxExecutionTime,
        DateTimeOffset? periodStart,
        DateTimeOffset? periodEnd)
    {
        var minExecutionTimeStart = ConvertToInstant(minExecutionTime);
        var maxExecutionTimeStart = ConvertToInstant(maxExecutionTime);
        var periodStartInstant = ConvertToInstant(periodStart);
        var periodEndInstant = ConvertToInstant(periodEnd);

        return await SearchAsync(
            filterByGridAreaCodes,
            filterByExecutionState,
            minExecutionTimeStart,
            maxExecutionTimeStart,
            periodStartInstant,
            periodEndInstant,
            null).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<CalculationDto>> SearchAsync(
        IEnumerable<string> filterByGridAreaCodes,
        CalculationState filterByExecutionState,
        Instant periodStart,
        Instant periodEnd,
        CalculationType calculationType)
    {
        return await SearchAsync(
            filterByGridAreaCodes,
            filterByExecutionState,
            null,
            null,
            periodStart,
            periodEnd,
            calculationType).ConfigureAwait(false);
    }

    private async Task<IReadOnlyCollection<CalculationDto>> SearchAsync(
        IEnumerable<string> filterByGridAreaCodes,
        CalculationState? filterByExecutionState,
        Instant? minExecutionTimeStart,
        Instant? maxExecutionTimeStart,
        Instant? periodStartInstant,
        Instant? periodEndInstant,
        CalculationType? calculationType)
    {
        var executionStateFilter = GetCalculationExecutionStates(filterByExecutionState);

        var gridAreaFilter = filterByGridAreaCodes
            .Select(g => new GridAreaCode(g))
            .ToList();

        var calculations = await _calculationRepository
            .SearchAsync(
                gridAreaFilter,
                executionStateFilter,
                minExecutionTimeStart,
                maxExecutionTimeStart,
                periodStartInstant,
                periodEndInstant,
                calculationType)
            .ConfigureAwait(false);

        return calculations.Select(_calculationDtoMapper.Map).ToList();
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
