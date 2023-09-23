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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers.EnergyResultProducedV1;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.Mappers.EnergyResultProducedV1;

public class TimeSeriesTypeMapperTests
{
    [Theory]
    [InlineAutoMoqData(TimeSeriesType.Production, Contracts.IntegrationEvents.TimeSeriesType.Production)]
    [InlineAutoMoqData(TimeSeriesType.FlexConsumption, Contracts.IntegrationEvents.TimeSeriesType.FlexConsumption)]
    [InlineAutoMoqData(TimeSeriesType.NonProfiledConsumption, Contracts.IntegrationEvents.TimeSeriesType.NonProfiledConsumption)]
    [InlineAutoMoqData(TimeSeriesType.NetExchangePerGa, Contracts.IntegrationEvents.TimeSeriesType.NetExchangePerGa)]
    [InlineAutoMoqData(TimeSeriesType.NetExchangePerNeighboringGa, Contracts.IntegrationEvents.TimeSeriesType.NetExchangePerNeighboringGa)]
    [InlineAutoMoqData(TimeSeriesType.GridLoss, Contracts.IntegrationEvents.TimeSeriesType.GridLoss)]
    [InlineAutoMoqData(TimeSeriesType.NegativeGridLoss, Contracts.IntegrationEvents.TimeSeriesType.NegativeGridLoss)]
    [InlineAutoMoqData(TimeSeriesType.PositiveGridLoss, Contracts.IntegrationEvents.TimeSeriesType.PositiveGridLoss)]
    [InlineAutoMoqData(TimeSeriesType.TotalConsumption, Contracts.IntegrationEvents.TimeSeriesType.TotalConsumption)]
    public void MapTimeSeriesType_WhenCalled_MapsCorrectly(TimeSeriesType timeSeriesType, Wholesale.Contracts.IntegrationEvents.TimeSeriesType expected)
    {
        // Act & Assert
        TimeSeriesTypeMapper.MapTimeSeriesType(timeSeriesType).Should().Be(expected);
    }

    [Fact]
    public void MapTimeSeriesType_MapsAnyValidValue()
    {
        foreach (var timeSeriesType in Enum.GetValues(typeof(TimeSeriesType)).Cast<TimeSeriesType>())
        {
            // Act
            var actual = TimeSeriesTypeMapper.MapTimeSeriesType(timeSeriesType);

            // Assert: Is defined (and implicitly that it didn't throw exception)
            Enum.IsDefined(typeof(Contracts.Events.TimeSeriesType), actual).Should().BeTrue();
        }
    }
}
