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

using Energinet.DataHub.Wholesale.Edi.UnitTests.Builders;
using Energinet.DataHub.Wholesale.Edi.Validation;
using Energinet.DataHub.Wholesale.Edi.Validation.WholesaleServicesRequest.Rules;
using FluentAssertions;
using FluentAssertions.Execution;
using NodaTime;
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Validators.WholesaleServicesRequest;

public class PeriodValidationRuleTests
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

    private readonly PeriodValidationRule _sut;

    private Instant _now;

    public PeriodValidationRuleTests()
    {
        _now = Instant.FromUtc(2024, 5, 31, 22, 0, 0);
        _sut = new PeriodValidationRule(
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!,
            new MockClock(() => _now));
    }

    [Fact]
    public async Task Validate_WhenPeriodStartIsNonsense_ReturnsExpectedValidationErrors()
    {
        // Arrange
        var message = new WholesaleServicesRequestBuilder()
            .WithPeriodStart("string.Empty")
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().ContainSingle();
        errors.Should().Contain(error =>
            error.Message.Contains(_invalidDateFormat.WithPropertyName("Period Start").Message)
            && error.ErrorCode.Equals(_invalidDateFormat.ErrorCode));
    }

    [Fact]
    public async Task Validate_WhenPeriodStartIsInAnInvalidFormat_ReturnsExpectedValidationErrors()
    {
        // Arrange
        var message1 = new WholesaleServicesRequestBuilder()
            .WithPeriodStart("2024-08-17")
            .Build();

        var message2 = new WholesaleServicesRequestBuilder()
            .WithPeriodStart("2024-08-17T23:00:00")
            .Build();

        // Act
        var errors1 = await _sut.ValidateAsync(message1);
        var errors2 = await _sut.ValidateAsync(message2);

        // Assert
        errors1.Should().ContainSingle();
        errors1.Should().Contain(error =>
            error.Message.Contains(_invalidDateFormat.WithPropertyName("Period Start").Message)
            && error.ErrorCode.Equals(_invalidDateFormat.ErrorCode));

        errors2.Should().ContainSingle();
        errors2.Should().Contain(error =>
            error.Message.Contains(_invalidDateFormat.WithPropertyName("Period Start").Message)
            && error.ErrorCode.Equals(_invalidDateFormat.ErrorCode));
    }

    [Fact]
    public async Task Validate_WhenPeriodStartIsOlderThanAllowed_ReturnsExpectedValidationError()
    {
        // Arrange
        var message = new WholesaleServicesRequestBuilder()
            .WithPeriodStart(_now.ToDateTimeOffset().AddYears(-5).ToInstant().ToString())
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().ContainSingle();
        var error = errors.First();
        error.ErrorCode.Should().Be(_startDateMustBeLessThanOrEqualTo3YearsAnd2Months.ErrorCode);
        error.Message.Should().Be(_startDateMustBeLessThanOrEqualTo3YearsAnd2Months.Message);
    }

    [Fact]
    public async Task Validate_WhenPeriodStartIsExactly3YearsAnd2MonthsOld_ReturnsNoValidationError()
    {
        // Arrange
        var message = new WholesaleServicesRequestBuilder()
            .WithPeriodStart(_now.ToDateTimeOffset().AddYears(-3).AddMonths(-2).ToInstant().ToString())
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WhenPeriodStartIsExactly3Years2MonthsAnd1HourOldDueToDaylightSavingTime_ReturnsNoValidationError()
    {
        // Arrange
        var periodStartDate = new LocalDateTime(2021, 10, 1, 0, 0, 0)
            .InZoneStrictly(DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!)
            .ToInstant();

        _now = new LocalDateTime(2024, 12, 1, 0, 0, 0)
            .InZoneStrictly(DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!)
            .ToInstant();

        var message = new WholesaleServicesRequestBuilder()
            .WithPeriodStart(periodStartDate.ToString())
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().BeEmpty();
        var duration = _now - periodStartDate;
        duration.Days.Should().Be(1157);
        duration.Hours.Should().Be(1);
    }

    [Fact]
    public async Task Validate_WhenPeriodStartIsExactly3Years2MonthsMinus1HourOldDueToDaylightSavingTime_ReturnsNoValidationError()
    {
        // Arrange
        var periodStartDate = new LocalDateTime(2021, 3, 1, 0, 0, 0)
            .InZoneStrictly(DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!)
            .ToInstant();

        _now = new LocalDateTime(2024, 5, 1, 0, 0, 0)
            .InZoneStrictly(DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!)
            .ToInstant();

        var message = new WholesaleServicesRequestBuilder()
            .WithPeriodStart(periodStartDate.ToString())
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().BeEmpty();
        var duration = _now - periodStartDate;
        duration.Days.Should().Be(1156);
        duration.Hours.Should().Be(23);
    }

    [Fact]
    public async Task Validate_WhenPeriodStartIs1DayTooOld_ReturnsExpectedValidationError()
    {
        // Arrange
        var message = new WholesaleServicesRequestBuilder()
            // 1 day too old is the smallest possible period it can be too old
            .WithPeriodStart(_now.ToDateTimeOffset().AddYears(-3).AddMonths(-2).AddDays(-1).ToInstant().ToString())
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        using var assertionScope = new AssertionScope();
        errors.Should().ContainSingle();
        var error = errors.First();
        error.ErrorCode.Should().Be(_startDateMustBeLessThanOrEqualTo3YearsAnd2Months.ErrorCode);
        error.Message.Should().Be(_startDateMustBeLessThanOrEqualTo3YearsAnd2Months.Message);
    }

    [Fact]
    public async Task Validate_WhenPeriodOverlapSummerDaylightSavingTime_ReturnsNoValidationErrors()
    {
        // Arrange
        var winterTime = Instant.FromUtc(_now.ToDateTimeOffset().Year, 3, 25, 23, 0, 0).ToString();
        var summerTime = Instant.FromUtc(_now.ToDateTimeOffset().Year, 4, 25, 22, 0, 0).ToString();

        var message = new WholesaleServicesRequestBuilder()
            .WithPeriodStart(winterTime)
            .WithPeriodEnd(summerTime)
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WhenPeriodOverlapWinterDaylightSavingTime_ReturnsNoValidationErrors()
    {
        // Arrange
        var summerTime = Instant.FromUtc(_now.ToDateTimeOffset().Year, 9, 29, 22, 0, 0).ToString();
        var winterTime = Instant.FromUtc(_now.ToDateTimeOffset().Year, 10, 29, 23, 0, 0).ToString();

        var message = new WholesaleServicesRequestBuilder()
            .WithPeriodStart(summerTime)
            .WithPeriodEnd(winterTime)
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WhenStartHourIsWrong_ReturnsExpectedValidationError()
    {
        // Arrange
        var notWinterTimeMidnight = Instant.FromUtc(_now.ToDateTimeOffset().Year, 1, 1, 22, 0, 0).ToString();

        var message = new WholesaleServicesRequestBuilder()
            .WithPeriodStart(notWinterTimeMidnight)
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().ContainSingle();
        var error = errors.First();
        error.ErrorCode.Should().Be(_invalidWinterMidnightFormat.ErrorCode);
        error.Message.Should().Be(_invalidWinterMidnightFormat.WithPropertyName("Period Start").Message);
    }

    [Fact]
    public async Task Validate_WhenEndHourIsWrong_ReturnsExpectedValidationError()
    {
        // Arrange
        var notSummerTimeMidnight = Instant.FromUtc(_now.ToDateTimeOffset().Year, 7, 1, 23, 0, 0).ToString();

        var message = new WholesaleServicesRequestBuilder()
            .WithPeriodEnd(notSummerTimeMidnight)
            .WithPeriodStart(Instant.FromUtc(_now.ToDateTimeOffset().Year, 7, 2, 22, 0, 0).ToString())
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().ContainSingle();
        var error = errors.First();
        error.ErrorCode.Should().Be(_invalidSummerMidnightFormat.ErrorCode);
        error.Message.Should().Be(_invalidSummerMidnightFormat.WithPropertyName("Period End").Message);
    }

    private sealed class MockClock(Func<Instant> getInstant) : IClock
    {
        public Instant GetCurrentInstant() => getInstant.Invoke();
    }
}
