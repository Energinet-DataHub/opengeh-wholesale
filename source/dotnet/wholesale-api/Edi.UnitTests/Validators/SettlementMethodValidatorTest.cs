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
using Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSerie.Rules;
using FluentAssertions;
using Xunit;
using AggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class SettlementMethodValidatorTest
{
    private const string ExpectedErrorMessage = "SettlementMethod kan kun benyttes i kombination med E17 og skal være enten D01 og E02 / SettlementMethod can only be used in combination with E17 and must be either D01 or E02";
    private const string ExpectedErrorCode = "D15";

    private readonly SettlementMethodValidationRule _sut = new();

    [Theory]
    [InlineData(SettlementMethod.Flex)]
    [InlineData(SettlementMethod.NonProfiled)]
    public void Validate_IsConsumptionAndSettlementMethodIsValid_NoValidationErrors(string settlementMethod)
    {
        // Arrange
        var message = CreateAggregatedTimeSeriesRequest(MeteringPointType.Consumption, settlementMethod);

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(MeteringPointType.Production)]
    [InlineData(MeteringPointType.Exchange)]
    [InlineData("not-consumption")]
    public void Validate_IsNotConsumptionAndSettlementMethodIsNull_NoValidationErrors(string meteringPointType)
    {
        // Arrange
        var message = CreateAggregatedTimeSeriesRequest(meteringPointType, null);

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_IsConsumptionAndSettlementMethodIsInvalid_ValidationError()
    {
        // Arrange
        var message = CreateAggregatedTimeSeriesRequest(MeteringPointType.Consumption, "invalid-settlement-method");

        // Act
        var errors = _sut.Validate(message);

        // Assert
        AssertSingleAndCorrectError(errors);
    }

    [Theory]
    [InlineData(MeteringPointType.Production, SettlementMethod.Flex)]
    [InlineData(MeteringPointType.Production, SettlementMethod.NonProfiled)]
    [InlineData(MeteringPointType.Production, "invalid-settlement-method")]
    [InlineData(MeteringPointType.Exchange, SettlementMethod.Flex)]
    [InlineData(MeteringPointType.Exchange, SettlementMethod.NonProfiled)]
    [InlineData(MeteringPointType.Exchange, "invalid-settlement-method")]
    [InlineData("not-consumption-metering-point", SettlementMethod.Flex)]
    [InlineData("not-consumption-metering-point", SettlementMethod.NonProfiled)]
    [InlineData("not-consumption-metering-point", "invalid-settlement-method")]
    [InlineData("", SettlementMethod.Flex)]
    [InlineData("", SettlementMethod.NonProfiled)]
    [InlineData("", "invalid-settlement-method")]
    public void Validate_IsNotConsumptionAndSettlementMethodIsGiven_ValidationError(string meteringPointType, string settlementMethod)
    {
        // Arrange
        var message = CreateAggregatedTimeSeriesRequest(meteringPointType, settlementMethod);

        // Act
        var errors = _sut.Validate(message);

        // Assert
        AssertSingleAndCorrectError(errors);
    }

    private static AggregatedTimeSeriesRequest CreateAggregatedTimeSeriesRequest(string meteringPointType, string? settlementMethod)
    {
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithMeteringPointType(meteringPointType)
            .WithSettlementMethod(settlementMethod)
            .Build();

        return message;
    }

    private void AssertSingleAndCorrectError(IList<ValidationError> errors)
    {
        Assert.Single(errors);

        var error = errors.Single();
        Assert.Contains(ExpectedErrorMessage, error.Message);
        Assert.Contains(ExpectedErrorCode, error.ErrorCode);
    }
}
