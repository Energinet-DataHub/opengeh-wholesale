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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.GridLossResultProducedV1.Mappers;
using FluentAssertions;
using Xunit;
using MeteringPointType = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.GridLossResultProducedV1.Types.MeteringPointType;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.GridLossResultProducedV1.Mappers;

public class GridLossMeteringPointTypeMapperTests
{
    [Theory]
    [InlineData(TimeSeriesType.NegativeGridLoss, MeteringPointType.Consumption)]
    [InlineData(TimeSeriesType.PositiveGridLoss, MeteringPointType.Production)]
    public void MapFromTimeSeriesType_WhenCalledWithValidMeteringTypes_MapsCorrectly(TimeSeriesType timeSeriesType, MeteringPointType expected)
    {
        // Act & Assert
        GridLossMeteringPointTypeMapper.MapFromTimeSeriesType(timeSeriesType).Should().Be(expected);
    }

    [Fact]
    public void MapFromTimeSeriesType_WhenUnexpectedTimeSeriesType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var unexpectedValues = Enum.GetValues(typeof(TimeSeriesType)).Cast<TimeSeriesType>().Where(t => t != TimeSeriesType.NegativeGridLoss && t != TimeSeriesType.PositiveGridLoss)

        foreach (var timeSeriesType in unexpectedValues)
        {
            // Act
            var act = () => GridLossMeteringPointTypeMapper.MapFromTimeSeriesType(timeSeriesType);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .And.ActualValue.Should().Be(timeSeriesType);
        }
    }
}
