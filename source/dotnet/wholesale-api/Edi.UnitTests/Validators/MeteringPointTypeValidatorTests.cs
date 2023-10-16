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

using Energinet.DataHub.Wholesale.Edi.Models;
using Energinet.DataHub.Wholesale.EDI.UnitTests.Builders;
using Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSerie.Rules;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class MeteringPointTypeValidatorTests
{
    private const string ExceptedErrorCodes = "D18";
    private const string ExceptedErrorMessage = "Metering point type skal være en af følgende: E17, E18, E20 / Metering point type has to be one of the following: E17, E18, E20";

    private readonly MeteringPointTypeValidationRule _sut = new();

    [Theory]
    [InlineData(MeteringPointType.Consumption)]
    [InlineData(MeteringPointType.Production)]
    [InlineData(MeteringPointType.Exchange)]
    public void Validate_WhenValidMeteringPoint_ReturnsExpectedNoValidationErrors(string meteringPointType)
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithMeteringPointType(meteringPointType)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Invalid")]
    public void Validate_InvalidMeteringPointType_ReturnsExpectedValidationError(string meteringPointType)
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithMeteringPointType(meteringPointType)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().ContainSingle();
        var error = errors.First();
        Assert.Equal(ExceptedErrorCodes, error.ErrorCode);
        Assert.Equal(ExceptedErrorMessage, error.Message);
    }
}
