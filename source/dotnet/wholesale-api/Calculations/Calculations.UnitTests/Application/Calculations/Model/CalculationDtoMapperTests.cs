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
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.Calculations.UnitTests.Infrastructure.CalculationAggregate;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Calculations.UnitTests.Application.Calculations.Model;

public class CalculationDtoMapperTests
{
    [Theory]
    [InlineAutoMoqData]
    public void Map_Returns_CorrectState(
        CalculationDtoMapper sut)
    {
        // Arrange
        var calculation = new CalculationBuilder().WithStateExecuting().Build();

        // Act
        var calculationDto = sut.Map(calculation);

        // Assert
        calculationDto.ExecutionState.Should().Be(CalculationState.Executing);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Map_Returns_CorrectPeriod(
        CalculationDtoMapper sut)
    {
        // Arrange
        var calculation = new CalculationBuilder().Build();

        // Act
        var calculationDto = sut.Map(calculation);

        // Assert
        calculationDto.PeriodStart.Should().Be(calculation.PeriodStart.ToDateTimeOffset());
        calculationDto.PeriodEnd.Should().Be(calculation.PeriodEnd.ToDateTimeOffset());
    }

    [Theory]
    [InlineAutoMoqData]
    public void Map_When_ExecutionTimeIsNotNull_Returns_CorrectExecutionTime(
        CalculationDtoMapper sut)
    {
        // Arrange
        var calculation = new CalculationBuilder().Build();
        calculation.MarkAsExecuting(); // this sets ExecutionTimeStart
        calculation.MarkAsCompleted(calculation.ExecutionTimeStart!.Value.Plus(Duration.FromDays(2))); // this sets ExecutionTimeEnd

        // Act
        var calculationDto = sut.Map(calculation);

        // Assert
        calculationDto.ExecutionTimeStart.Should().Be(calculation.ExecutionTimeStart.Value.ToDateTimeOffset());
        calculationDto.ExecutionTimeEnd.Should().Be(calculation.ExecutionTimeEnd!.Value.ToDateTimeOffset());
    }

    [Theory]
    [InlineAutoMoqData]
    public void Map_CalculationNumber_Equals_RunId(
        CalculationDtoMapper sut)
    {
        // Arrange
        var calculation = new CalculationBuilder().Build();
        var expectedRunId = new CalculationJobId(111);
        calculation.MarkAsSubmitted(expectedRunId);

        // Act
        var calculationDto = sut.Map(calculation);

        // Assert
        calculationDto.RunId.Should().Be(expectedRunId.Id);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Map_When_NoRunIdIsNull_Then_CalculationNumberIsNull(
        CalculationDtoMapper sut)
    {
        // Arrange
        var calculation = new CalculationBuilder().Build();

        // Act
        var calculationDto = sut.Map(calculation);

        // Assert
        calculationDto.RunId.Should().Be(null);
    }

    [Theory]
    [AutoMoqData]
    public void Map_Returns_Version_Greater_Than_Zero(
        CalculationDtoMapper sut)
    {
        // Arrange
        var calculation = new CalculationBuilder().Build();

        // Act
        var calculationDto = sut.Map(calculation);

        // Assert
        calculationDto.Version.Should().BeGreaterThan(0);
    }
}
