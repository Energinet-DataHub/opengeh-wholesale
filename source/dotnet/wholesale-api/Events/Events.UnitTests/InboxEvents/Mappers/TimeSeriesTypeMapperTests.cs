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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Events.Infrastructure.InboxEvents.Exceptions;
using Energinet.DataHub.Wholesale.Events.Infrastructure.InboxEvents.Mappers;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.InboxEvents.Mappers;

public class TimeSeriesTypeMapperTests
{
    [Theory]
    [InlineData(TimeSeriesType.Production, Edi.Responses.TimeSeriesType.Production)]
    [InlineData(TimeSeriesType.TotalConsumption, Edi.Responses.TimeSeriesType.TotalConsumption)]
    [InlineData(TimeSeriesType.NonProfiledConsumption, Edi.Responses.TimeSeriesType.NonProfiledConsumption)]
    [InlineData(TimeSeriesType.FlexConsumption, Edi.Responses.TimeSeriesType.FlexConsumption)]
    [InlineData(TimeSeriesType.NetExchangePerGa, Edi.Responses.TimeSeriesType.NetExchangePerGa)]
    public void MapTimeSeriesType_WhenCalled_MapsCorrectly(TimeSeriesType timeSeriesType, Edi.Responses.TimeSeriesType expected)
    {
        // Act & Assert
        TimeSeriesTypeMapper.MapTimeSeriesType(timeSeriesType).Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(TimeSeriesTypes))]
    public void MapTimeSeriesType_WhenCalled_HandlesExpectedType(TimeSeriesType timeSeriesType)
    {
        // Act
        var actual = () => TimeSeriesTypeMapper.MapTimeSeriesType(timeSeriesType);

        // Assert
        if (IsNotSupportedTimeSeriesType(timeSeriesType))
        {
            actual.Should()
                .Throw<NotSupportedTimeSeriesTypeException>();
        }
        else
        {
            actual.Should().NotThrow();
        }
    }

    public static IEnumerable<object[]> TimeSeriesTypes()
    {
        foreach (var timeSerieType in Enum.GetValues(typeof(TimeSeriesType)))
        {
            yield return new[] { timeSerieType };
        }
    }

    private static bool IsNotSupportedTimeSeriesType(TimeSeriesType timeSeriesType)
    {
        return timeSeriesType is TimeSeriesType.GridLoss
            or TimeSeriesType.TempProduction
            or TimeSeriesType.NegativeGridLoss
            or TimeSeriesType.PositiveGridLoss
            or TimeSeriesType.TempFlexConsumption
            or TimeSeriesType.NetExchangePerNeighboringGa;
    }
}
