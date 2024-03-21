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
using NodaTime.Text;

namespace Energinet.DataHub.Wholesale.Edi.Validation.WholesaleServicesRequest.Rules;

public sealed class PeriodValidationRule(DateTimeZone dateTimeZone, IClock clock)
    : IValidationRule<DataHub.Edi.Requests.WholesaleServicesRequest>
{
    private static readonly ValidationError _invalidDateFormat =
        new(
            "Forkert dato format for {PropertyName}, skal være YYYY-MM-DDT22:00:00Z eller YYYY-MM-DDT23:00:00Z / Wrong date format for {PropertyName}, must be YYYY-MM-DDT22:00:00Z or YYYY-MM-DDT23:00:00Z",
            "D66");

    private static readonly ValidationError _startDateMustBeLessThanOrEqualTo3YearsAnd2Months =
        new(
            "Der kan ikke anmodes om data for mere end 3 år og 2 måneder tilbage i tid / It is not possible to request data longer than 3 years and 2 months back in time",
            "E17");

    private static readonly ValidationError _invalidWinterMidnightFormat =
        new(
            "Forkert dato format for {PropertyName}, skal være YYYY-MM-DDT23:00:00Z / Wrong date format for {PropertyName}, must be YYYY-MM-DDT23:00:00Z",
            "D66");

    private static readonly ValidationError _invalidSummerMidnightFormat =
        new(
            "Forkert dato format for {PropertyName}, skal være YYYY-MM-DDT22:00:00Z / Wrong date format for {PropertyName}, must be YYYY-MM-DDT22:00:00Z",
            "D66");

    public Task<IList<ValidationError>> ValidateAsync(DataHub.Edi.Requests.WholesaleServicesRequest subject)
    {
        ArgumentNullException.ThrowIfNull(subject);

        var periodStart = subject.PeriodStart;

        // This should not be possible, but we need to check for null due to the nullable type
        ArgumentNullException.ThrowIfNull(periodStart);

        var errors = new List<ValidationError>();

        var startInstant = ParseToInstant(periodStart, "Period Start", errors);

        if (startInstant is null)
            return Task.FromResult<IList<ValidationError>>(errors);

        MustBeMidnight(startInstant.Value, "Period Start", errors);
        AddErrorIfPeriodStartIsTooOld(startInstant.Value, errors);

        var periodEnd = subject.PeriodEnd;

        if (periodEnd == string.Empty)
            return Task.FromResult<IList<ValidationError>>(errors);

        var endInstant = ParseToInstant(periodEnd, "Period End", errors);

        if (endInstant is null)
            return Task.FromResult<IList<ValidationError>>(errors);

        MustBeMidnight(endInstant.Value, "Period End", errors);

        return Task.FromResult<IList<ValidationError>>(errors);
    }

    private static Instant? ParseToInstant(
        string dateTimeString,
        string propertyName,
        ICollection<ValidationError> errors)
    {
        var parseResult = InstantPattern.General.Parse(dateTimeString);

        if (parseResult.Success)
            return parseResult.Value;

        errors.Add(_invalidDateFormat.WithPropertyName(propertyName));
        return null;
    }

    private void AddErrorIfPeriodStartIsTooOld(Instant periodStart, ICollection<ValidationError> errors)
    {
        var zonedStartDateTime = new ZonedDateTime(periodStart, dateTimeZone);
        var zonedCurrentDateTime = new ZonedDateTime(clock.GetCurrentInstant(), dateTimeZone);

        if (zonedStartDateTime.LocalDateTime.Date
            < zonedCurrentDateTime.LocalDateTime.Date.PlusYears(-3).PlusMonths(-2))
        {
            errors.Add(_startDateMustBeLessThanOrEqualTo3YearsAnd2Months);
        }
    }

    private void MustBeMidnight(Instant instant, string propertyName, ICollection<ValidationError> errors)
    {
        var zonedDateTime = new ZonedDateTime(instant, dateTimeZone);

        if (zonedDateTime.TimeOfDay == LocalTime.Midnight)
            return;

        errors.Add(zonedDateTime.IsDaylightSavingTime()
            ? _invalidSummerMidnightFormat.WithPropertyName(propertyName)
            : _invalidWinterMidnightFormat.WithPropertyName(propertyName));
    }
}
