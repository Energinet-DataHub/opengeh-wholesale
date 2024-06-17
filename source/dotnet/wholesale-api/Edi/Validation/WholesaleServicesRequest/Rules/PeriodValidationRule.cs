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

using Energinet.DataHub.Wholesale.Edi.Validation.Helpers;
using NodaTime;
using NodaTime.Text;

namespace Energinet.DataHub.Wholesale.Edi.Validation.WholesaleServicesRequest.Rules;

public sealed class PeriodValidationRule(DateTimeZone dateTimeZone, PeriodValidationHelper periodValidationHelper)
    : IValidationRule<DataHub.Edi.Requests.WholesaleServicesRequest>
{
    private static readonly ValidationError _invalidDateFormat =
        new(
            "Forkert dato format for {PropertyName}, skal være YYYY-MM-DDT22:00:00Z eller YYYY-MM-DDT23:00:00Z / Wrong date format for {PropertyName}, must be YYYY-MM-DDT22:00:00Z or YYYY-MM-DDT23:00:00Z",
            "D66");

    private static readonly ValidationError _startDateMustBeLessThanOrEqualTo3YearsAnd3Months =
        new(
            "Der kan ikke anmodes om data for 3 år og 3 måneder tilbage i tid / It is not possible to request data 3 years and 3 months back in time",
            "E17");

    private static readonly ValidationError _invalidWinterMidnightFormat =
        new(
            "Forkert dato format for {PropertyName}, skal være YYYY-MM-DDT23:00:00Z / Wrong date format for {PropertyName}, must be YYYY-MM-DDT23:00:00Z",
            "D66");

    private static readonly ValidationError _invalidSummerMidnightFormat =
        new(
            "Forkert dato format for {PropertyName}, skal være YYYY-MM-DDT22:00:00Z / Wrong date format for {PropertyName}, must be YYYY-MM-DDT22:00:00Z",
            "D66");

    private static readonly ValidationError _invalidPeriodAcrossMonths =
        new(
            "Det er ikke muligt at anmode om data på tværs af måneder i forbindelse med en engrosfiksering eller korrektioner / It is not possible to request data across months in relation to wholesalefixing or corrections",
            "E17");

    private static readonly ValidationError _invalidPeriodLength =
        new(
            "Det er kun muligt at anmode om data på for en hel måned i forbindelse med en engrosfiksering eller korrektioner / It is only possible to request data for a full month in relation to wholesalefixing or corrections",
            "E17");

    public Task<IList<ValidationError>> ValidateAsync(DataHub.Edi.Requests.WholesaleServicesRequest subject)
    {
        ArgumentNullException.ThrowIfNull(subject);

        var periodStart = subject.PeriodStart;
        var periodEnd = subject.PeriodEnd;

        // This should not be possible, but we need to check for null due to the nullable type
        ArgumentNullException.ThrowIfNull(periodStart);
        ArgumentNullException.ThrowIfNull(periodEnd);

        var errors = new List<ValidationError>();

        var startInstant = ParseToInstant(periodStart, "Period Start", errors);
        var endInstant = ParseToInstant(periodEnd, "Period End", errors);

        if (startInstant is null || endInstant is null)
            return Task.FromResult<IList<ValidationError>>(errors);

        MustBeMidnight(startInstant.Value, "Period Start", errors);
        MustBeMidnight(endInstant.Value, "Period End", errors);
        MustBeAWholeMonth(startInstant.Value, endInstant.Value, errors);
        MustNotBe3YearsAnd3MonthsOld(startInstant.Value, errors);

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

    private void MustNotBe3YearsAnd3MonthsOld(Instant periodStart, ICollection<ValidationError> errors)
    {
        if (periodValidationHelper.IsMonthOlder3Years2Months(periodStart))
        {
            errors.Add(_startDateMustBeLessThanOrEqualTo3YearsAnd3Months);
        }
    }

    private void MustBeAWholeMonth(
        Instant periodStart,
        Instant periodEnd,
        ICollection<ValidationError> errors)
    {
        var zonedStartDateTime = new ZonedDateTime(periodStart, dateTimeZone);
        var zonedEndDateTime = new ZonedDateTime(periodEnd, dateTimeZone);
        if (zonedEndDateTime.LocalDateTime.Month > zonedStartDateTime.LocalDateTime.Month
            && zonedEndDateTime.LocalDateTime.Day > zonedStartDateTime.LocalDateTime.Day)
        {
            errors.Add(_invalidPeriodAcrossMonths);
            return;
        }

        if (zonedStartDateTime.LocalDateTime.Day != 1
            || zonedEndDateTime.LocalDateTime.Day != 1)
        {
            errors.Add(_invalidPeriodLength);
            return;
        }

        if (zonedEndDateTime.LocalDateTime.Month - zonedStartDateTime.LocalDateTime.Month != 1)
            errors.Add(_invalidPeriodAcrossMonths);
    }

    private void MustBeMidnight(Instant instant, string propertyName, ICollection<ValidationError> errors)
    {
        if (periodValidationHelper.IsMidnight(instant, out var zonedDateTime))
            return;

        errors.Add(zonedDateTime.IsDaylightSavingTime()
            ? _invalidSummerMidnightFormat.WithPropertyName(propertyName)
            : _invalidWinterMidnightFormat.WithPropertyName(propertyName));
    }
}
