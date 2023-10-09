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

using Energinet.DataHub.Wholesale.EDI.Validators;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class PeriodValidatorTests
{
    private readonly PeriodValidator _sut = new(DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!);

    [Fact]
    public void Validate_Period_SuccessValidation()
    {
        // Arrange
        var winterTimeMidnight = Instant.FromUtc(2022, 1, 1, 23, 0, 0);
        var period = new PeriodCompound(
            winterTimeMidnight.ToString(),
            winterTimeMidnight.Plus(Duration.FromDays(1))
                .ToString());

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.True(periodStatus.IsValid);
    }

    [Fact]
    public void Validate_EndDateIsUnspecified_FailsValidation()
    {
        // Arrange
        var winterTimeMidnight = Instant.FromUtc(2022, 1, 1, 23, 0, 0);
        var endDataIsUnspecified = string.Empty;
        var period = new PeriodCompound(
            winterTimeMidnight.ToString(),
            endDataIsUnspecified);

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.False(periodStatus.IsValid);
    }

    [Fact]
    public void Validate_WrongStartHour_FailsValidation()
    {
        // Arrange
        var winterTimeMidnight = Instant.FromUtc(2022, 1, 1, 23, 0, 0);
        var winterTimeNotMidnight = Instant.FromUtc(2022, 1, 1, 22, 0, 0);
        var period = new PeriodCompound(
            winterTimeNotMidnight.ToString(),
            winterTimeMidnight.ToString());

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.False(periodStatus.IsValid);
    }

    [Fact]
    public void Validate_StartIsUnspecified_FailsValidation()
    {
        // Arrange
        var startDataIsUnspecified = string.Empty;
        var winterTimeMidnight = Instant.FromUtc(2022, 1, 1, 23, 0, 0);
        var period = new PeriodCompound(
            startDataIsUnspecified,
            winterTimeMidnight.ToString());

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.False(periodStatus.IsValid);
    }

    [Fact]
    public void Validate_Fails_CorrectErrorCode()
    {
        // Arrange
        var endDataIsUnspecified = string.Empty;
        var winterTimeMidnight = Instant.FromUtc(2022, 1, 1, 23, 0, 0);
        var period = new PeriodCompound(
            winterTimeMidnight.ToString(),
            endDataIsUnspecified);

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.Equal("D66", periodStatus.Errors.First().ErrorCode);
    }

    [Fact]
    public void Validate_Fails_CorrectNumberOfMessages()
    {
        // Arrange
        var endDataIsUnspecified = string.Empty;
        var winterTimeMidnight = Instant.FromUtc(2022, 1, 1, 23, 0, 0);
        var period = new PeriodCompound(
            winterTimeMidnight.ToString(),
            endDataIsUnspecified);

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.True(periodStatus.Errors.Count == 1);
    }

    [Fact]
    public void Validate_StartAndEndAreInvalid_TwoErrorsWithMessages()
    {
        // Arrange
        var invalidDate = string.Empty;
        var period = new PeriodCompound(invalidDate, invalidDate);

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.True(periodStatus.Errors.Count == 2);
        Assert.Contains(
            periodStatus.Errors.Where(error => error.ErrorMessage.Contains("Start date")),
            error => error.ErrorCode.Equals("D66"));
        Assert.Contains(
            periodStatus.Errors.Where(error => error.ErrorMessage.Contains("End date")),
            error => error.ErrorCode.Equals("D66"));
    }
}
