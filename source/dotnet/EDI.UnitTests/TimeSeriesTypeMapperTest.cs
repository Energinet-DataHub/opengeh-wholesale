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

using Energinet.DataHub.Wholesale.EDI.UnitTests.Mappers;
using Energinet.DataHub.Wholesale.EDI.UnitTests.Models;
using FluentAssertions;
using Xunit;
using CalculationTimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests;

public class TimeSeriesTypeMapperTests
{
    [Theory]
    [InlineData(TimeSeriesType.Production, CalculationTimeSeriesType.Production)]
    [InlineData(TimeSeriesType.FlexConsumption, CalculationTimeSeriesType.FlexConsumption)]
    [InlineData(TimeSeriesType.TotalConsumption, CalculationTimeSeriesType.TotalConsumption)]
    [InlineData(TimeSeriesType.NetExchangePerGa, CalculationTimeSeriesType.NetExchangePerGa)]
    [InlineData(TimeSeriesType.NonProfiledConsumption, CalculationTimeSeriesType.NonProfiledConsumption)]
    public void ToCalculationTimeSerieType_ReturnsExpectedType(TimeSeriesType type, CalculationTimeSeriesType expected)
    {
        // Act
        var actual = TimeSeriesTypeMapper.MapTimeSeriesTypeFromEdi(type);

        // Assert
        actual.Should().Be(expected);
    }
}
