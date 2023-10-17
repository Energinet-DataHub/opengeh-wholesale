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
using Energinet.DataHub.Wholesale.Common.Models;
using FluentAssertions;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SqlStatements.Mappers.WholesaleResult;

public class QuantityUnitMapperTests
{
    [Fact]
    public async Task QuantityUnit_Matches_ContractPropertyCount()
    {
        // Arrange
        await using var stream = EmbeddedResources.GetStream<Root>("DeltaTableContracts.enums.charge-unit.json");
        var validDeltaValues = await ContractComplianceTestHelper.GetCodeListValuesAsync(stream);

        // Act
        var expectedLength = Enum.GetNames(typeof(QuantityUnit)).Length;

        // Assert
        expectedLength.Should().Be(validDeltaValues.Count);
    }

    [Theory]
    [InlineData("kWh")]
    [InlineData("pcs")]
    public async Task QuantityUnit_Matches_Contract(string quantityUnit)
    {
        // Arrange
        await using var stream = EmbeddedResources.GetStream<Root>("DeltaTableContracts.enums.charge-unit.json");
        var validDeltaValues = await ContractComplianceTestHelper.GetCodeListValuesAsync(stream);

        // Assert
        quantityUnit.Should().BeOneOf(validDeltaValues);
   }

    [Fact]
    public async Task FromDeltaTableValue_MapsAllValidDeltaTableValues()
    {
        // Arrange
        await using var stream = EmbeddedResources.GetStream<Root>("DeltaTableContracts.enums.charge-unit.json");
        var validDeltaValues = await ContractComplianceTestHelper.GetCodeListValuesAsync(stream);

        foreach (var validDeltaValue in validDeltaValues)
        {
            // Act
            var actual = QuantityUnitMapper.FromDeltaTableValue(validDeltaValue);

            // Assert it's a defined enum value (and not null)
            actual.Should().BeDefined();
        }
    }

    [Theory]
    [InlineData("kWh", QuantityUnit.Kwh)]
    [InlineData("pcs", QuantityUnit.Pieces)]
    public void FromDeltaTableValue_WhenValidDeltaTableValue_ReturnsExpectedType(string deltaTableValue, QuantityUnit expectedType)
    {
        // Act
        var actualType = QuantityUnitMapper.FromDeltaTableValue(deltaTableValue);

        // Assert
        actualType.Should().Be(expectedType);
    }

    [Fact]
    public void FromDeltaTableValue_WhenInvalidDeltaTableValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidDeltaTableValue = Guid.NewGuid().ToString();

        // Act
        var act = () => QuantityUnitMapper.FromDeltaTableValue(invalidDeltaTableValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidDeltaTableValue);
    }
}
