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
using Energinet.DataHub.Wholesale.EDI.Mappers;
using Energinet.DataHub.Wholesale.EDI.Models;

namespace Energinet.DataHub.Wholesale.Edi.Calculations;

public class CompletedCalculationRetriever
{
    private readonly LatestCalculationsForPeriod _latestCalculationsForPeriod;
    private readonly ICalculationsClient _calculationsClient;

    public CompletedCalculationRetriever(
        LatestCalculationsForPeriod latestCalculationsForPeriod,
        ICalculationsClient calculationsClient)
    {
        _latestCalculationsForPeriod = latestCalculationsForPeriod;
        _calculationsClient = calculationsClient;
    }

    public async Task<IReadOnlyCollection<CalculationForPeriod>> GetLatestCompletedCalculationForRequestAsync(
        AggregatedTimeSeriesRequest aggregatedTimeSeriesRequest)
    {
        if (aggregatedTimeSeriesRequest.RequestedCalculationType == RequestedCalculationType.LatestCorrection)
        {
            aggregatedTimeSeriesRequest = aggregatedTimeSeriesRequest with
            {
                RequestedCalculationType = RequestedCalculationType.ThirdCorrection,
            };
            var calculationForLatestCorrection = await GetCompletedCalculationAsync(aggregatedTimeSeriesRequest).ConfigureAwait(true);
            if (!calculationForLatestCorrection.Any())
            {
                aggregatedTimeSeriesRequest = aggregatedTimeSeriesRequest with
                {
                    RequestedCalculationType = RequestedCalculationType.SecondCorrection,
                };
                calculationForLatestCorrection = await GetCompletedCalculationAsync(aggregatedTimeSeriesRequest).ConfigureAwait(true);
            }

            if (!calculationForLatestCorrection.Any())
            {
                aggregatedTimeSeriesRequest = aggregatedTimeSeriesRequest with
                {
                    RequestedCalculationType = RequestedCalculationType.FirstCorrection,
                };
                calculationForLatestCorrection = await GetCompletedCalculationAsync(aggregatedTimeSeriesRequest).ConfigureAwait(true);
            }

            return _latestCalculationsForPeriod.FindLatestCalculationsForPeriod(
                aggregatedTimeSeriesRequest.Period.Start,
                aggregatedTimeSeriesRequest.Period.End,
                calculationForLatestCorrection);
        }

        var calculations = await GetCompletedCalculationAsync(aggregatedTimeSeriesRequest).ConfigureAwait(true);

        return _latestCalculationsForPeriod.FindLatestCalculationsForPeriod(
            aggregatedTimeSeriesRequest.Period.Start,
            aggregatedTimeSeriesRequest.Period.End,
            calculations);
    }

    private async Task<IReadOnlyCollection<CalculationDto>> GetCompletedCalculationAsync(AggregatedTimeSeriesRequest aggregatedTimeSeriesRequest)
    {
        return await _calculationsClient
            .SearchAsync(
                filterByGridAreaCodes: aggregatedTimeSeriesRequest.AggregationPerRoleAndGridArea.GridAreaCode != null
                    ? new[] { aggregatedTimeSeriesRequest.AggregationPerRoleAndGridArea.GridAreaCode }
                    : new string[] { },
                filterByExecutionState: CalculationState.Completed,
                periodStart: aggregatedTimeSeriesRequest.Period.Start,
                periodEnd: aggregatedTimeSeriesRequest.Period.End,
                calculationType: CalculationTypeMapper.FromRequestedCalculationType(
                    aggregatedTimeSeriesRequest.RequestedCalculationType))
            .ConfigureAwait(false);
    }
}
