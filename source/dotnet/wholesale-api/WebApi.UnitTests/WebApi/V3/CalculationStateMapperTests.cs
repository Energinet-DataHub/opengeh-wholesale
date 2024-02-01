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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.WebApi.V3.Calculation;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.WebApi.V3;

public class CalculationStateMapperTests
{
    [Theory]
    [InlineAutoMoqData(Calculations.Interfaces.Models.CalculationState.Failed, CalculationState.Failed)]
    [InlineAutoMoqData(Calculations.Interfaces.Models.CalculationState.Completed, CalculationState.Completed)]
    [InlineAutoMoqData(Calculations.Interfaces.Models.CalculationState.Executing, CalculationState.Executing)]
    [InlineAutoMoqData(Calculations.Interfaces.Models.CalculationState.Pending, CalculationState.Pending)]
    public void Map_ReturnsExpectedTypeForWebApi(Calculations.Interfaces.Models.CalculationState source, CalculationState expected)
    {
        var actual = CalculationStateMapper.MapState(source);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineAutoMoqData(CalculationState.Failed, Calculations.Interfaces.Models.CalculationState.Failed)]
    [InlineAutoMoqData(CalculationState.Completed, Calculations.Interfaces.Models.CalculationState.Completed)]
    [InlineAutoMoqData(CalculationState.Executing, Calculations.Interfaces.Models.CalculationState.Executing)]
    [InlineAutoMoqData(CalculationState.Pending, Calculations.Interfaces.Models.CalculationState.Pending)]
    public void Map_ReturnsExpectedTypeForBatchModule(CalculationState source, Calculations.Interfaces.Models.CalculationState expected)
    {
        var actual = CalculationStateMapper.MapState(source);
        actual.Should().Be(expected);
    }

    [Fact]
    public void MapState_WhenInvalidEnumNumberForBatchState_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (Calculations.Interfaces.Models.CalculationState)99;

        // Act
        var act = () => CalculationStateMapper.MapState(invalidValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidValue);
    }

    [Fact]
    public void MapState_WhenInvalidEnumNumberForV3BatchState_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (CalculationState)99;

        // Act
        var act = () => CalculationStateMapper.MapState(invalidValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidValue);
    }
}
