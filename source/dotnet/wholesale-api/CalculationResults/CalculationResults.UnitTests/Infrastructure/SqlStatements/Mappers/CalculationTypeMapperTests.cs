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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SqlStatements.Mappers;

public class CalculationTypeMapperTests
{
    [Theory]
    [InlineData(CalculationType.Aggregation, DeltaTableCalculationType.Aggregation)]
    [InlineData(CalculationType.BalanceFixing, DeltaTableCalculationType.BalanceFixing)]
    [InlineData(CalculationType.WholesaleFixing, DeltaTableCalculationType.WholesaleFixing)]
    [InlineData(CalculationType.FirstCorrectionSettlement, DeltaTableCalculationType.FirstCorrectionSettlement)]
    [InlineData(CalculationType.SecondCorrectionSettlement, DeltaTableCalculationType.SecondCorrectionSettlement)]
    [InlineData(CalculationType.ThirdCorrectionSettlement, DeltaTableCalculationType.ThirdCorrectionSettlement)]
    public void ToDeltaTableValue_ReturnsExpectedString(CalculationType type, string expected)
    {
        // Act
        var actual = CalculationTypeMapper.ToDeltaTableValue(type);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(DeltaTableCalculationType.Aggregation, CalculationType.Aggregation)]
    [InlineData(DeltaTableCalculationType.BalanceFixing, CalculationType.BalanceFixing)]
    [InlineData(DeltaTableCalculationType.WholesaleFixing, CalculationType.WholesaleFixing)]
    [InlineData(DeltaTableCalculationType.FirstCorrectionSettlement, CalculationType.FirstCorrectionSettlement)]
    [InlineData(DeltaTableCalculationType.SecondCorrectionSettlement, CalculationType.SecondCorrectionSettlement)]
    [InlineData(DeltaTableCalculationType.ThirdCorrectionSettlement, CalculationType.ThirdCorrectionSettlement)]
    public void FromDeltaTableValue_ReturnsExpectedType(string deltaTableValue, CalculationType expected)
    {
        // Act
        var actual = CalculationTypeMapper.FromDeltaTableValue(deltaTableValue);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void FromDeltaTableValue_WhenInvalidDeltaTableValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidDeltaTableValue = Guid.NewGuid().ToString();

        // Act
        var act = () => CalculationTypeMapper.FromDeltaTableValue(invalidDeltaTableValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidDeltaTableValue);
    }

    [Fact]
    public void ToDeltaTableValue_WhenInvalidEnumNumberForCalculationType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (CalculationType)99;

        // Act
        var act = () => CalculationTypeMapper.ToDeltaTableValue(invalidValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidValue);
    }
}
