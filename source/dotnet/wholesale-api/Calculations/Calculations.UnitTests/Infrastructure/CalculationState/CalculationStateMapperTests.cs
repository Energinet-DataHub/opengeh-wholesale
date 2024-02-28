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
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.CalculationState;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Calculations.UnitTests.Infrastructure.CalculationState;

public class CalculationStateMapperTests
{
    [Theory]
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Pending, CalculationExecutionState.Pending)]
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Running, CalculationExecutionState.Executing)]
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Completed, CalculationExecutionState.Completed)]
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Canceled, CalculationExecutionState.Canceled)]
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Failed, CalculationExecutionState.Failed)]
    public void MapState_CalledWithACalculationStateItCanMap_ExpectedCalculationExecutionState(Wholesale.Calculations.Application.Model.CalculationState calculationState, CalculationExecutionState expectedCalculationExecutionState)
    {
        // Act
        var actualCalculationExecutionState = CalculationStateMapper.MapState(calculationState);

        // Assert
        actualCalculationExecutionState.Should().Be(expectedCalculationExecutionState);
    }

    [Fact]
    public void MapState_CalledWithAUnexpectedCalculationStateItCanNotMap_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        const Wholesale.Calculations.Application.Model.CalculationState unexpectedCalculationState = (Wholesale.Calculations.Application.Model.CalculationState)99;

        // Act
        var act = () => CalculationStateMapper.MapState(unexpectedCalculationState);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(unexpectedCalculationState);
    }
}
