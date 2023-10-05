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

public class AggregatedTimeSeriesRequestValidatorTests
{
    private readonly AggregatedTimeSeriesRequestValidator _sut = new();

    [Fact]
    public void Validate_AggregatedTimeSeriesRequest_SuccessValidation()
    {
        // Arrange
        var request = new Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest()
        {
            Period = new Energinet.DataHub.Edi.Requests.Period()
            {
                Start = Instant.FromUtc(2022, 1, 1, 22, 0, 0).ToString(),
                End = Instant.FromUtc(2022, 1, 2, 23, 0, 0).ToString(),
            },
            TimeSeriesType = Energinet.DataHub.Edi.Requests.TimeSeriesType.Production,
        };

        // Act
        var validationStatus = _sut.Validate(request);

        // Assert
        Assert.True(validationStatus.IsValid);
    }

    [Fact]
    public void Validate_AggregatedTimeSeriesRequestWithEndIsEmpty_SuccessValidation()
    {
        // Arrange
        var request = new Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest()
        {
            Period = new Energinet.DataHub.Edi.Requests.Period()
            {
                Start = Instant.FromUtc(2022, 1, 1, 22, 0, 0).ToString(),
                End = string.Empty,
            },
            TimeSeriesType = Energinet.DataHub.Edi.Requests.TimeSeriesType.Production,
        };

        // Act
        var validationStatus = _sut.Validate(request);

        // Assert
        Assert.True(validationStatus.IsValid);
    }

    [Fact]
    public void Validate_AggregatedTimeSeriesRequest_FailsDueToWrongHourFormat()
    {
        // Arrange
        var request = new Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest()
        {
            Period = new Energinet.DataHub.Edi.Requests.Period()
            {
                Start = Instant.FromUtc(2022, 1, 1, 21, 0, 0).ToString(),
                End = Instant.FromUtc(2022, 1, 2, 22, 0, 0).ToString(),
            },
            TimeSeriesType = Energinet.DataHub.Edi.Requests.TimeSeriesType.Production,
        };

        // Act
        var validationStatus = _sut.Validate(request);

        // Assert
        Assert.False(validationStatus.IsValid);
        Assert.Equal("D66", validationStatus.Errors.First().ErrorCode);
    }

    [Fact]
    public void Validate_AggregatedTimeSeriesRequest_FailsOnStartBeforeEndCheck()
    {
        // Arrange
        var request = new Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest()
            {
                Period = new Energinet.DataHub.Edi.Requests.Period()
                {
                    Start = Instant.FromUtc(2022, 1, 1, 22, 0, 0).ToString(),
                    End = Instant.FromUtc(2022, 1, 1, 22, 0, 0).ToString(),
                },
                TimeSeriesType = Energinet.DataHub.Edi.Requests.TimeSeriesType.Production,
            };

        // Act
        var validationStatus = _sut.Validate(request);

        // Assert
        Assert.False(validationStatus.IsValid);
        Assert.Equal("D66", validationStatus.Errors.First().ErrorCode);
    }

    [Fact]
    public void Validate_AggregatedTimeSeriesRequest_FailsOnStartTimeFormat()
    {
        // Arrange
        var request = new Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest()
            {
                Period = new Energinet.DataHub.Edi.Requests.Period()
                {
                    Start = "a023-07-27T22:00:00Z",
                    End = Instant.FromUtc(2022, 1, 1, 22, 0, 0).ToString(),
                },
                TimeSeriesType = Energinet.DataHub.Edi.Requests.TimeSeriesType.Production,
            };

        // Act
        var validationStatus = _sut.Validate(request);

        // Assert
        Assert.False(validationStatus.IsValid);
        Assert.Equal("D66", validationStatus.Errors.First().ErrorCode);
    }

    [Fact]
    public void Validate_AggregatedTimeSeriesRequest_FailsOnEndTimeFormat()
    {
        // Arrange
        var request = new Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest()
            {
                Period = new Energinet.DataHub.Edi.Requests.Period()
                {
                    Start = Instant.FromUtc(2022, 1, 1, 22, 0, 0).ToString(),
                    End = "a023-07-27T22:00:00Z",
                },
                TimeSeriesType = Energinet.DataHub.Edi.Requests.TimeSeriesType.Production,
            };

        // Act
        var validationStatus = _sut.Validate(request);

        // Assert
        Assert.False(validationStatus.IsValid);
        Assert.Equal("D66", validationStatus.Errors.First().ErrorCode);
    }
}
