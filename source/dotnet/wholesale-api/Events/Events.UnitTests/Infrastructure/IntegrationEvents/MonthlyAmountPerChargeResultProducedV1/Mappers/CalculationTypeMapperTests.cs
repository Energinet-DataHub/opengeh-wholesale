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

using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1.Mappers;
using FluentAssertions;
using Xunit;
using EventCalculationType = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1.Types.CalculationType;
using ModelCalculationType = Energinet.DataHub.Wholesale.Common.Interfaces.Models.CalculationType;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1.Mappers;

public class CalculationTypeMapperTests
{
    [Theory]
    [InlineData(ModelCalculationType.WholesaleFixing, EventCalculationType.WholesaleFixing)]
    [InlineData(ModelCalculationType.FirstCorrectionSettlement, EventCalculationType.FirstCorrectionSettlement)]
    [InlineData(ModelCalculationType.SecondCorrectionSettlement, EventCalculationType.SecondCorrectionSettlement)]
    [InlineData(ModelCalculationType.ThirdCorrectionSettlement, EventCalculationType.ThirdCorrectionSettlement)]
    public void MapCalculationType_WhenCalled_MapsCorrectly(ModelCalculationType calculationType, EventCalculationType expected)
    {
        // Act & Assert
        CalculationTypeMapper.MapCalculationType(calculationType).Should().Be(expected);
    }

    [Theory]
    [InlineData(ModelCalculationType.Aggregation)]
    [InlineData(ModelCalculationType.BalanceFixing)]
    public void MapCalculationType_WhenCalledWithUnexpectedCalculationType_ThrowsArgumentOutOfRangeException(ModelCalculationType calculationType)
    {
        // Act
        var act = () => CalculationTypeMapper.MapCalculationType(calculationType);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(calculationType);
    }

    [Fact]
    public void MapCalculationType_WhenCalledWithAnyValueExceptAggregationOrBalanceFixing_MapsValue()
    {
        foreach (var calculationType in Enum.GetValues(typeof(ModelCalculationType)).Cast<ModelCalculationType>())
        {
            // Arrange
            if (calculationType is ModelCalculationType.Aggregation or ModelCalculationType.BalanceFixing)
                continue;

            // Act
            var actual = CalculationTypeMapper.MapCalculationType(calculationType);

            // Assert: Is defined (and implicitly that it didn't throw exception)
            Enum.IsDefined(typeof(EventCalculationType), actual).Should().BeTrue();
        }
    }

    [Fact]
    public void MapCalculationType_WhenInvalidEnumNumber_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (ModelCalculationType)99;

        // Act
        var act = () => CalculationTypeMapper.MapCalculationType(invalidValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidValue);
    }
}
