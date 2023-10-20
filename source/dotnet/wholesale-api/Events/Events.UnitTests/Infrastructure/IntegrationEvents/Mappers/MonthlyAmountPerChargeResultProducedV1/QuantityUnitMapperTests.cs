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

using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers.MonthlyAmountPerChargeResultProducedV1;
using FluentAssertions;
using Xunit;
using EventQuantityUnit = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.MonthlyAmountPerChargeResultProducedV1.Types.QuantityUnit;
using ModelQuantityUnit = Energinet.DataHub.Wholesale.Common.Models.QuantityUnit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.Mappers.MonthlyAmountPerChargeResultProducedV1;

public class QuantityUnitMapperTests
{
    [Theory]
    [InlineData(ModelQuantityUnit.Kwh, EventQuantityUnit.Kwh)]
    [InlineData(ModelQuantityUnit.Pieces, EventQuantityUnit.Pieces)]
    public void MapQuantityUnit_WhenCalled_MapsCorrectly(ModelQuantityUnit quantityUnit, EventQuantityUnit expected)
    {
        // Act & Assert
        QuantityUnitMapper.MapQuantityUnit(quantityUnit).Should().Be(expected);
    }

    [Fact]
    public void MapQuantityUnit_MapsAnyValidValue()
    {
        foreach (var quantityUnit in Enum.GetValues(typeof(ModelQuantityUnit)).Cast<ModelQuantityUnit>())
        {
            // Act
            var actual = QuantityUnitMapper.MapQuantityUnit(quantityUnit);

            // Assert: Is defined (and implicitly that it didn't throw exception)
            Enum.IsDefined(typeof(EventQuantityUnit), actual).Should().BeTrue();
        }
    }

    [Fact]
    public void MapQuantityUnit_WhenInvalidEnumNumber_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (ModelQuantityUnit)99;

        // Act
        var act = () => QuantityUnitMapper.MapQuantityUnit(invalidValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidValue);
    }
}
