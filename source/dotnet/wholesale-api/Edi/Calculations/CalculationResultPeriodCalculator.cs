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

namespace Energinet.DataHub.Wholesale.Edi.Calculations;

public class CalculationResultPeriodCalculator
{
    public IReadOnlyCollection<AggregatedTimeSeriesResult> GetLatestCalculationsResultsPerDay(
        IReadOnlyCollection<LatestCalculationForPeriod> latestCalculationsForPeriod,
        IReadOnlyCollection<AggregatedTimeSeries> calculationResults)
    {
        var latestCalculationResults = new List<AggregatedTimeSeriesResult>();
        if (calculationResults.Count == 0)
            return latestCalculationResults;

        foreach (var latestCalculation in latestCalculationsForPeriod.OrderBy(x => x.Period.Start))
        {
            var calculationResult = calculationResults.FirstOrDefault(x => x.BatchId == latestCalculation.BatchId);
            if (calculationResult == null)
                throw new MissingCalculationResultException($"No calculation result found for batch {latestCalculation.BatchId}");

            // Calculation result is exclusive of the period end, so we add a day to include the results from the last day.
            var periodEndForCalculationResults = latestCalculation.Period.End.Plus(Duration.FromDays(1));
            var timeSeriesPointWithinPeriod = GetTimeSeriesPointWithinPeriod(
                calculationResult.TimeSeriesPoints,
                latestCalculation.Period.Start,
                periodEndForCalculationResults);

            latestCalculationResults.Add(
                new AggregatedTimeSeriesResult(
                    latestCalculation.CalculationVersion,
                    latestCalculation.Period.Start,
                    periodEndForCalculationResults,
                    calculationResult.GridArea,
                    timeSeriesPointWithinPeriod,
                    calculationResult.TimeSeriesType,
                    calculationResult.ProcessType));
        }

        return latestCalculationResults;
    }

    private static EnergyTimeSeriesPoint[] GetTimeSeriesPointWithinPeriod(
        EnergyTimeSeriesPoint[] timeSeriesPoints,
        Instant periodStart,
        Instant periodEnd)
    {
        return timeSeriesPoints
            .Where(x => x.Time.ToInstant() >= periodStart && x.Time.ToInstant() < periodEnd)
            .ToArray();
    }
}
