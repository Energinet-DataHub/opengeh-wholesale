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

namespace Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSeries.Rules;

public class PeriodValidationRule : IValidationRule<AggregatedTimeSeriesRequest>
{
    private readonly DateTimeZone _dateTimeZone;
    private readonly IClock _clock;
    private readonly int _maxAllowedPeriodSizeInMonths = 1;
    private readonly int _allowedTimeFrameInYearsFromNow = 3;

    private static readonly ValidationError _invalidDateFormat = new("Forkert dato format for {PropertyName}, skal være YYYY-MM-DDT22:00:00Z eller YYYY-MM-DDT23:00:00Z / Wrong date format for {PropertyName}, must be YYYY-MM-DDT22:00:00Z or YYYY-MM-DDT23:00:00Z", "D66");
    private static readonly ValidationError _invalidWinterMidnightFormat = new("Forkert dato format for {PropertyName}, skal være YYYY-MM-DDT23:00:00Z / Wrong date format for {PropertyName}, must be YYYY-MM-DDT23:00:00Z", "D66");
    private static readonly ValidationError _invalidSummerMidnightFormat = new("Forkert dato format for {PropertyName}, skal være YYYY-MM-DDT22:00:00Z / Wrong date format for {PropertyName}, must be YYYY-MM-DDT22:00:00Z", "D66");
    private static readonly ValidationError _startDateMustBeLessThen3Years = new("Dato må max være 3 år tilbage i tid / Can maximum be 3 years back in time", "E17");
    private static readonly ValidationError _periodIsGreaterThenAllowedPeriodSize = new("Dato må kun være for 1 måned af gangen / Can maximum be for a 1 month period", "E17");
    private static readonly ValidationError _missingStartOrAndEndDate = new("Start og slut dato skal udfyldes / Start and end date must be present in request", "E50");

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
            errors.Add(_missingStartOrAndEndDate);
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
            errors.Add(_periodIsGreaterThenAllowedPeriodSize);
    }

    private void StartDateMustBeGreaterThenAllowedYears(Instant start, List<ValidationError> errors)
    {
        var zonedStartDateTime = new ZonedDateTime(start, _dateTimeZone);
        var zonedCurrentDateTime = new ZonedDateTime(_clock.GetCurrentInstant(), _dateTimeZone);
        var latestStartDate = zonedCurrentDateTime.LocalDateTime.PlusYears(-_allowedTimeFrameInYearsFromNow);

        if (zonedStartDateTime.LocalDateTime < latestStartDate)
            errors.Add(_startDateMustBeLessThen3Years);
    }

    private Instant? ParseToInstant(string dateTimeString, string propertyName, List<ValidationError> errors)
    {
        var parseResult = InstantPattern.General.Parse(dateTimeString);
        if (parseResult.Success)
            return parseResult.Value;

        errors.Add(_invalidDateFormat.WithPropertyName(propertyName));
        return null;
    }

    private void MustBeMidnight(Instant instant, string propertyName, List<ValidationError> errors)
    {
        var zonedDateTime = new ZonedDateTime(instant, _dateTimeZone);

        if (zonedDateTime.TimeOfDay == LocalTime.Midnight) return;

        if (zonedDateTime.IsDaylightSavingTime())
        {
            errors.Add(_invalidSummerMidnightFormat.WithPropertyName(propertyName));
        }
        else
        {
            errors.Add(_invalidWinterMidnightFormat.WithPropertyName(propertyName));
        }
    }
}
