﻿// Copyright 2020 Energinet DataHub A/S
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

    public bool IsStartDateOlderThanAllowed(Instant start, int maxYears, int maxMonths)
    {
        var zonedStartDateTime = new ZonedDateTime(start, dateTimeZone);
        var zonedCurrentDateTime = new ZonedDateTime(clock.GetCurrentInstant(), dateTimeZone);
        var latestStartDate = zonedCurrentDateTime.LocalDateTime.PlusYears(-maxYears).PlusMonths(maxMonths);

        return zonedStartDateTime.LocalDateTime < latestStartDate;
    }

    public bool IntervalMustBeLessThanAllowedPeriodSize(Instant start, Instant end, int maxAllowedPeriodSizeInMonths)
    {
        var zonedStartDateTime = new ZonedDateTime(start, dateTimeZone);
        var zonedEndDateTime = new ZonedDateTime(end, dateTimeZone);
        var monthsFromStart = zonedStartDateTime.LocalDateTime.PlusMonths(maxAllowedPeriodSizeInMonths);

        return zonedEndDateTime.LocalDateTime > monthsFromStart;
    }

    public bool PeriodStartIs3YearsAnd2MonthsAgo(Instant periodStart)
    {
        var zonedDateTime = new ZonedDateTime(periodStart, dateTimeZone);
        var zonedCurrentDataTime = new ZonedDateTime(clock.GetCurrentInstant(), dateTimeZone);
        return zonedDateTime.LocalDateTime.Month == zonedCurrentDataTime.LocalDateTime.Month - 2
               && zonedDateTime.LocalDateTime.Year == zonedCurrentDataTime.LocalDateTime.Year - 3;
    }
}
