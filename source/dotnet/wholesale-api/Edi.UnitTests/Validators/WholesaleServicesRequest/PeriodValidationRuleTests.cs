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
using Microsoft.EntityFrameworkCore.SqlServer.NodaTime.Extensions;
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

    private static readonly MockClock _mockClock = new(Instant.FromUtc(2024, 6, 1, 23, 0, 0));

    private readonly PeriodValidationRule _sut = new(
        DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!,
        _mockClock);

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
            .WithPeriodStart(_mockClock.GetCurrentInstant().ToDateTimeOffset().AddYears(-5).ToInstant().ToString())
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
            .WithPeriodStart(_mockClock.GetCurrentInstant().ToDateTimeOffset().AddYears(-3).AddMonths(-2).ToInstant().ToString())
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WhenPeriodStartIs1DayTooOld_ReturnsNoValidationError()
    {
        // Arrange
        var message = new WholesaleServicesRequestBuilder()
            // 1 day too old is the smallest possible period it can be too old
            .WithPeriodStart(_mockClock.GetCurrentInstant().ToDateTimeOffset().AddYears(-3).AddMonths(-2).AddDays(-1).ToInstant().ToString())
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

    private sealed class MockClock(Instant instant) : IClock
    {
        public Instant GetCurrentInstant() => instant;
    }
}
