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

using System.Linq.Expressions;
using FluentValidation;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Text;
using Period = Energinet.DataHub.Edi.Requests.Period;

namespace Energinet.DataHub.Wholesale.EDI.Validators;

public class PeriodValidator : AbstractValidator<PeriodValidator.Foo>
{
    private const string SummerTimeFormat = "YYYY-MM-DDT22:00:00Z";
    private const string WinterTimeFormat = "YYYY-MM-DDT23:00:00Z";

    private const string DanishTimeFormatErrorMessage = $"Forkert dato format, skal være {SummerTimeFormat} eller {WinterTimeFormat}";
    private const string EnglishTimeFormatErrorMessage = $"Wrong date format, must be {SummerTimeFormat} or {WinterTimeFormat}";
    private const string ErrorMessage = $"{DanishTimeFormatErrorMessage} / {EnglishTimeFormatErrorMessage}";

    private const string ErrorCode = "D66";
    private readonly DateTimeZone _dateTimeZone;

    public PeriodValidator(DateTimeZone dateTimeZone)
    {
        _dateTimeZone = dateTimeZone;
        RuleFor(x => x.StartValueAsInstant)
            .NotNull()
            .Must(IsMidnight)
            .WithMessage(ErrorMessage).WithErrorCode(ErrorCode)
            .Must(TimeIsInThePast)
            .WithMessage("{PropertyName} Date kan ikke være i fremtiden " +
                             "/ {PropertyName} Date can not be in the future")
            .WithErrorCode(ErrorCode);

        /*
        RuleFor(x => x.e).NotNull()
            .WithMessage($"{{PropertyName}} Date har {DanishTimeFormatErrorMessage} / {{PropertyName}} Date has {EnglishTimeFormatErrorMessage}")
            .WithErrorCode(WrongTimeFormatErrorCode);

        RuleFor(x => x.Start).Must(IsMidnight);
*/
        /*
        When(
            x => CanConvertToInstant(x.Start),
            () =>
                IsMidnightAndInThePast(x => ConvertToInstant(x.Start)));

        When(
            x => CanConvertToInstant(x.End),
            () =>
                IsMidnightAndInThePast(x => ConvertToInstant(x.Start)));

        When(
            x => CanConvertToInstant(x.Start) && CanConvertToInstant(x.End),
            () =>
                RuleFor(x => x)
                    .Must(StartIsBeforeEnd)
                    .WithMessage("Start Date has to be before End Date").WithErrorCode("D66"));
       */
        /*
        _dateTimeZone = dateTimeZone;
        RuleFor(x => x.Start).Must(CanConvertToInstant)
            .WithMessage($"{{PropertyName}} Date har {DanishTimeFormatErrorMessage} / {{PropertyName}} Date has {EnglishTimeFormatErrorMessage}")
            .WithErrorCode(WrongTimeFormatErrorCode);
        RuleFor(x => x.End).Must(CanConvertToInstant)
            .WithMessage($"{{PropertyName}} Date har {DanishTimeFormatErrorMessage} / {{PropertyName}} Date has {EnglishTimeFormatErrorMessage}")
            .WithErrorCode(WrongTimeFormatErrorCode);

        When(
            x => CanConvertToInstant(x.Start),
            () =>
                IsMidnightAndInThePast(x => ConvertToInstant(x.Start)));

        When(
            x => CanConvertToInstant(x.End),
            () =>
                IsMidnightAndInThePast(x => ConvertToInstant(x.Start)));

        When(
            x => CanConvertToInstant(x.Start) && CanConvertToInstant(x.End),
            () =>
            RuleFor(x => x)
                .Must(StartIsBeforeEnd)
                .WithMessage("Start Date has to be before End Date").WithErrorCode("D66"));
*/
        /*

        RuleFor(x => x.Start)
            .Cascade(CascadeMode.Stop)
            .Must(CanConvertToInstant)
            .WithMessage(WrongTimeFormatErrorMessage).WithErrorCode(WrongTimeFormatErrorCode)
            .Must(stringDate => ConvertToInstant(stringDate).InUtc().TimeOfDay == LocalTime.Midnight)
            .WithMessage(WrongTimeFormatErrorMessage).WithErrorCode(WrongTimeFormatErrorCode);

        When(x => string.IsNullOrWhiteSpace(x.End) == false, () =>
            RuleFor(x => x.End)
                .Cascade(CascadeMode.Stop)
                .Must(CanConvertToInstant)
                .WithMessage(WrongTimeFormatErrorMessage).WithErrorCode(WrongTimeFormatErrorCode)
                .Must(stringDate => ConvertToInstant(stringDate).InUtc().TimeOfDay == LocalTime.Midnight)
                .WithMessage(WrongTimeFormatErrorMessage).WithErrorCode(WrongTimeFormatErrorCode));

        RuleFor(x => x.Start)
        When(
            x => CanConvertToInstant(x.Start),
            () =>
                RuleFor(x => ConvertToInstant(x.Start))
                    .Must(instant => instant.InUtc().Hour == 0)
                    .WithMessage(WrongTimeFormatErrorMessage).WithErrorCode(WrongTimeFormatErrorCode));
 */

        // RuleForConvertToInstantAndTimeIsInThePast(x => x.Start, cascadeMode: CascadeMode.Stop);
        // RuleFor(x => x.Start)
        //     .Must(IsWinterOrSummerTime)
        //     .WithMessage(WrongTimeFormatErrorMessage).WithErrorCode(WrongTimeFormatErrorCode);
        //
        // When(x => string.IsNullOrWhiteSpace(x.End) == false, () =>
        // {
        //     RuleForConvertToInstantAndTimeIsInThePast(x => x.End, cascadeMode: CascadeMode.Stop);
        //     RuleFor(x => x.End)
        //         .Must(IsWinterOrSummerTime)
        //         .WithMessage(WrongTimeFormatErrorMessage).WithErrorCode(WrongTimeFormatErrorCode);
        // });
        //
        // When(
        //     x =>
        //     string.IsNullOrWhiteSpace(x.End) == false
        //     && CanConvertToInstant(x.Start)
        //     && CanConvertToInstant(x.End),
        //     () =>
        //     RuleFor(x => x)
        //         .Must(StartIsBeforeEnd)
        //         .WithMessage("Start Date has to be before End Date").WithErrorCode("D66"));
    }

    // private void IsMidnightAndInThePast(Expression<Func<Period, Instant>> extractFunc)
    // {
    //     RuleFor(extractFunc)
    //         .Must(IsMidnight)
    //         .WithMessage($"{{PropertyName}} Date har {DanishTimeFormatErrorMessage} / {{PropertyName}} Date has {EnglishTimeFormatErrorMessage}")
    //         .WithErrorCode(WrongTimeFormatErrorCode)
    //         .Must(TimeIsInThePast)
    //         .WithMessage("{PropertyName} Date kan ikke være i fremtiden " +
    //                      "/ {PropertyName} Date can not be in the future")
    //         .WithErrorCode("D66");
    // }
    private bool IsMidnight(Instant? instant)
    {
        if (instant == null) return true;
        var zonedDateTime = new ZonedDateTime(instant.Value, _dateTimeZone);

        return zonedDateTime.TimeOfDay == LocalTime.Midnight;
    }

    private static bool CanConvertToInstant(string stringDate)
    {
        return InstantPattern.General.Parse(stringDate).Success;
    }

    private static Instant ConvertToInstant(string stringDate)
    {
        return InstantPattern.General.Parse(stringDate).Value;
    }

    private static bool StartIsBeforeEnd(Period period)
    {
        var (start, end) = (period.Start, period.End);
        return CanConvertToInstant(start) && CanConvertToInstant(end) && ConvertToInstant(start) < ConvertToInstant(end);
    }

    private static bool TimeIsInThePast(Instant? date)
    {
        return date < DateTime.UtcNow.ToInstant();
    }

    public class Foo
    {
        public Foo(string startValue, string endValue)
        {
            SomeValue = startValue;
            EndValue = endValue;
        }

        public string SomeValue { get; set; }

        public Instant? StartValueAsInstant => InstantPattern.General.Parse(SomeValue).Success ? InstantPattern.General.Parse(SomeValue).Value : null;

        public string EndValue { get; set; }

        public Instant? EndValueAsInstant => InstantPattern.General.Parse(SomeValue).Success ? InstantPattern.General.Parse(SomeValue).Value : null;
    }
}
