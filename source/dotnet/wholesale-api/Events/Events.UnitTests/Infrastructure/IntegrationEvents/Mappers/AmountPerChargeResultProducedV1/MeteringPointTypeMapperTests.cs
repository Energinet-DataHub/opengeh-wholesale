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
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers.AmountPerChargeResultProducedV1;
using FluentAssertions;
using Xunit;
using EventMeteringPointType = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.MeteringPointType;
using ModelMeteringPointType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.MeteringPointType;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.Mappers.AmountPerChargeResultProducedV1;

public class MeteringPointTypeMapperTests
{
    [Theory]
    [InlineAutoMoqData(ModelMeteringPointType.Consumption, EventMeteringPointType.Consumption)]
    [InlineAutoMoqData(ModelMeteringPointType.Production, EventMeteringPointType.Production)]
    [InlineAutoMoqData(null!, EventMeteringPointType.Unspecified)]
    public void MapMeteringPointType_WhenCalledWithValidMeteringTypes_MapsCorrectly(ModelMeteringPointType? meteringPointType, EventMeteringPointType expected)
    {
        // Act & Assert
        MeteringPointTypeMapper.MapMeteringPointType(meteringPointType).Should().Be(expected);
    }

    [Fact]
    public void MapProcessType_WhenInvalidEnumNumberFor_ThrowsArgumentOutOfRangeException()
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
