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
    private static readonly ValidationError _chargeTypeIdIsToLongError = new ValidationError(
        "Følgende chargeType er for lang: {PropertyName}. Den må højst indeholde 10 karaktere/"
        + "The following ChargeId is to long: {PropertyName} It must at most be 10 characters",
        "D14");

    private readonly ChargeTypeValidationRule _sut = new();

    [Theory]
    [InlineData(null)]
    [InlineData("0")]
    [InlineData("%/)(&)")]
    [InlineData("0000000000")]
    [InlineData("1234567890")]
    [InlineData("-234567890")]
    public async Task Validate_WhenChargeTypeContainsAValidType_returnsExpectedValidationError(string? chargeTypeId)
    {
        // Arrange
        var chargeTypes = Array.Empty<ChargeType>();

        if (chargeTypeId is not null)
            chargeTypes = [new ChargeType() { ChargeType_ = chargeTypeId, ChargeCode = "D01" }];

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
    public async Task Validate_WhenChargeTypeContainsAInvalidType_returnsExpectedValidationError(string chargeTypeId)
    {
        // Arrange
        var chargeTypes = new ChargeType() { ChargeType_ = chargeTypeId, ChargeCode = "D01" };

        var message = new WholesaleServicesRequestBuilder()
            .WithChargeTypes(chargeTypes)
            .Build();

        // Act
        var validationErrors = await _sut.ValidateAsync(message);

        // Assert
        validationErrors.Should().ContainSingle()
            .Subject.Should().Be(
                _chargeTypeIdIsToLongError.WithPropertyName(chargeTypeId));
    }

    [Fact]
    public async Task Validate_WhenChargeTypeContainsMultipleInvalidType_returnsExpectedValidationError()
    {
        // Arrange
        var chargeTypes = new ChargeType[]
            {
                new ChargeType() { ChargeType_ = "12345678901", ChargeCode = "D01" },
                new ChargeType() { ChargeType_ = "10987654321", ChargeCode = "D01" },
            };
        var expectedErrors = new List<ValidationError>();

        for (var i = 0; i < chargeTypes.Count(); i++)
        {
            expectedErrors.Add(_chargeTypeIdIsToLongError.WithPropertyName(chargeTypes[i].ChargeType_));
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
        var invalidCharType = "ThisIsMoreThan10Charlong";
        var chargeTypes = new ChargeType[]
            {
                new ChargeType() { ChargeType_ = "valid1", ChargeCode = "D01" },
                new ChargeType() { ChargeType_ = invalidCharType, ChargeCode = "D01" },
                new ChargeType() { ChargeType_ = "valid2", ChargeCode = "D01" },
            };

        var message = new WholesaleServicesRequestBuilder()
            .WithChargeTypes(chargeTypes)
            .Build();

        // Act
        var validationErrors = await _sut.ValidateAsync(message);

        // Assert
        validationErrors.Should().ContainSingle().Subject.Should().Be(_chargeTypeIdIsToLongError.WithPropertyName(invalidCharType));
    }
}
