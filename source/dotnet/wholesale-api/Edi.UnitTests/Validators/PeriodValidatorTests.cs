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
    private readonly Instant _winterTimeMidnight = Instant.FromUtc(2022, 1, 1, 23, 0, 0);

    [Fact]
    public void Validate_WhenValidRequest_ReturnsExceptedNoValidationErrors()
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
    public void Validate_WhenEndDateIsUnspecified_ReturnsExceptedValidationError()
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
    public void Validate_WhenWrongStartHour_ReturnsExceptedValidationError()
    {
        // Arrange
        var notWinterTimeMidnight = Instant.FromUtc(2022, 1, 1, 22, 0, 0).ToString();
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate(notWinterTimeMidnight)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().ContainSingle();
        errors.First().ErrorCode.Should().Be(ValidationError.InvalidDateFormat.ErrorCode);
    }

    [Fact]
    public void Validate_WhenStartIsUnspecified_ReturnsExceptedValidationError()
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
    public void Validate_WhenStartAndEndAreInvalid_ReturnsExceptedValidationErrors()
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
    public void Validate_WhenPeriodSizeIsGreaterThenAllowed_ReturnsExceptedValidationError()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate(_winterTimeMidnight.ToString())
            .WithEndDate(_winterTimeMidnight.Plus(Duration.FromDays(32)).ToString())
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().ContainSingle();
        errors.First().ErrorCode.Should().Be(ValidationError.PeriodIsGreaterThenAllowedPeriodSize.ErrorCode);
    }

    [Fact]
    public void Validate_WhenPeriodIsOlderThenAllowed_ReturnsExceptedValidationError()
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
}
