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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Edi.Exceptions;
using Energinet.DataHub.Wholesale.Edi.Models;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.Edi;

public class LatestCalculationResultsForPeriod
{
    private readonly Instant _periodStart;
    private readonly Instant _periodEnd;
    private readonly DateTimeZone _dateTimeZone;
    private readonly IList<Instant> _remainingDaysInPeriod;
    private readonly IReadOnlyCollection<CalculationDto> _calculations;
    private readonly ICollection<LatestCalculationForPeriod> _latestCalculationsForPeriod;

    public LatestCalculationResultsForPeriod(
        Instant periodStart,
        Instant periodEnd,
        DateTimeZone dateTimeZone,
        IReadOnlyCollection<CalculationDto> calculations)
    {
        _periodStart = periodStart;
        _periodEnd = periodEnd;
        _dateTimeZone = dateTimeZone;
        _calculations = calculations;
        _latestCalculationsForPeriod = new List<LatestCalculationForPeriod>();
        _remainingDaysInPeriod = GetDaysInPeriod(periodStart, periodEnd);
        FindLatestCalculations();
    }

    public IReadOnlyCollection<LatestCalculationForPeriod> LatestCalculationForPeriods => _latestCalculationsForPeriod.ToList().AsReadOnly();

    public IReadOnlyCollection<AggregatedTimeSeriesResult> GetLatestCalculationsResultsPerDay(IReadOnlyCollection<AggregatedTimeSeries> calculationResults)
    {
        var latestCalculationResults = new List<AggregatedTimeSeriesResult>();
        if (calculationResults.Count == 0)
            return latestCalculationResults;

        foreach (var latestCalculation in LatestCalculationForPeriods.OrderByDescending(x => x.PeriodStart))
        {
            var calculationResult = calculationResults.FirstOrDefault(x => x.BatchId == latestCalculation.BatchId);
            if (calculationResult == null)
                throw new MissingCalculationResultException($"No calculation result found for batch {latestCalculation.BatchId}");

            var timeSeriesPointWithinPeriod = GetTimeSeriesPointWithinPeriod(calculationResult.TimeSeriesPoints, latestCalculation.PeriodStart, latestCalculation.PeriodEnd);
            if (timeSeriesPointWithinPeriod.Count() != 0)
            {
                latestCalculationResults.Add(new AggregatedTimeSeriesResult(
                    latestCalculation.CalculationVersion,
                    calculationResult.GridArea,
                    timeSeriesPointWithinPeriod,
                    calculationResult.TimeSeriesType,
                    calculationResult.ProcessType));
            }
        }

        return latestCalculationResults;
    }

    private void FindLatestCalculations()
    {
        foreach (var calculation in _calculations.OrderByDescending(x => x.Version))
        {
            var calculationPeriodStart = ConvertToInstant(calculation.PeriodStart)!.Value;
            var calculationPeriodEnd = ConvertToInstant(calculation.PeriodEnd)!.Value;
            if (_latestCalculationsForPeriod.Count == 0)
            {
                AddPeriod(new LatestCalculationForPeriod(
                    calculationPeriodStart,
                    calculationPeriodEnd,
                    calculation.BatchId,
                    calculation.Version));
            }

            var earliestStartDate = GetEarliestStartDate().Minus(Duration.FromMinutes(15));
            if (calculationPeriodStart < earliestStartDate)
            {
                var periodStart = calculationPeriodStart > _periodStart ? calculationPeriodStart : _periodStart;
                var periodEnd = earliestStartDate < calculationPeriodEnd ? earliestStartDate : calculationPeriodEnd;
                AddPeriod(new LatestCalculationForPeriod(
                    periodStart,
                    periodEnd,
                    calculation.BatchId,
                    calculation.Version));
            }

            var latestEndDate = GetLatestEndDate().Plus(Duration.FromDays(1));
            if (calculationPeriodEnd > latestEndDate)
            {
                var periodStart = latestEndDate > calculationPeriodStart ? latestEndDate : calculationPeriodStart;
                var periodEnd = calculationPeriodEnd < _periodEnd ? calculationPeriodEnd : _periodEnd;
                AddPeriod(new LatestCalculationForPeriod(
                    periodStart,
                    periodEnd,
                    calculation.BatchId,
                    calculation.Version));
            }

            if (_remainingDaysInPeriod.Count == 0)
                return;
        }

        if (_latestCalculationsForPeriod.Count == 0)
            return;

        throw new MissingCalculationException($"No calculation found for dates: {string.Join(", ", _remainingDaysInPeriod)}");
    }

    private void AddPeriod(LatestCalculationForPeriod latestCalculationForPeriod)
    {
        var daysInPeriod = GetDaysInPeriod(latestCalculationForPeriod.PeriodStart, latestCalculationForPeriod.PeriodEnd);
        foreach (var day in daysInPeriod.Where(x => _remainingDaysInPeriod.Contains(x)))
        {
            _remainingDaysInPeriod.Remove(day);
        }

        _latestCalculationsForPeriod.Add(latestCalculationForPeriod);
    }

    private Instant GetLatestEndDate()
    {
        return _latestCalculationsForPeriod.Max(x => x.PeriodEnd);
    }

    private Instant GetEarliestStartDate()
    {
        return _latestCalculationsForPeriod.Min(x => x.PeriodStart);
    }

    private static Instant? ConvertToInstant(DateTimeOffset? dateTimeOffset)
    {
        return dateTimeOffset == null
            ? null
            : Instant.FromDateTimeOffset(dateTimeOffset.Value);
    }

    private EnergyTimeSeriesPoint[] GetTimeSeriesPointWithinPeriod(EnergyTimeSeriesPoint[] timeSeriesPoints, Instant periodStart, Instant periodEnd)
    {
        return timeSeriesPoints
            .Where(x => x.Time.ToInstant() >= periodStart && x.Time.ToInstant() <= periodEnd)
            .ToArray();
    }

    private List<Instant> GetDaysInPeriod(Instant periodStart, Instant periodEnd)
    {
        var periodStartInTimeZone = new ZonedDateTime(periodStart, _dateTimeZone);
        var periodEndInTimeZone = new ZonedDateTime(periodEnd, _dateTimeZone);
        var period = Period.Between(
            periodStartInTimeZone.LocalDateTime,
            periodEndInTimeZone.LocalDateTime,
            PeriodUnits.Days);

        var datesInPeriod = new List<Instant>();
        // if (period.Days == 0)
        //     datesInPeriod.Add(periodStart);
        for (var days = 0; days <= period.Days; ++days)
        {
            datesInPeriod.Add(periodStart.Plus(Duration.FromDays(days)));
        }

        return datesInPeriod;
    }
}
