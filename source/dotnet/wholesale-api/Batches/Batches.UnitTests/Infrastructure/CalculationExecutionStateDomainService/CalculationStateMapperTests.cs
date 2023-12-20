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
using Energinet.DataHub.Wholesale.Batches.Application.Model;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.BatchState;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Batches.UnitTests.Infrastructure.CalculationExecutionStateDomainService;

public class CalculationStateMapperTests
{
    [Theory]
    [InlineAutoMoqData(CalculationState.Pending, CalculationExecutionState.Pending)]
    [InlineAutoMoqData(CalculationState.Running, CalculationExecutionState.Executing)]
    [InlineAutoMoqData(CalculationState.Completed, CalculationExecutionState.Completed)]
    [InlineAutoMoqData(CalculationState.Canceled, CalculationExecutionState.Canceled)]
    [InlineAutoMoqData(CalculationState.Failed, CalculationExecutionState.Failed)]
    public void MapState_CalledWithACalculationStateItCanMap_ExpectedBatchExecutionState(CalculationState calculationState, CalculationExecutionState expectedCalculationExecutionState)
    {
        // Act
        var actualBatchExecutionState = CalculationStateMapper.MapState(calculationState);

        // Assert
        actualBatchExecutionState.Should().Be(expectedCalculationExecutionState);
    }

    [Fact]
    public void MapState_CalledWithAUnexpectedCalculationStateItCanNotMap_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        const CalculationState unexpectedCalculationState = (CalculationState)99;

        // Act
        var act = () => CalculationStateMapper.MapState(unexpectedCalculationState);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(unexpectedCalculationState);
    }
}
