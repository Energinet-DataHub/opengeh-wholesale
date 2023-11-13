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

using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.AmountPerChargeResultProducedV1.Mappers;
using FluentAssertions;
using Xunit;
using EventChargeType = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.ChargeType;
using ModelChargeType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.ChargeType;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.Mappers.AmountPerChargeResultProducedV1;

public class ChargeTypeMapperTests
{
    [Theory]
    [InlineData(ModelChargeType.Tariff, EventChargeType.Tariff)]
    [InlineData(ModelChargeType.Fee, EventChargeType.Fee)]
    [InlineData(ModelChargeType.Subscription, EventChargeType.Subscription)]
    public void MapChargeType_WhenCalled_MapsCorrectly(ModelChargeType chargeType, EventChargeType expected)
    {
        // Act & Assert
        ChargeTypeMapper.MapChargeType(chargeType).Should().Be(expected);
    }

    [Fact]
    public void MapChargeType_MapsAnyValidValue()
    {
        foreach (var chargeType in Enum.GetValues(typeof(ModelChargeType)).Cast<ModelChargeType>())
        {
            // Act
            var actual = ChargeTypeMapper.MapChargeType(chargeType);

            // Assert: Is defined (and implicitly that it didn't throw exception)
            Enum.IsDefined(typeof(EventChargeType), actual).Should().BeTrue();
        }
    }

    [Fact]
    public void MapChargeType_WhenInvalidEnumNumber_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (ModelChargeType)99;

        // Act
        var act = () => ChargeTypeMapper.MapChargeType(invalidValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidValue);
    }
}
