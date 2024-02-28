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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using FluentAssertions;
using Xunit;
using EnergyResultProduced = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV2;
using TimeSeriesTypeMapper = Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EnergyResultProducedV2.Mappers.TimeSeriesTypeMapper;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.EnergyResultProducedV2.Mappers;

public class TimeSeriesTypeMapperTests
{
    [Theory]
    [InlineAutoMoqData(TimeSeriesType.Production, EnergyResultProduced.Types.TimeSeriesType.Production)]
    [InlineAutoMoqData(TimeSeriesType.FlexConsumption, EnergyResultProduced.Types.TimeSeriesType.FlexConsumption)]
    [InlineAutoMoqData(TimeSeriesType.NonProfiledConsumption, EnergyResultProduced.Types.TimeSeriesType.NonProfiledConsumption)]
    [InlineAutoMoqData(TimeSeriesType.NetExchangePerGa, EnergyResultProduced.Types.TimeSeriesType.NetExchangePerGa)]
    [InlineAutoMoqData(TimeSeriesType.NetExchangePerNeighboringGa, EnergyResultProduced.Types.TimeSeriesType.NetExchangePerNeighboringGa)]
    [InlineAutoMoqData(TimeSeriesType.GridLoss, EnergyResultProduced.Types.TimeSeriesType.GridLoss)]
    [InlineAutoMoqData(TimeSeriesType.NegativeGridLoss, EnergyResultProduced.Types.TimeSeriesType.NegativeGridLoss)]
    [InlineAutoMoqData(TimeSeriesType.PositiveGridLoss, EnergyResultProduced.Types.TimeSeriesType.PositiveGridLoss)]
    [InlineAutoMoqData(TimeSeriesType.TotalConsumption, EnergyResultProduced.Types.TimeSeriesType.TotalConsumption)]
    public void MapTimeSeriesType_WhenCalled_MapsCorrectly(TimeSeriesType timeSeriesType, EnergyResultProduced.Types.TimeSeriesType expected)
    {
        // Act & Assert
        var actual = TimeSeriesTypeMapper.MapTimeSeriesType(timeSeriesType);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void MapTimeSeriesType_MapsAnyValidValue()
    {
        foreach (var timeSeriesType in Enum.GetValues(typeof(TimeSeriesType)).Cast<TimeSeriesType>())
        {
            // Act
            var actual = TimeSeriesTypeMapper.MapTimeSeriesType(timeSeriesType);

            // Assert: Is defined (and implicitly that it didn't throw exception)
            Enum.IsDefined(typeof(EnergyResultProduced.Types.TimeSeriesType), actual).Should().BeTrue();
        }
    }

    [Fact]
    public void MapTimeSeriesType_WhenInvalidEnumNumberForTimeSeriesType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (TimeSeriesType)99;

        // Act
        var act = () => TimeSeriesTypeMapper.MapTimeSeriesType(invalidValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidValue);
    }
}
