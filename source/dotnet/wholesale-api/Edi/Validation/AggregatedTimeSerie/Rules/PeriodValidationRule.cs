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

using Energinet.DataHub.Edi.Requests;
using NodaTime;
using NodaTime.Text;

namespace Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSerie.Rules;

public class PeriodValidationRule : IValidationRule<AggregatedTimeSeriesRequest>
{
    private readonly DateTimeZone _dateTimeZone;
    private readonly IClock _clock;
    private readonly int _maxAllowedPeriodSizeInMonths = 1;
    private readonly int _allowedTimeFrameInYearsFromNow = 3;

    public PeriodValidationRule(DateTimeZone dateTimeZone, IClock clock)
    {
        _dateTimeZone = dateTimeZone;
        _clock = clock;
    }

    public IList<ValidationError> Validate(AggregatedTimeSeriesRequest subject)
    {
        if (subject == null) throw new ArgumentNullException(nameof(subject));
        var period = subject.Period;
        if (period == null) throw new ArgumentNullException(nameof(period));
        var errors = new List<ValidationError>();

        if (MissingDates(period.Start, period.End, errors)) return errors;

        var startInstant = ParseToInstant(period.Start, "Start date", errors);
        var endInstant = ParseToInstant(period.End, "End date", errors);

        if (startInstant == null || endInstant == null) return errors;

        MustBeMidnight(startInstant.Value, "Start date", errors);
        MustBeMidnight(endInstant.Value, "End date", errors);

        StartDateMustBeGreaterThenAllowedYears(startInstant.Value, errors);
        IntervalMustBeWithinAllowedPeriodSize(startInstant.Value, endInstant.Value, errors);

        return errors;
    }

    private bool MissingDates(string start, string end, List<ValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(start) || string.IsNullOrWhiteSpace(end))
        {
            errors.Add(ValidationError.MissingStartOrAndEndDate);
            return true;
        }

        return false;
    }

    private void IntervalMustBeWithinAllowedPeriodSize(Instant start, Instant end, List<ValidationError> errors)
    {
        var zonedStartDateTime = new ZonedDateTime(start, _dateTimeZone);
        var zonedEndDateTime = new ZonedDateTime(end, _dateTimeZone);
        var monthsFromStart = zonedStartDateTime.LocalDateTime.PlusMonths(_maxAllowedPeriodSizeInMonths);
        if (zonedEndDateTime.LocalDateTime > monthsFromStart)
            errors.Add(ValidationError.PeriodIsGreaterThenAllowedPeriodSize);
    }

    private void StartDateMustBeGreaterThenAllowedYears(Instant start, List<ValidationError> errors)
    {
        var zonedStartDateTime = new ZonedDateTime(start, _dateTimeZone);
        var zonedCurrentDateTime = new ZonedDateTime(_clock.GetCurrentInstant(), _dateTimeZone);
        var latestStartDate = zonedCurrentDateTime.LocalDateTime.PlusYears(-_allowedTimeFrameInYearsFromNow);

        if (zonedStartDateTime.LocalDateTime < latestStartDate)
            errors.Add(ValidationError.StartDateMustBeLessThen3Years);
    }

    private Instant? ParseToInstant(string dateTimeString, string propertyName, List<ValidationError> errors)
    {
        var parseResult = InstantPattern.General.Parse(dateTimeString);
        if (parseResult.Success)
            return parseResult.Value;

        errors.Add(ValidationError.InvalidDateFormat.WithPropertyName(propertyName));
        return null;
    }

    private void MustBeMidnight(Instant instant, string propertyName, List<ValidationError> errors)
    {
        var zonedDateTime = new ZonedDateTime(instant, _dateTimeZone);
        if (zonedDateTime.TimeOfDay != LocalTime.Midnight)
            errors.Add(ValidationError.InvalidDateFormat.WithPropertyName(propertyName));
    }
}
