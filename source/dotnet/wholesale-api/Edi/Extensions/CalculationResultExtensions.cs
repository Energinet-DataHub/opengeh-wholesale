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
using Energinet.DataHub.Wholesale.Edi.Exceptions;
using Energinet.DataHub.Wholesale.Edi.Models;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.Edi.Extensions;

public static class CalculationResultExtensions
{
    public static IReadOnlyCollection<AggregatedTimeSeriesResult> GetLatestCalculationsResultsPerDay(
        this IReadOnlyCollection<AggregatedTimeSeries> calculationResults,
        IReadOnlyCollection<LatestCalculationForPeriod> latestCalculationsForPeriod)
    {
        var latestCalculationResults = new List<AggregatedTimeSeriesResult>();
        if (calculationResults.Count == 0)
            return latestCalculationResults;

        foreach (var latestCalculation in latestCalculationsForPeriod.OrderBy(x => x.PeriodStart))
        {
            var calculationResult = calculationResults.FirstOrDefault(x => x.BatchId == latestCalculation.BatchId);
            if (calculationResult == null)
                throw new MissingCalculationResultException($"No calculation result found for batch {latestCalculation.BatchId}");

            var timeSeriesPointWithinPeriod = GetTimeSeriesPointWithinPeriodPerDay(
                calculationResult.TimeSeriesPoints,
                latestCalculation.PeriodStart,
                latestCalculation.PeriodEnd);

            latestCalculationResults.AddRange(timeSeriesPointWithinPeriod.Select(x =>
                new AggregatedTimeSeriesResult(
                    latestCalculation.CalculationVersion,
                    x.Min(point => point.Time).ToInstant(),
                    x.Min(point => point.Time).ToInstant().Plus(Duration.FromDays(1)),
                    calculationResult.GridArea,
                    x,
                    calculationResult.TimeSeriesType,
                    calculationResult.ProcessType)));
        }

        return latestCalculationResults;
    }

    private static IReadOnlyCollection<EnergyTimeSeriesPoint[]> GetTimeSeriesPointWithinPeriodPerDay(
        EnergyTimeSeriesPoint[] timeSeriesPoints,
        Instant periodStart,
        Instant periodEnd)
    {
        var timeSeriesPointsWithinPeriodPerDay = new List<EnergyTimeSeriesPoint[]>();

        var currentTime = periodStart;
        while (currentTime <= periodEnd)
        {
            currentTime = currentTime.Plus(Duration.FromDays(1));
            var pointsForTheCurrentDay = timeSeriesPoints
                .Where(x => x.Time.ToInstant() >= periodStart && x.Time.ToInstant() < currentTime)
                .ToArray();
            if (pointsForTheCurrentDay.Any())
                timeSeriesPointsWithinPeriodPerDay.Add(pointsForTheCurrentDay);
            periodStart = currentTime;
        }

        return timeSeriesPointsWithinPeriodPerDay;
    }
}
