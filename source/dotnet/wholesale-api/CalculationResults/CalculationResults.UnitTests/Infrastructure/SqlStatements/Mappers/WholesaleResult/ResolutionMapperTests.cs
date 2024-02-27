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
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SqlStatements.Mappers.WholesaleResult;

public class ResolutionMapperTests
{
    private const string DocumentPath = "DeltaTableContracts.enums.wholesale-result-resolution.json";

    [Fact]
    public async Task ContractPropertyCount_Matches_ModelValuesCount()
    {
        // Arrange
        await using var stream = EmbeddedResources.GetStream<Root>(DocumentPath);
        var validDeltaValues = await ContractComplianceTestHelper.GetCodeListValuesAsync(stream);

        // Act
        var expectedLength = Enum.GetNames(typeof(Resolution)).Length;

        // Assert
        expectedLength.Should().Be(validDeltaValues.Count);
    }

    [Theory]
    [InlineData("P1M")]
    [InlineData("P1D")]
    [InlineData("PT1H")]
    public async Task ModelValues_Matches_DeltaTableValues(string deltaTableValue)
    {
        // Arrange
        await using var stream = EmbeddedResources.GetStream<Root>(DocumentPath);
        var validDeltaValues = await ContractComplianceTestHelper.GetCodeListValuesAsync(stream);

        // Assert
        deltaTableValue.Should().BeOneOf(validDeltaValues);
    }

    [Fact]
    public async Task FromDeltaTableValue_MapsAllValidDeltaTableValues()
    {
        // Arrange
        await using var stream = EmbeddedResources.GetStream<Root>(DocumentPath);
        var validDeltaValues = await ContractComplianceTestHelper.GetCodeListValuesAsync(stream);

        foreach (var validDeltaValue in validDeltaValues)
        {
            // Act
            var actual = ResolutionMapper.FromDeltaTableValue(validDeltaValue);

            // Assert it's a defined enum value (and not null)
            actual.Should().BeDefined();
        }
    }

    [Theory]
    [InlineData("P1M", Resolution.Month)]
    [InlineData("P1D", Resolution.Day)]
    [InlineData("PT1H", Resolution.Hour)]
    public void FromDeltaTableValue_WhenValidDeltaTableValue_ReturnsExpectedType(string deltaTableValue, Resolution expectedType)
    {
        // Act
        var actualType = ResolutionMapper.FromDeltaTableValue(deltaTableValue);

        // Assert
        actualType.Should().Be(expectedType);
    }

    [Fact]
    public void FromDeltaTableValue_WhenInvalidDeltaTableValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidDeltaTableValue = Guid.NewGuid().ToString();

        // Act
        var act = () => ResolutionMapper.FromDeltaTableValue(invalidDeltaTableValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidDeltaTableValue);
    }
}
