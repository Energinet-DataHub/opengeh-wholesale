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
using NodaTime.Extensions;
using NodaTime.Text;
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class PeriodValidatorTests
{
    private readonly PeriodValidator _sut = new(DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!);

    [Fact]
    public void Validate_Period_SuccessValidation()
    {
        // Arrange
        var period = new PeriodCompound(Instant.FromUtc(2022, 1, 1, 23, 0, 0).ToString(), Instant.FromUtc(2022, 1, 2, 23, 0, 0).ToString());

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.True(periodStatus.IsValid);
    }

    [Fact]
    public void Validate_EndDateIsUnspecified_FailsValidation()
    {
        // Arrange
        var period = new PeriodCompound(Instant.FromUtc(2022, 1, 1, 23, 0, 0).ToString(), string.Empty);

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.False(periodStatus.IsValid);
    }

    [Fact]
    public void Validate_WrongStartHour_FailsValidation()
    {
        // Arrange
        var period = new PeriodCompound(Instant.FromUtc(2022, 1, 1, 22, 0, 0).ToString(), Instant.FromUtc(2022, 1, 2, 23, 0, 0).ToString());

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.False(periodStatus.IsValid);
    }

    [Fact]
    public void Validate_StartIsUnspecified_FailsValidation()
    {
        // Arrange
        var period = new PeriodCompound(string.Empty, Instant.FromUtc(2022, 1, 2, 23, 0, 0).ToString());

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.False(periodStatus.IsValid);
    }

    [Fact]
    public void Validate_Fails_CorrectErrorCode()
    {
        // Arrange
        var period = new PeriodCompound(Instant.FromUtc(2022, 1, 1, 23, 0, 0).ToString(), string.Empty);

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.Equal("D66", periodStatus.Errors.First().ErrorCode);
    }

    [Fact]
    public void Validate_Fails_CorrectNumberOfMessages()
    {
        // Arrange
        var period = new PeriodCompound(Instant.FromUtc(2022, 1, 1, 23, 0, 0).ToString(), string.Empty);

        // Act
        var periodStatus = _sut.Validate(period);

        // Assert
        Assert.True(periodStatus.Errors.Count == 1);
    }
}
