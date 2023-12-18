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

public class SettlementSeriesVersionValidatorTest
{
    private static readonly ValidationError _expectedInvalidSettlementMethodError = new("SettlementSeriesVersion kan kun benyttes i kombination med D32 og skal være enten D01, D02 eller D03 / SettlementSeriesVersion can only be used in combination with D32 and must be either D01, D02 or D03", "E86");

    private readonly SettlementSeriesVersionValidationRule _sut = new();

    [Theory]
    [InlineData("invalid-settlement-series-version")]
    [InlineData("D04")]
    [InlineData("")]
    public async Task Validate_WhenCorrectionAndInvalidSeriesVersion_ReturnsValidationErrorsAsync(string invalidSettlementSeriesVersion)
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithBusinessReason(BusinessReason.Correction)
            .WithSettlementSeriesVersion(invalidSettlementSeriesVersion)
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(_expectedInvalidSettlementMethodError);
    }

    [Theory]
    [InlineData("invalid-settlement-series-version")]
    [InlineData("D04")]
    [InlineData("")]
    [InlineData(SettlementSeriesVersion.FirstCorrection)]
    [InlineData(SettlementSeriesVersion.SecondCorrection)]
    [InlineData(SettlementSeriesVersion.ThirdCorrection)]
    public async Task Validate_WhenNotCorrectionAndSettlementSeriesVersionExists_ReturnsValidationErrorsAsync(string settlementSeriesVersion)
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithBusinessReason(BusinessReason.WholesaleFixing)
            .WithSettlementSeriesVersion(settlementSeriesVersion)
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(_expectedInvalidSettlementMethodError);
    }

    [Theory]
    [InlineData(SettlementSeriesVersion.FirstCorrection)]
    [InlineData(SettlementSeriesVersion.SecondCorrection)]
    [InlineData(SettlementSeriesVersion.ThirdCorrection)]
    public async Task Validate_WhenCorrectionAndValidSettlementSeriesVersion_ReturnsNoValidationErrorsAsync(string validSettlementSeriesVersion)
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithBusinessReason(BusinessReason.Correction)
            .WithSettlementSeriesVersion(validSettlementSeriesVersion)
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WhenCorrectionAndNoSettlementSeriesVersion_ReturnsNoValidationErrorsAsync()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithBusinessReason(BusinessReason.Correction)
            .WithSettlementSeriesVersion(null)
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WhenNotCorrectionAndNoSettlementSeriesVersion_ReturnsNoValidationErrorsAsync()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithBusinessReason(BusinessReason.WholesaleFixing)
            .WithSettlementSeriesVersion(null)
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().BeEmpty();
    }
}
