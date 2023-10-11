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

using Energinet.DataHub.Wholesale.EDI.Exceptions;
using Energinet.DataHub.Wholesale.EDI.Mappers;
using Energinet.DataHub.Wholesale.EDI.Models;
using Xunit;
using CalculationTimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Mappers;

public class CalculationTimeSeriesTypeMapperTests
{
    private readonly List<CalculationTimeSeriesType> _notSupportedCalculationTypes = new()
    {
        CalculationTimeSeriesType.GridLoss,
        CalculationTimeSeriesType.TempProduction,
        CalculationTimeSeriesType.NegativeGridLoss,
        CalculationTimeSeriesType.PositiveGridLoss,
        CalculationTimeSeriesType.TempFlexConsumption,
        CalculationTimeSeriesType.NetExchangePerNeighboringGa,
    };

    [Theory]
    [MemberData(nameof(TimeSeriesTypesEdiModel))]
    public void ToCalculationTimeSerieType_ReturnsExpectedType(TimeSeriesType type)
    {
        // Act
        CalculationTimeSeriesTypeMapper.MapTimeSeriesTypeFromEdi(type);
    }

    [Theory]
    [MemberData(nameof(TimeSeriesTypesCalculationModel))]
    public void ToTimeSeriesType_ReturnsExpectedType(CalculationTimeSeriesType type)
    {
        // Act
        if (_notSupportedCalculationTypes.Contains(type))
        {
            Assert.Throws<NotSupportedTimeSeriesTypeException>(() =>
                CalculationTimeSeriesTypeMapper.MapTimeSeriesTypeFromCalculationsResult(type));
        }
        else
        {
            CalculationTimeSeriesTypeMapper.MapTimeSeriesTypeFromCalculationsResult(type);
        }
    }

    public static IEnumerable<object[]> TimeSeriesTypesEdiModel()
    {
        foreach (var number in Enum.GetValues(typeof(TimeSeriesType)))
        {
            yield return new[] { number };
        }
    }

    public static IEnumerable<object[]> TimeSeriesTypesCalculationModel()
    {
        foreach (var number in Enum.GetValues(typeof(CalculationTimeSeriesType)))
        {
            yield return new[] { number };
        }
    }
}
