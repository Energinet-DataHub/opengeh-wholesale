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

using FluentValidation;
using NodaTime;
using NodaTime.Text;
using Period = Energinet.DataHub.Edi.Requests.Period;

namespace Energinet.DataHub.Wholesale.Edi.Validators;

public class PeriodValidator : AbstractValidator<Period>
{
    private const string WrongTimeFormatErrorMessage =
        "Forkert dato format, skal være T22:00:00Z eller T23:00:00Z / Wrong date format, must be T22:00:00Z or T23:00:00Z";

    private const string WrongTimeFormatErrorCode = "D66";

    private const string SummerTimeFormat = "T22:00:00Z";
    private const string WinterTimeFormat = "T23:00:00Z";

    public PeriodValidator()
    {
        RuleFor(x => x.Start)
            .Cascade(CascadeMode.Stop)
            .Must(CanConvertToInstant)
            .WithMessage(WrongTimeFormatErrorMessage).WithErrorCode(WrongTimeFormatErrorCode)
            .Must(ValidHourFormat)
            .WithMessage(WrongTimeFormatErrorMessage).WithErrorCode(WrongTimeFormatErrorCode)
            .Must(TimeIsInThePast)
            .WithMessage("Time has to be in the past").WithErrorCode("D66");
        When(x => string.IsNullOrWhiteSpace(x.End) == false, () =>
            RuleFor(x => x.End)
            .Must(CanConvertToInstant)
            .WithMessage(WrongTimeFormatErrorMessage).WithErrorCode(WrongTimeFormatErrorCode)
            .Must(ValidHourFormat)
            .WithMessage(WrongTimeFormatErrorMessage).WithErrorCode(WrongTimeFormatErrorCode));

        When(x => string.IsNullOrWhiteSpace(x.End) == false, () =>
            RuleFor(x => x)
                .Must(StartIsBeforeEnd)
                .WithMessage("Start time has to be before end time").WithErrorCode("D66"));
    }

    private static bool CanConvertToInstant(string stringDate)
    {
        return InstantPattern.General.Parse(stringDate).Success;
    }

    private static Instant ConvertToInstant(string stringDate)
    {
        return InstantPattern.General.Parse(stringDate).Value;
    }

    private static bool ValidHourFormat(string stringData)
    {
        return stringData.Contains(SummerTimeFormat) || stringData.Contains(WinterTimeFormat);
    }

    private static bool StartIsBeforeEnd(Period period)
    {
        var (start, end) = (period.Start, period.End);
        return CanConvertToInstant(start)
               && CanConvertToInstant(end)
               && ConvertToInstant(start) < ConvertToInstant(end);
    }

    private static bool TimeIsInThePast(string stringDate)
    {
        return InstantPattern.General.Parse(stringDate).Value < Instant.FromDateTimeUtc(DateTime.UtcNow);
    }
}
