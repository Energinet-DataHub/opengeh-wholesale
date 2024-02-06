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
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.Edi.Exceptions;
using NodaTime;
using Period = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.Period;

namespace Energinet.DataHub.Wholesale.Edi.Calculations;

public class LatestCalculationsForPeriod
{
    private readonly DateTimeZone _dateTimeZone;

    public LatestCalculationsForPeriod(DateTimeZone dateTimeZone)
    {
        _dateTimeZone = dateTimeZone;
    }

    public IReadOnlyCollection<CalculationForPeriod> FindLatestCalculationsForPeriod(
        Instant periodStart,
        Instant periodEnd,
        IReadOnlyCollection<CalculationDto> calculations)
    {
        var remainingDaysInPeriod = GetDaysInPeriod(periodStart, periodEnd);
        var latestCalculationsForPeriod = new List<CalculationForPeriod>();
        foreach (var calculation in calculations.OrderByDescending(x => x.Version))
        {
            var periodsWhereCalculationIsLatest =
                GetPeriodsWhereCalculationIsLatest(calculation, latestCalculationsForPeriod, remainingDaysInPeriod);

            foreach (var periodWhereCalculationIsLatest in periodsWhereCalculationIsLatest)
            {
                var daysInPeriod = GetDaysInPeriod(
                    periodWhereCalculationIsLatest.Period.Start,
                    periodWhereCalculationIsLatest.Period.End);
                foreach (var day in daysInPeriod.Where(x => remainingDaysInPeriod.Contains(x)))
                {
                    remainingDaysInPeriod.Remove(day);
                }

                latestCalculationsForPeriod.Add(periodWhereCalculationIsLatest);
            }

            if (remainingDaysInPeriod.Count == 0)
                return latestCalculationsForPeriod.OrderBy(x => x.Period.Start).ToList();
        }

        if (latestCalculationsForPeriod.Count == 0)
            return latestCalculationsForPeriod.OrderBy(x => x.Period.Start).ToList();

        throw new MissingCalculationException($"No calculation found for dates: {string.Join(", ", remainingDaysInPeriod)}");
    }

    private static IReadOnlyCollection<CalculationForPeriod> GetPeriodsWhereCalculationIsLatest(
        CalculationDto calculation,
        IReadOnlyCollection<CalculationForPeriod> latestCalculationsForPeriod,
        IReadOnlyCollection<Instant> remainingDaysInPeriod)
    {
        var result = new List<CalculationForPeriod>();
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
                result.Add(new CalculationForPeriod(
                    new Period(startOfPeriod.Value, remainDay),
                    calculation.CalculationId,
                    calculation.Version));
                startOfPeriod = null;
            }
        }

        return result;
    }

    private static bool NextDayInExistingPeriod(
        Instant remainDay,
        IReadOnlyCollection<CalculationForPeriod> latestCalculationsForPeriod)
    {
        return latestCalculationsForPeriod
            .Any(x => x.Period.Start <= remainDay.Plus(Duration.FromDays(1))
                      && x.Period.End >= remainDay.Plus(Duration.FromDays(1)));
    }

    private List<Instant> GetDaysInPeriod(Instant periodStart, Instant periodEnd)
    {
        var periodStartInTimeZone = new ZonedDateTime(periodStart, _dateTimeZone);
        var periodEndInTimeZone = new ZonedDateTime(periodEnd, _dateTimeZone);
        var period = NodaTime.Period.Between(
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
