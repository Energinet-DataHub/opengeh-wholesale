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

using Energinet.DataHub.Wholesale.EDI.Mappers;
using Energinet.DataHub.Wholesale.Edi.Models;
using Energinet.DataHub.Wholesale.EDI.Models;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Mappers;

public class TimeSeriesTypeMapperTests
{
    [Theory]
    [InlineData("E17", "", TimeSeriesType.TotalConsumption)]
    [InlineData("E17", null, TimeSeriesType.TotalConsumption)]
    [InlineData("E17", SettlementMethodType.NonProfiled, TimeSeriesType.NonProfiledConsumption)]
    [InlineData("E17", SettlementMethodType.Flex, TimeSeriesType.FlexConsumption)]
    [InlineData("E18", null, TimeSeriesType.Production)]
    [InlineData("E20", null, TimeSeriesType.NetExchangePerGa)]
    public void MapTimeSeriesType_WhenValidMeteringPointTypeAndSettlementMethod_ReturnsExpectedType(
        string meteringPointType,
        string? settlementMethod,
        TimeSeriesType expectedType)
    {
        // Act
        var actualType = TimeSeriesTypeMapper.MapTimeSeriesType(meteringPointType, settlementMethod);

        // Assert
        actualType.Should().Be(expectedType);
    }

    [Fact]
    public void MapTimeSeriesType_WhenInvalidMeteringPointType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidMeteringPointType = Guid.NewGuid().ToString();

        // Act
        var act = () => TimeSeriesTypeMapper.MapTimeSeriesType(invalidMeteringPointType, null);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidMeteringPointType);
    }

    [Fact]
    public void MapTimeSeriesType_WhenValidMeteringPointTypeAndInvalidSettlementMethod_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidSettlementMethod = Guid.NewGuid().ToString();

        // Act
        var act = () => TimeSeriesTypeMapper.MapTimeSeriesType("E17", invalidSettlementMethod);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidSettlementMethod);
    }
}
