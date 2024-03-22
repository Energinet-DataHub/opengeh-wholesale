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

using Energinet.DataHub.Wholesale.Edi.Contracts;
using Energinet.DataHub.Wholesale.Edi.Mappers;
using Energinet.DataHub.Wholesale.Edi.Models;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Mappers;

public class TimeSeriesTypeMapperTests
{
    [Theory]
    [InlineData("Consumption", "", TimeSeriesType.TotalConsumption)]
    [InlineData("Consumption", null, TimeSeriesType.TotalConsumption)]
    [InlineData("Consumption", DataHubNames.SettlementMethod.NonProfiled, TimeSeriesType.NonProfiledConsumption)]
    [InlineData("Consumption", DataHubNames.SettlementMethod.Flex, TimeSeriesType.FlexConsumption)]
    [InlineData("Production", null, TimeSeriesType.Production)]
    [InlineData("Exchange", null, TimeSeriesType.NetExchangePerGa)]
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
        var invalidMeteringPointType = "invalid-metering-point-type-value";

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
        var invalidSettlementMethod = "invalid-settlement-method-value";

        // Act
        var act = () => TimeSeriesTypeMapper.MapTimeSeriesType("Consumption", invalidSettlementMethod);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidSettlementMethod);
    }
}
