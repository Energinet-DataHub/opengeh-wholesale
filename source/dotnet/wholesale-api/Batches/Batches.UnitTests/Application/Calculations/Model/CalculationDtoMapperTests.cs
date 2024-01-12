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
using Energinet.DataHub.Wholesale.Batches.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.Batches.UnitTests.Infrastructure.CalculationAggregate;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Batches.UnitTests.Application.Calculations.Model;

public class CalculationDtoMapperTests
{
    [Theory]
    [InlineAutoMoqData]
    public void Map_Returns_CorrectState(
        CalculationDtoMapper sut)
    {
        // Arrange
        var batch = new CalculationBuilder().WithStateExecuting().Build();

        // Act
        var batchDto = sut.Map(batch);

        // Assert
        batchDto.ExecutionState.Should().Be(CalculationState.Executing);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Map_Returns_CorrectPeriod(
        CalculationDtoMapper sut)
    {
        // Arrange
        var batch = new CalculationBuilder().Build();

        // Act
        var batchDto = sut.Map(batch);

        // Assert
        batchDto.PeriodStart.Should().Be(batch.PeriodStart.ToDateTimeOffset());
        batchDto.PeriodEnd.Should().Be(batch.PeriodEnd.ToDateTimeOffset());
    }

    [Theory]
    [InlineAutoMoqData]
    public void Map_When_ExecutionTimeIsNotNull_Returns_CorrectExecutionTime(
        CalculationDtoMapper sut)
    {
        // Arrange
        var batch = new CalculationBuilder().Build();
        batch.MarkAsExecuting(); // this sets ExecutionTimeStart
        batch.MarkAsCompleted(batch.ExecutionTimeStart!.Value.Plus(Duration.FromDays(2))); // this sets ExecutionTimeEnd

        // Act
        var batchDto = sut.Map(batch);

        // Assert
        batchDto.ExecutionTimeStart.Should().Be(batch.ExecutionTimeStart.Value.ToDateTimeOffset());
        batchDto.ExecutionTimeEnd.Should().Be(batch.ExecutionTimeEnd!.Value.ToDateTimeOffset());
    }

    [Theory]
    [InlineAutoMoqData]
    public void Map_BatchNumber_Equals_RunId(
        CalculationDtoMapper sut)
    {
        // Arrange
        var batch = new CalculationBuilder().Build();
        var expectedRunId = new CalculationId(111);
        batch.MarkAsSubmitted(expectedRunId);

        // Act
        var batchDto = sut.Map(batch);

        // Assert
        batchDto.RunId.Should().Be(expectedRunId.Id);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Map_When_NoRunIdIsNull_Then_BatchNumberIsNull(
        CalculationDtoMapper sut)
    {
        // Arrange
        var batch = new CalculationBuilder().Build();

        // Act
        var batchDto = sut.Map(batch);

        // Assert
        batchDto.RunId.Should().Be(null);
    }

    [Theory]
    [AutoMoqData]
    public void Map_Returns_Version_Not_Null_Or_Empty(
        CalculationDtoMapper sut)
    {
        // Arrange
        var batch = new CalculationBuilder().Build();

        // Act
        var batchDto = sut.Map(batch);

        // Assert
        batchDto.Version.Should().NotBeNullOrWhiteSpace();
    }
}
