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

using EDI.InboxEvents.Exceptions;
using Energinet.DataHub.Edi.Responses;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.InboxEvents.Mappers;

public class TimeSeriesTypeMapperTests
{
    [Theory]
    [InlineData(Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.Production, TimeSeriesType.Production)]
    [InlineData(Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.TotalConsumption, TimeSeriesType.TotalConsumption)]
    [InlineData(Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.NonProfiledConsumption, TimeSeriesType.NonProfiledConsumption)]
    [InlineData(Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.FlexConsumption, TimeSeriesType.FlexConsumption)]
    [InlineData(Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.NetExchangePerGa, TimeSeriesType.NetExchangePerGa)]
    public void MapTimeSeriesType_WhenCalled_MapsCorrectly(Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType timeSeriesType, TimeSeriesType expected)
    {
        // Act & Assert
        global::EDI.InboxEvents.Mappers.TimeSeriesTypeMapper.MapTimeSeriesTypeFromCalculationsResult(timeSeriesType).Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(TimeSeriesTypes))]
    public void MapTimeSeriesType_WhenCalled_HandlesExpectedType(Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType timeSeriesType)
    {
        // Act
        var actual = () => global::EDI.InboxEvents.Mappers.TimeSeriesTypeMapper.MapTimeSeriesTypeFromCalculationsResult(timeSeriesType);

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
        foreach (var timeSerieType in Enum.GetValues(typeof(Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType)))
        {
            yield return new[] { timeSerieType };
        }
    }

    private static bool IsNotSupportedTimeSeriesType(Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType timeSeriesType)
    {
        return timeSeriesType is Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.GridLoss
            or Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.TempProduction
            or Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.NegativeGridLoss
            or Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.PositiveGridLoss
            or Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.TempFlexConsumption
            or Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.NetExchangePerNeighboringGa;
    }
}
