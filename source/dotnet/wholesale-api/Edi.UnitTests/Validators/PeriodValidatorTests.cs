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

using Energinet.DataHub.Wholesale.Edi.Validators;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class PeriodValidatorTests
{
    private readonly PeriodValidator _sut = new();

    [Fact]
    public void Validate_Period_SuccessValidation()
    {
        // Arrange
        var period = new Energinet.DataHub.Edi.Requests.Period()
        {
            Start = Instant.FromUtc(2022, 1, 1, 22, 0, 0).ToString(),
            End = Instant.FromUtc(2022, 1, 2, 23, 0, 0).ToString(),
        };

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.True(periodStatus.IsValid);
    }

    [Fact]
    public void Validate_EndDateIsUnspecified_SuccessfulValidation()
    {
        // Arrange
        var period = new Energinet.DataHub.Edi.Requests.Period()
        {
            Start = Instant.FromUtc(2022, 1, 1, 22, 0, 0).ToString(),
            End = string.Empty,
        };

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.True(periodStatus.IsValid);
    }

    [Fact]
    public void Validate_PeriodIsInTheFuture_FailsValidation()
    {
        // Arrange
        var tomorrow = DateTime.UtcNow.AddDays(1);
        var tomorrowAt22 = LocalDate.FromDateTime(tomorrow) + new LocalTime(22, 0, 0);

        var period = new Energinet.DataHub.Edi.Requests.Period()
        {
            Start = tomorrowAt22.InUtc().ToInstant().ToString(),
            End = tomorrowAt22.PlusDays(1).InUtc().ToInstant().ToString(),
        };

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.False(periodStatus.IsValid);
        Assert.Single(periodStatus.Errors);
        Assert.Equal("D66", periodStatus.Errors.First().ErrorCode);
    }

    [Fact]
    public void Validate_WrongStartHour_FailsValidation()
    {
        // Arrange
        var period = new Energinet.DataHub.Edi.Requests.Period()
        {
            Start = Instant.FromUtc(2022, 1, 1, 21, 0, 0).ToString(),
            End = Instant.FromUtc(2022, 1, 2, 22, 0, 0).ToString(),
        };

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.False(periodStatus.IsValid);
        Assert.Single(periodStatus.Errors);
        Assert.Equal("D66", periodStatus.Errors.First().ErrorCode);
    }

    [Fact]
    public void Validate_StartIsEqualToEnd_FailsValidation()
    {
        // Arrange
        var period = new Energinet.DataHub.Edi.Requests.Period()
        {
            Start = Instant.FromUtc(2022, 1, 1, 22, 0, 0).ToString(),
            End = Instant.FromUtc(2022, 1, 1, 22, 0, 0).ToString(),
        };

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.False(periodStatus.IsValid);
        Assert.Single(periodStatus.Errors);
        Assert.Equal("D66", periodStatus.Errors.First().ErrorCode);
    }

    [Fact]
    public void Validate_StartIsBadFormat_FailsValidation()
    {
        // Arrange
        var period = new Energinet.DataHub.Edi.Requests.Period()
            {
                Start = "a023-07-27T22:00:00Z",
                End = Instant.FromUtc(2022, 1, 1, 22, 0, 0).ToString(),
            };

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.False(periodStatus.IsValid);
        Assert.Single(periodStatus.Errors);
        Assert.Equal("D66", periodStatus.Errors.First().ErrorCode);
    }

    [Fact]
    public void Validate_EndIsBadFormat_FailsValidation()
    {
        // Arrange
        var period = new Energinet.DataHub.Edi.Requests.Period()
            {
                Start = Instant.FromUtc(2022, 1, 1, 22, 0, 0).ToString(),
                End = "a023-07-27T22:00:00Z",
            };

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.False(periodStatus.IsValid);
        Assert.Single(periodStatus.Errors);
        Assert.Equal("D66", periodStatus.Errors.First().ErrorCode);
    }
}
