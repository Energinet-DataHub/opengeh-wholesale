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

using NodaTime;

namespace Energinet.DataHub.Wholesale.Edi.Validation.Helpers;

public class PeriodValidationHelper(DateTimeZone dateTimeZone, IClock clock)
{
    public bool IsMidnight(Instant instant, out ZonedDateTime zonedDateTime)
    {
        zonedDateTime = new ZonedDateTime(instant, dateTimeZone);

        return zonedDateTime.TimeOfDay == LocalTime.Midnight;
    }

    public bool IsDateOlderThanAllowed(Instant date, int maxYears, int maxMonths)
    {
        var zonedStartDateTime = new ZonedDateTime(date, dateTimeZone);
        var zonedCurrentDateTime = new ZonedDateTime(clock.GetCurrentInstant(), dateTimeZone);
        var latestStartDate = zonedCurrentDateTime.LocalDateTime.PlusYears(-maxYears).PlusMonths(-maxMonths);

        return zonedStartDateTime.LocalDateTime < latestStartDate;
    }

    public bool IntervalMustBeLessThanAllowedPeriodSize(Instant start, Instant end, int maxAllowedPeriodSizeInMonths)
    {
        var zonedStartDateTime = new ZonedDateTime(start, dateTimeZone);
        var zonedEndDateTime = new ZonedDateTime(end, dateTimeZone);
        var monthsFromStart = zonedStartDateTime.LocalDateTime.PlusMonths(maxAllowedPeriodSizeInMonths);

        return zonedEndDateTime.LocalDateTime > monthsFromStart;
    }

    public bool IsMonthOlder3Years2Months(Instant periodStart)
    {
        var zonedDateTime = new ZonedDateTime(periodStart, dateTimeZone);
        var zonedCurrentDataTime = new ZonedDateTime(clock.GetCurrentInstant(), dateTimeZone);
        var threeYearsAndTwoMonthsAgo = zonedCurrentDataTime.LocalDateTime.PlusYears(-3).PlusMonths(-2);

        if (zonedDateTime.Year > threeYearsAndTwoMonthsAgo.Year)
            return false;

        if (zonedDateTime.Year == threeYearsAndTwoMonthsAgo.Year)
            return zonedDateTime.Month < threeYearsAndTwoMonthsAgo.Month;

        return true;
    }
}
