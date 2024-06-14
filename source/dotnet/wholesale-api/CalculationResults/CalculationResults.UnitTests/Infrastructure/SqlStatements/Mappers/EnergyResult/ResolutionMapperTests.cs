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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.EnergyResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SqlStatements.Mappers.EnergyResult;

public class ResolutionMapperTests
{
    [Theory]
    [InlineData("PT15M", Resolution.Quarter)]
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

    [Theory]
    [InlineData("PT15M", Resolution.Quarter)]
    [InlineData("PT1H", Resolution.Hour)]
    public void ToDeltaTableValue_WhenValidResolutionValue_ReturnsExpectedString(string expectedDeltaTableValue, Resolution resolution)
    {
        // Act
        var actualDeltaTableValue = ResolutionMapper.ToDeltaTableValue(resolution);

        // Assert
        actualDeltaTableValue.Should().Be(expectedDeltaTableValue);
    }

    [Fact]
    public void ToDeltaTableValue_WhenInvalidResolutionValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidResolutionValue = (Resolution)int.MinValue;

        // Act
        var act = () => ResolutionMapper.ToDeltaTableValue(invalidResolutionValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidResolutionValue);
    }
}
