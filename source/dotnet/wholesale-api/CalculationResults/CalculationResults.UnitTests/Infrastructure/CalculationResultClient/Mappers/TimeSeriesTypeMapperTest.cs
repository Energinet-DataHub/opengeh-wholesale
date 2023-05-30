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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.CalculationResultClient.Mappers;

[UnitTest]
public class TimeSeriesTypeMapperTests
{
    [Theory]
    [InlineData(TimeSeriesType.FlexConsumption, DeltaTableTimeSeriesType.FlexConsumption)]
    [InlineData(TimeSeriesType.Production, DeltaTableTimeSeriesType.Production)]
    [InlineData(TimeSeriesType.NonProfiledConsumption, DeltaTableTimeSeriesType.NonProfiledConsumption)]
    [InlineData(TimeSeriesType.NetExchangePerGa, DeltaTableTimeSeriesType.NetExchangePerGridArea)]
    public void ToDeltaTableValue_ReturnsExpectedString(TimeSeriesType type, string expected)
    {
        // Act
        var actual = TimeSeriesTypeMapper.ToDeltaTableValue(type);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(DeltaTableTimeSeriesType.FlexConsumption, TimeSeriesType.FlexConsumption)]
    [InlineData(DeltaTableTimeSeriesType.Production, TimeSeriesType.Production)]
    [InlineData(DeltaTableTimeSeriesType.NonProfiledConsumption, TimeSeriesType.NonProfiledConsumption)]
    [InlineData(DeltaTableTimeSeriesType.NetExchangePerGridArea, TimeSeriesType.NetExchangePerGa)]
    public void FromDeltaTableValue_ReturnsExpectedType(string deltaTableValue, TimeSeriesType expected)
    {
        // Act
        var actual = TimeSeriesTypeMapper.FromDeltaTableValue(deltaTableValue);

        // Assert
        actual.Should().Be(expected);
    }
}
