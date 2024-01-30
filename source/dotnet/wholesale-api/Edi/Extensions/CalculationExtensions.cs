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
using Energinet.DataHub.Wholesale.Edi.Exceptions;
using Energinet.DataHub.Wholesale.Edi.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Edi.Extensions;

public static class CalculationExtensions
{
    public static IReadOnlyCollection<LatestCalculationForPeriod> FindLatestCalculations(
        this IReadOnlyCollection<CalculationDto> calculations,
        Instant periodStart,
        Instant periodEnd,
        DateTimeZone dateTimeZone)
    {
        var remainingDaysInPeriod = GetDaysInPeriod(periodStart, periodEnd, dateTimeZone);
        var latestCalculationsForPeriod = new List<LatestCalculationForPeriod>();
        foreach (var calculation in calculations.OrderByDescending(x => x.Version))
        {
            var periodsWhereCalculationIsLatest =
                GetPeriodsWhereCalculationIsLatest(calculation, latestCalculationsForPeriod, remainingDaysInPeriod);

            foreach (var periodWhereCalculationIsLatest in periodsWhereCalculationIsLatest)
            {
                var daysInPeriod = GetDaysInPeriod(periodWhereCalculationIsLatest.PeriodStart, periodWhereCalculationIsLatest.PeriodEnd, dateTimeZone);
                foreach (var day in daysInPeriod.Where(x => remainingDaysInPeriod.Contains(x)))
                {
                    remainingDaysInPeriod.Remove(day);
                }

                latestCalculationsForPeriod.Add(periodWhereCalculationIsLatest);
            }

            if (remainingDaysInPeriod.Count == 0)
                return latestCalculationsForPeriod;
        }

        if (latestCalculationsForPeriod.Count == 0)
            return latestCalculationsForPeriod;

        throw new MissingCalculationException($"No calculation found for dates: {string.Join(", ", remainingDaysInPeriod)}");
    }

    private static IReadOnlyCollection<LatestCalculationForPeriod> GetPeriodsWhereCalculationIsLatest(
        CalculationDto calculation,
        IReadOnlyCollection<LatestCalculationForPeriod> latestCalculationsForPeriod,
        IReadOnlyCollection<Instant> remainingDaysInPeriod)
    {
        var result = new List<LatestCalculationForPeriod>();
        var calculationStart = Instant.FromDateTimeOffset(calculation.PeriodStart);
        var calculationEnd = Instant.FromDateTimeOffset(calculation.PeriodEnd);
        Instant? startOfPeriod = null;
        foreach (var remainDay in remainingDaysInPeriod)
        {
            if (startOfPeriod == null
                && calculationStart <= remainDay && calculationEnd >= remainDay)
            {
                startOfPeriod = remainDay;
            }

            if (startOfPeriod != null
                && (calculationEnd == remainDay
                    || NextDayInExistingPeriod(remainDay, latestCalculationsForPeriod)))
            {
                result.Add(new LatestCalculationForPeriod(
                    startOfPeriod.Value,
                    remainDay,
                    calculation.BatchId,
                    calculation.Version));
                startOfPeriod = null;
            }
        }

        return result;
    }

    private static bool NextDayInExistingPeriod(
        Instant remainDay,
        IReadOnlyCollection<LatestCalculationForPeriod> latestCalculationsForPeriod)
    {
        return latestCalculationsForPeriod
            .Any(x => x.PeriodStart <= remainDay.Plus(Duration.FromDays(1))
                      && x.PeriodEnd >= remainDay.Plus(Duration.FromDays(1)));
    }

    private static List<Instant> GetDaysInPeriod(Instant periodStart, Instant periodEnd, DateTimeZone dateTimeZone)
    {
        var periodStartInTimeZone = new ZonedDateTime(periodStart, dateTimeZone);
        var periodEndInTimeZone = new ZonedDateTime(periodEnd, dateTimeZone);
        var period = Period.Between(
            periodStartInTimeZone.LocalDateTime,
            periodEndInTimeZone.LocalDateTime,
            PeriodUnits.Days);

        var datesInPeriod = new List<Instant>();
        for (var days = 0; days <= period.Days; ++days)
        {
            datesInPeriod.Add(periodStart.Plus(Duration.FromDays(days)));
        }

        return datesInPeriod;
    }
}
