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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.WholesaleResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SqlStatements.Mappers.WholesaleResult;

public class ChargeTypeMapperTests
{
    [Theory]
    [InlineData("fee", ChargeType.Fee)]
    [InlineData("subscription", ChargeType.Subscription)]
    [InlineData("tariff", ChargeType.Tariff)]
    public void FromDeltaTableValue_WhenValidDeltaTableValue_ReturnsExpectedType(string deltaTableValue, ChargeType expectedType)
    {
        // Act
        var actualType = ChargeTypeMapper.FromDeltaTableValue(deltaTableValue);

        // Assert
        actualType.Should().Be(expectedType);
    }

    [Fact]
    public void FromDeltaTableValue_WhenInvalidDeltaTableValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidDeltaTableValue = Guid.NewGuid().ToString();

        // Act
        var act = () => ChargeTypeMapper.FromDeltaTableValue(invalidDeltaTableValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidDeltaTableValue);
    }

    [Theory]
    [InlineData("fee", ChargeType.Fee)]
    [InlineData("subscription", ChargeType.Subscription)]
    [InlineData("tariff", ChargeType.Tariff)]
    public void ToDeltaTableValue_WhenValidChargeTypeValue_ReturnsExpectedString(string expectedDeltaTableValue, ChargeType chargeType)
    {
        // Act
        var actualDeltaTableValue = ChargeTypeMapper.ToDeltaTableValue(chargeType);

        // Assert
        actualDeltaTableValue.Should().Be(expectedDeltaTableValue);
    }

    [Fact]
    public void ToDeltaTableValue_WhenInvalidChargeTypeValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidChargeTypeValue = (ChargeType)int.MinValue;

        // Act
        var act = () => ChargeTypeMapper.ToDeltaTableValue(invalidChargeTypeValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidChargeTypeValue);
    }
}
