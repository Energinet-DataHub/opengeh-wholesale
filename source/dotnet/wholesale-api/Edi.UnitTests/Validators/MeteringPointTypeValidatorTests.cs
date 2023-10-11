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
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class MeteringPointTypeValidatorTests
{
    private readonly MeteringPointTypeValidationRule _sut = new();

    [Theory]
    [InlineData("E17")]
    [InlineData("E18")]
    [InlineData("E20")]
    public void Validate_WhenValidMeteringPoint_ReturnsExceptedNoValidationErrors(string meteringPointType)
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
    public void Validate_InvalidMeteringPointType_ReturnsExceptedValidationError(string meteringPointType)
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
        errors.First().ErrorCode.Should().Be(ValidationError.InvalidMeteringPointType.ErrorCode);
    }
}
