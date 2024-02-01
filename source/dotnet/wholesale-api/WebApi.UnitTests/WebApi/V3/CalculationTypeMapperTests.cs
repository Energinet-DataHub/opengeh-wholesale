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
using Energinet.DataHub.Wholesale.WebApi.V3.Calculation;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.WebApi.V3;

public class CalculationTypeMapperTests
{
    [Theory]
    [InlineAutoMoqData(Energinet.DataHub.Wholesale.Common.Interfaces.Models.CalculationType.BalanceFixing, CalculationType.BalanceFixing)]
    [InlineAutoMoqData(Energinet.DataHub.Wholesale.Common.Interfaces.Models.CalculationType.Aggregation, CalculationType.Aggregation)]
    public void Map_ReturnsExpectedType(Energinet.DataHub.Wholesale.Common.Interfaces.Models.CalculationType source, CalculationType expected)
    {
        var actual = CalculationTypeMapper.Map(source);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineAutoMoqData(CalculationType.BalanceFixing, Energinet.DataHub.Wholesale.Common.Interfaces.Models.CalculationType.BalanceFixing)]
    [InlineAutoMoqData(CalculationType.Aggregation, Energinet.DataHub.Wholesale.Common.Interfaces.Models.CalculationType.Aggregation)]
    public void MapProcessType_ReturnsExpectedType(CalculationType source, Energinet.DataHub.Wholesale.Common.Interfaces.Models.CalculationType expected)
    {
        var actual = CalculationTypeMapper.Map(source);
        actual.Should().Be(expected);
    }

    [Fact]
    public void Map_WhenInvalidEnumNumberForProcessType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (Energinet.DataHub.Wholesale.Common.Interfaces.Models.CalculationType)99;

        // Act
        var act = () => CalculationTypeMapper.Map(invalidValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidValue);
    }

    [Fact]
    public void Map_WhenInvalidEnumNumberForV3ProcessType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (CalculationType)99;

        // Act
        var act = () => CalculationTypeMapper.Map(invalidValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidValue);
    }
}
