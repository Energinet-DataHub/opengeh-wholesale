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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.Edi.Exceptions;
using Energinet.DataHub.Wholesale.Edi.Mappers;
using Energinet.DataHub.Wholesale.Edi.Models;
using Microsoft.Extensions.Logging;
using Period = Energinet.DataHub.Wholesale.Edi.Models.Period;

namespace Energinet.DataHub.Wholesale.Edi.Calculations;

public class CompletedCalculationRetriever
{
    private static readonly List<RequestedCalculationType> _correctionVersionsInOrder =
    [
        RequestedCalculationType.ThirdCorrection,
        RequestedCalculationType.SecondCorrection,
        RequestedCalculationType.FirstCorrection
    ];

    private readonly LatestCalculationsForPeriod _latestCalculationsForPeriod;
    private readonly ICalculationsClient _calculationsClient;
    private readonly ILogger<CompletedCalculationRetriever> _logger;

    public CompletedCalculationRetriever(
        LatestCalculationsForPeriod latestCalculationsForPeriod,
        ICalculationsClient calculationsClient,
        ILogger<CompletedCalculationRetriever> logger)
    {
        _latestCalculationsForPeriod = latestCalculationsForPeriod;
        _calculationsClient = calculationsClient;
        _logger = logger;
    }

    public virtual async Task<IReadOnlyCollection<CalculationForPeriod>> GetLatestCompletedCalculationsForPeriodAsync(
        string? gridAreaCode, Period period, RequestedCalculationType requestedCalculationType)
    {
        IReadOnlyCollection<CalculationDto> completedCalculationsForPeriod = Array.Empty<CalculationDto>();

        if (requestedCalculationType == RequestedCalculationType.LatestCorrection)
        {
            foreach (var correctionVersion in _correctionVersionsInOrder)
            {
                completedCalculationsForPeriod = await GetCompletedCalculationsForPeriodAsync(
                        gridAreaCode,
                        period,
                        correctionVersion)
                    .ConfigureAwait(true);

                // If we found any calculations for this correction version, it means the correction exists, and we should use those calculations
                if (completedCalculationsForPeriod.Any())
                    break;
            }
        }
        else
        {
            completedCalculationsForPeriod = await GetCompletedCalculationsForPeriodAsync(
                    gridAreaCode,
                    period,
                    requestedCalculationType)
                .ConfigureAwait(true);
        }

        try
        {
            return _latestCalculationsForPeriod.FindLatestCalculationsForPeriod(
                period.Start,
                period.End,
                completedCalculationsForPeriod);
        }
        catch (MissingCalculationException e)
        {
            _logger.LogError(
                e,
                "Failed to find latest calculations for calculation type {RequestedCalculationType}, grid area code: {GridAreaCode}, within period: {Start}  - {End}",
                requestedCalculationType,
                gridAreaCode,
                period.Start,
                period.End);
            throw;
        }
    }

    private async Task<IReadOnlyCollection<CalculationDto>> GetCompletedCalculationsForPeriodAsync(string? gridAreaCode, Period period, RequestedCalculationType requestedCalculationType)
    {
        return await _calculationsClient
            .SearchAsync(
                filterByGridAreaCodes: gridAreaCode != null
                    ? new[] { gridAreaCode }
                    : new string[] { },
                filterByExecutionState: CalculationState.Completed,
                periodStart: period.Start,
                periodEnd: period.End,
                calculationType: CalculationTypeMapper.FromRequestedCalculationType(
                    requestedCalculationType))
            .ConfigureAwait(false);
    }
}
