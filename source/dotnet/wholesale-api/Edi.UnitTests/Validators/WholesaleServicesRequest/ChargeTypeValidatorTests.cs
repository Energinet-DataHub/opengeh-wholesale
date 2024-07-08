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
using Energinet.DataHub.Wholesale.Edi.UnitTests.Builders;
using Energinet.DataHub.Wholesale.Edi.Validation;
using Energinet.DataHub.Wholesale.Edi.Validation.WholesaleServicesRequest.Rules;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Validators.WholesaleServicesRequest;

public class ChargeTypeValidatorTests
{
    private static readonly ValidationError _chargeTypeIdIsToLongError = new(
        "Følgende chargeType mRID er for lang: {PropertyName}. Den må højst indeholde 10 karaktere/"
        + "The following chargeType mRID is to long: {PropertyName} It must at most be 10 characters",
        "D14");

    private readonly ChargeCodeValidationRule _sut = new();

    [Theory]
    [InlineData(null)]
    [InlineData("0")]
    [InlineData("%/)(&)")]
    [InlineData("0000000000")]
    [InlineData("1234567890")]
    [InlineData("-234567890")]
    public async Task Validate_WhenChargeTypeContainsAValidType_returnsExpectedValidationError(string? chargeCode)
    {
        // Arrange
        var chargeTypes = Array.Empty<ChargeType>();

        if (chargeCode is not null)
            chargeTypes = [new ChargeType() { ChargeType_ = "D01", ChargeCode = chargeCode }];

        var message = new WholesaleServicesRequestBuilder()
            .WithChargeTypes(chargeTypes)
            .Build();

        // Act
        var validationErrors = await _sut.ValidateAsync(message);

        // Assert
        validationErrors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("12345678901")] // 11 char long
    public async Task Validate_WhenChargeTypeContainsAInvalidType_returnsExpectedValidationError(string chargeCode)
    {
        // Arrange
        var chargeTypes = new ChargeType() { ChargeType_ = "D01", ChargeCode = chargeCode };

        var message = new WholesaleServicesRequestBuilder()
            .WithChargeTypes(chargeTypes)
            .Build();

        // Act
        var validationErrors = await _sut.ValidateAsync(message);

        // Assert
        validationErrors.Should().ContainSingle()
            .Subject.Should().Be(
                _chargeTypeIdIsToLongError.WithPropertyName(chargeCode));
    }

    [Fact]
    public async Task Validate_WhenChargeTypeContainsMultipleInvalidType_returnsExpectedValidationError()
    {
        // Arrange
        var chargeTypes = new ChargeType[]
            {
                new() { ChargeType_ = "D01", ChargeCode = "12345678901" },
                new() { ChargeType_ = "D01", ChargeCode = "10987654321" },
            };
        var expectedErrors = new List<ValidationError>();

        foreach (var t in chargeTypes)
        {
            expectedErrors.Add(_chargeTypeIdIsToLongError.WithPropertyName(t.ChargeCode));
        }

        var message = new WholesaleServicesRequestBuilder()
            .WithChargeTypes(chargeTypes)
            .Build();

        // Act
        var validationErrors = await _sut.ValidateAsync(message);

        // Assert
        validationErrors.Should().Equal(expectedErrors);
    }

    [Fact]
    public async Task Validate_WhenMultipleChargeTypeButOneHasInvalidType_returnsExpectedValidationError()
    {
        // Arrange
        var invalidChargeCode = "ThisIsMoreThan10CharacterLong";
        var chargeTypes = new ChargeType[]
            {
                new() { ChargeType_ = "D01", ChargeCode = "valid1" },
                new() { ChargeType_ = "D01", ChargeCode = invalidChargeCode },
                new() { ChargeType_ = "D01", ChargeCode = "valid2" },
            };

        var message = new WholesaleServicesRequestBuilder()
            .WithChargeTypes(chargeTypes)
            .Build();

        // Act
        var validationErrors = await _sut.ValidateAsync(message);

        // Assert
        validationErrors.Should().ContainSingle().Subject.Should().Be(_chargeTypeIdIsToLongError.WithPropertyName(invalidChargeCode));
    }
}
