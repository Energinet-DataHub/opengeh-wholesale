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
using Energinet.DataHub.Wholesale.Batches.Application;
using Energinet.DataHub.Wholesale.Batches.Application.Model;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Batches;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.BatchState;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Batches.UnitTests.Infrastructure.BatchExecutionStateDomainService;

public class BatchStateMapperTests
{
    [Theory]
    [InlineAutoMoqData(CalculationState.Pending, BatchExecutionState.Pending)]
    [InlineAutoMoqData(CalculationState.Running, BatchExecutionState.Executing)]
    [InlineAutoMoqData(CalculationState.Completed, BatchExecutionState.Completed)]
    [InlineAutoMoqData(CalculationState.Canceled, BatchExecutionState.Canceled)]
    [InlineAutoMoqData(CalculationState.Failed, BatchExecutionState.Failed)]
    public void MapState_CalledWithACalculationStateItCanMap_ExpectedBatchExecutionState(CalculationState calculationState, BatchExecutionState expectedBatchExecutionState)
    {
        // Act
        var actualBatchExecutionState = BatchStateMapper.MapState(calculationState);

        // Assert
        actualBatchExecutionState.Should().Be(expectedBatchExecutionState);
    }

    [Fact]
    public void MapState_CalledWithAUnexpectedCalculationStateItCanNotMap_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        const CalculationState unexpectedCalculationState = (CalculationState)99;

        // Act
        var act = () => BatchStateMapper.MapState(unexpectedCalculationState);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(unexpectedCalculationState);
    }
}
