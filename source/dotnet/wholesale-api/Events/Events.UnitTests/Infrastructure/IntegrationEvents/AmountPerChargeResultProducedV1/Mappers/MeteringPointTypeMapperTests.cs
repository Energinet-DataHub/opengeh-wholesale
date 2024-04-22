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
using EventMeteringPointType = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.MeteringPointType;
using ModelMeteringPointType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.MeteringPointType;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.AmountPerChargeResultProducedV1.Mappers;

public class MeteringPointTypeMapperTests
{
    [Fact]
    public void MapMeteringPointType_WhenCalledWithNull_MapsToUnspecified()
    {
        // Act & Assert
        MeteringPointTypeMapper.MapMeteringPointType(null).Should().Be(EventMeteringPointType.Unspecified);
    }

    [Fact]
    public void MapMeteringPointType_WhenCalledWithValidMeteringTypes_MapsCorrectly()
    {
        // Arrange
        var validTypes = Enum
            .GetValues<ModelMeteringPointType>()
            .Except(new[] { ModelMeteringPointType.Exchange });

        foreach (var expectedType in validTypes)
        {
            // Act
            var actualType = MeteringPointTypeMapper.MapMeteringPointType(expectedType);

            // Assert
            actualType.ToString().Should().Be(expectedType.ToString());
        }
    }

    [Fact]
    public void MapMeteringPointType_WhenInvalidEnumNumber_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (ModelMeteringPointType)99;

        // Act
        var act = () => MeteringPointTypeMapper.MapMeteringPointType(invalidValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidValue);
    }
}
