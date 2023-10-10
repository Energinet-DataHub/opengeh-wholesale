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
using Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSerie.Rules;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class PeriodValidatorTests
{
    private readonly PeriodValidationRule _sut = new(DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!);
    private readonly Instant _winterTimeMidnight = Instant.FromUtc(2022, 1, 1, 23, 0, 0);

    [Fact]
    public void Validate_Period_SuccessValidation()
    {
        // Arrange
        var message = new AggregatedTimeSeriesRequest();
        message.Period = new Edi.Requests.Period();
        message.Period.Start = _winterTimeMidnight.ToString();
        message.Period.End = _winterTimeMidnight.ToString();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        Assert.False(errors.Any());
    }

    [Fact]
    public void Validate_EndDateIsUnspecified_FailsValidation()
    {
        // Arrange
        var message = new AggregatedTimeSeriesRequest();
        message.Period = new Edi.Requests.Period();
        message.Period.Start = _winterTimeMidnight.ToString();
        message.Period.End = string.Empty;

        // Act
        var errors = _sut.Validate(message);

        // Assert
        Assert.True(errors.Any());
    }

    [Fact]
    public void Validate_WrongStartHour_FailsValidation()
    {
        // Arrange
        var notWinterTimeMidnight = Instant.FromUtc(2022, 1, 1, 22, 0, 0).ToString();
        var message = new AggregatedTimeSeriesRequest();
        message.Period = new Edi.Requests.Period();
        message.Period.Start = notWinterTimeMidnight;
        message.Period.End = _winterTimeMidnight.ToString();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        Assert.True(errors.Any());
    }

    [Fact]
    public void Validate_StartIsUnspecified_FailsValidation()
    {
        // Arrange
        var message = new AggregatedTimeSeriesRequest();
        message.Period = new Edi.Requests.Period();
        message.Period.Start = string.Empty;
        message.Period.End = _winterTimeMidnight.ToString();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        Assert.True(errors.Any());
    }

    [Fact]
    public void Validate_Fails_CorrectNumberOfMessagesAndErrorCode()
    {
        // Arrange
        var message = new AggregatedTimeSeriesRequest();
        message.Period = new Edi.Requests.Period();
        message.Period.Start = _winterTimeMidnight.ToString();
        message.Period.End = string.Empty;

        // Act
        var errors = _sut.Validate(message);

        // Assert
        Assert.Equal("D66", errors.First().ErrorCode);
        Assert.True(errors.Count == 1);
    }

    [Fact]
    public void Validate_StartAndEndAreInvalid_TwoErrorsWithMessages()
    {
        // Arrange
        var message = new AggregatedTimeSeriesRequest();
        message.Period = new Edi.Requests.Period();
        message.Period.Start = string.Empty;
        message.Period.End = string.Empty;

        // Act
        var errors = _sut.Validate(message);

        // Assert
        Assert.True(errors.Count == 2);
        Assert.Contains(
            errors.Where(error => error.Message.Contains("Start date")),
            error => error.ErrorCode.Equals("D66"));
        Assert.Contains(
            errors.Where(error => error.Message.Contains("End date")),
            error => error.ErrorCode.Equals("D66"));
    }
}
