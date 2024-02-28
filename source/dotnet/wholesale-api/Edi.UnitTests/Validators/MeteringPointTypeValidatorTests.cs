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

using Energinet.DataHub.Wholesale.EDI.Models;
using Energinet.DataHub.Wholesale.EDI.UnitTests.Builders;
using Energinet.DataHub.Wholesale.EDI.Validation;
using Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSeries.Rules;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class MeteringPointTypeValidatorTests
{
    private static readonly ValidationError _invalidMeteringPointType =
        new(
            "Metering point type skal være tom eller en af følgende: {PropertyName} / Metering point type has to be empty or one of the following: {PropertyName}",
            "D18");

    private readonly MeteringPointTypeValidationRule _sut = new();

    [Theory]
    [InlineData(MeteringPointType.Consumption)]
    [InlineData(MeteringPointType.Production)]
    [InlineData(MeteringPointType.Exchange)]
    [InlineData("")]
    public async Task Validate_WhenMeteringPointIsValid_ReturnsExpectedNoValidationErrors(string meteringPointType)
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithMeteringPointType(meteringPointType)
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WhenMeteringPointTypeIsInvalid_ReturnsExpectedValidationError()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithMeteringPointType("Invalid")
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().ContainSingle();
        var error = errors.First();
        error.ErrorCode.Should().Be(_invalidMeteringPointType.ErrorCode);
        error.Message.Should().Be(_invalidMeteringPointType.WithPropertyName("E17, E18, E20").Message);
    }
}
