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
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Calculations.UnitTests.Infrastructure.CalculationState;

public class CalculationStateMapperTests
{
    [Theory]
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Pending, CalculationOrchestrationState.Scheduled)]
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Running, CalculationOrchestrationState.Calculating)]
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Completed, CalculationOrchestrationState.Calculated)]
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Failed, CalculationOrchestrationState.CalculationFailed)]
    public void MapState_CalledWithACalculationStateItCanMap_ExpectedCalculationOrchestrationState(Wholesale.Calculations.Application.Model.CalculationState calculationState, CalculationOrchestrationState expectedCalculationOrchestrationState)
    {
        // Act
        var actualCalculationExecutionState = CalculationStateMapper.MapState(calculationState);

        // Assert
        actualCalculationExecutionState.Should().Be(expectedCalculationOrchestrationState);
    }

    [Theory]
    [InlineAutoMoqData(Wholesale.Calculations.Application.Model.CalculationState.Canceled)]
    [InlineAutoMoqData((Wholesale.Calculations.Application.Model.CalculationState)99)]
    public void MapState_CalledWithAUnexpectedCalculationStateItCanNotMap_ThrowsArgumentOutOfRangeException(Wholesale.Calculations.Application.Model.CalculationState unexpectedCalculationState)
    {
        // Act
        var act = () => CalculationStateMapper.MapState(unexpectedCalculationState);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(unexpectedCalculationState);
    }
}
