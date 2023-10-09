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
using Google.Protobuf;
using NodaTime;
using NodaTime.Text;

namespace Energinet.DataHub.Wholesale.EDI.Validators.ValidationRules.AggregatedTimeSerie;

public class PeriodValidationRule : IValidationRule<IMessage>
{
    private const string DanishErrorMessage =
        "Forkert dato format for {PropertyName}, skal være YYYY-MM-DDT22:00:00Z eller YYYY-MM-DDT23:00:00Z";

    private const string EnglishErrorMessage =
        "Wrong date format for {PropertyName}, must be YYYY-MM-DDT22:00:00Z or YYYY-MM-DDT23:00:00Z";

    private const string ErrorMessage = $"{DanishErrorMessage} / {EnglishErrorMessage}";
    private const string ErrorCode = "D66";

    private readonly DateTimeZone _dateTimeZone;
    private IList<ValidationError> _errors = new List<ValidationError>();

    public PeriodValidationRule(DateTimeZone dateTimeZone)
    {
        _dateTimeZone = dateTimeZone;
    }

    public void Validate(IMessage entity, out IList<ValidationError> errors)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        var period = (entity as AggregatedTimeSeriesRequest)?.Period;
        if (period == null) throw new ArgumentNullException(nameof(period));

        ValidateDateFormat(period.Start, "Start date", out var startInstant);
        ValidateDateFormat(period.End, "End date", out var endInstant);

        if (!_errors.Any())
        {
            MustBeMidnight(startInstant);
            MustBeMidnight(endInstant);
        }

        errors = _errors;
    }

    public bool Support(Type type)
    {
        return type == typeof(AggregatedTimeSeriesRequest);
    }

    private void ValidateDateFormat(string dateTimeString, string propertyName, out Instant instant)
    {
        instant = default;

        var parseResult = InstantPattern.General.Parse(dateTimeString);
        if (!parseResult.Success)
        {
            _errors.Add(new ValidationError(ErrorMessage.Replace("{PropertyName}", propertyName), ErrorCode));
        }
        else
        {
            instant = parseResult.Value;
        }
    }

    private void MustBeMidnight(Instant instant)
    {
        var zonedDateTime = new ZonedDateTime(instant, _dateTimeZone);
        if (zonedDateTime.TimeOfDay != LocalTime.Midnight)
            _errors.Add(new ValidationError(ErrorMessage, ErrorCode));
    }
}
