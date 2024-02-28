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
using EventSettlementMethod = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.AmountPerChargeResultProducedV1.Types.SettlementMethod;
using ModelSettlementMethod = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.SettlementMethod;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.AmountPerChargeResultProducedV1.Mappers;

public class SettlementMethodMapperTests
{
    [Theory]
    [InlineData(ModelSettlementMethod.Flex, EventSettlementMethod.Flex)]
    [InlineData(ModelSettlementMethod.NonProfiled, EventSettlementMethod.NonProfiled)]
    [InlineData(null, EventSettlementMethod.Unspecified)]
    public void MapSettlementMethod_WhenCalled_MapsCorrectly(ModelSettlementMethod? settlementMethod, EventSettlementMethod expected)
    {
        // Act & Assert
        SettlementMethodMapper.MapSettlementMethod(settlementMethod).Should().Be(expected);
    }

    [Fact]
    public void MapSettlementMethod_MapsAnyValidValue()
    {
        foreach (var settlementMethod in Enum.GetValues(typeof(ModelSettlementMethod)).Cast<ModelSettlementMethod>())
        {
            // Act
            var actual = SettlementMethodMapper.MapSettlementMethod(settlementMethod);

            // Assert: Is defined (and implicitly that it didn't throw exception)
            Enum.IsDefined(typeof(EventSettlementMethod), actual).Should().BeTrue();
        }
    }

    [Fact]
    public void MapSettlementMethod_WhenInvalidEnumNumber_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (ModelSettlementMethod)99;

        // Act
        var act = () => SettlementMethodMapper.MapSettlementMethod(invalidValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidValue);
    }
}
