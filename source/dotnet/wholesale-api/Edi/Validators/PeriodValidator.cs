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

namespace Energinet.DataHub.Wholesale.EDI.Validators;

public class PeriodValidator : AbstractValidator<PeriodCompound>
{
    private const string ErrorMessage = "forkert dato format, skal være YYYY-MM-DDT22:00:00Z eller YYYY-MM-DDT23:00:00Z / Wrong date format, must be YYYY-MM-DDT22:00:00Z or YYYY-MM-DDT23:00:00Z";
    private const string ErrorCode = "D66";
    private readonly DateTimeZone _dateTimeZone;

    public PeriodValidator(DateTimeZone dateTimeZone)
    {
        _dateTimeZone = dateTimeZone;

        RuleFor(x => x.StartValueAsInstant).Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(ErrorMessage).WithErrorCode(ErrorCode)
            .WithErrorCode(ErrorCode)
            .Must(BeMidnight)
            .WithMessage(ErrorMessage).WithErrorCode(ErrorCode)
            .WithErrorCode(ErrorCode);

        RuleFor(x => x.EndValueAsInstant).Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(ErrorMessage).WithErrorCode(ErrorCode)
            .WithErrorCode(ErrorCode)
            .Must(BeMidnight)
            .WithMessage(ErrorMessage).WithErrorCode(ErrorCode)
            .WithErrorCode(ErrorCode);
    }

    private bool BeMidnight(Instant? instant)
    {
        if (instant == null) return true;

        var zonedDateTime = new ZonedDateTime(instant.Value, _dateTimeZone);
        return zonedDateTime.TimeOfDay == LocalTime.Midnight;
    }
}
