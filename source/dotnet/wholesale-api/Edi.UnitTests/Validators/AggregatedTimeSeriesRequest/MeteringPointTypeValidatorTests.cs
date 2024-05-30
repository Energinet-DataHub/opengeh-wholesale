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

using Energinet.DataHub.Wholesale.Edi.Contracts;
using Energinet.DataHub.Wholesale.Edi.UnitTests.Builders;
using Energinet.DataHub.Wholesale.Edi.Validation;
using Energinet.DataHub.Wholesale.Edi.Validation.AggregatedTimeSeriesRequest.Rules;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Validators.AggregatedTimeSeriesRequest;

public class MeteringPointTypeValidatorTests
{
    private static readonly ValidationError _invalidMeteringPointType =
        new(
            "Metering point type skal være en af følgende: {PropertyName} eller undladt / Metering point type has one of the following: {PropertyName} or omitted",
            "D18");

    private readonly MeteringPointTypeValidationRule _sut = new();

    [Theory]
    [InlineData(DataHubNames.MeteringPointType.Consumption)]
    [InlineData(DataHubNames.MeteringPointType.Production)]
    [InlineData(DataHubNames.MeteringPointType.Exchange)]
    [InlineData(null)]
    public async Task Validate_WhenMeteringPointIsValid_ReturnsExpectedNoValidationErrors(string? meteringPointType)
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

    [Theory]
    [InlineData("Invalid")]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validate_WhenMeteringPointTypeIsInvalid_ReturnsExpectedValidationError(string? meteringPointType)
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithMeteringPointType(meteringPointType)
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
