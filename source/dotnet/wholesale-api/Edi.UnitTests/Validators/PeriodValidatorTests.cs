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

using Energinet.DataHub.Wholesale.EDI.UnitTests.Builders;
using Energinet.DataHub.Wholesale.EDI.Validation;
using Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSerie.Rules;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class PeriodValidatorTests
{
    private readonly PeriodValidationRule _sut = new(DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!, SystemClock.Instance);

    [Fact]
    public void Validate_WhenValidRequest_ReturnsExpectedNoValidationErrors()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WhenEndDateIsUnspecified_ReturnsExpectedValidationError()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithEndDate(string.Empty)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().ContainSingle();
        errors.First().ErrorCode.Should().Be(ValidationError.MissingStartOrAndEndDate.ErrorCode);
    }

    [Fact]
    public void Validate_WhenWrongStartHour_ReturnsExpectedValidationError()
    {
        // Arrange
        var now = SystemClock.Instance.GetCurrentInstant();
        var notWinterTimeMidnight = Instant.FromUtc(now.InUtc().Year, 1, 1, 22, 0, 0).ToString();
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate(notWinterTimeMidnight)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().ContainSingle();
        errors.First().ErrorCode.Should().Be(ValidationError.InvalidWinterMidnightFormat.ErrorCode);
        errors.Should().Contain(error => error.Message.Contains("23:00:00Z"));
    }

    [Fact]
    public void Validate_WhenStartIsUnspecified_ReturnsExpectedValidationError()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate(string.Empty)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().ContainSingle();
        errors.First().ErrorCode.Should().Be(ValidationError.MissingStartOrAndEndDate.ErrorCode);
    }

    [Fact]
    public void Validate_WhenStartAndEndAreInvalid_ReturnsExpectedValidationErrors()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate("string.Empty")
            .WithEndDate("string.Empty")
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Count.Should().Be(2);
        errors.Should().Contain(error => error.Message.Contains("Start date")
                                         && error.ErrorCode.Equals(ValidationError.InvalidDateFormat.ErrorCode));
        errors.Should().Contain(error => error.Message.Contains("End date")
                                         && error.ErrorCode.Equals(ValidationError.InvalidDateFormat.ErrorCode));
    }

    [Fact]
    public void Validate_WhenPeriodSizeIsGreaterThenAllowed_ReturnsExpectedValidationError()
    {
        // Arrange
        var now = SystemClock.Instance.GetCurrentInstant();
        var winterTimeMidnight = Instant.FromUtc(now.InUtc().Year, 1, 1, 23, 0, 0);
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate(winterTimeMidnight.ToString())
            .WithEndDate(winterTimeMidnight.Plus(Duration.FromDays(32)).ToString())
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().ContainSingle();
        errors.First().ErrorCode.Should().Be(ValidationError.PeriodIsGreaterThenAllowedPeriodSize.ErrorCode);
    }

    [Fact]
    public void Validate_WhenPeriodIsOlderThenAllowed_ReturnsExpectedValidationError()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate(Instant.FromUtc(2018, 1, 1, 23, 0, 0).ToString())
            .WithEndDate(Instant.FromUtc(2018, 1, 1, 23, 0, 0).ToString())
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().ContainSingle();
        errors.First().ErrorCode.Should().Be(ValidationError.StartDateMustBeLessThen3Years.ErrorCode);
    }

    [Fact]
    public void Validate_WhenPeriodOverlapSummerDaylightSavingTime_ReturnsExpectedNoValidationErrors()
    {
        // Arrange
        var now = SystemClock.Instance.GetCurrentInstant();
        var winterTime = Instant.FromUtc(now.InUtc().Year, 2, 26, 23, 0, 0).ToString();
        var summerTime = Instant.FromUtc(now.InUtc().Year, 3, 26, 22, 0, 0).ToString();
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate(winterTime)
            .WithEndDate(summerTime)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WhenPeriodOverlapWinterDaylightSavingTime_ReturnsExpectedNoValidationErrors()
    {
        // Arrange
        var now = SystemClock.Instance.GetCurrentInstant();
        var summerTime = Instant.FromUtc(now.InUtc().Year, 9, 29, 22, 0, 0).ToString();
        var winterTime = Instant.FromUtc(now.InUtc().Year, 10, 29, 23, 0, 0).ToString();
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate(summerTime)
            .WithEndDate(winterTime)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().BeEmpty();
    }
}
