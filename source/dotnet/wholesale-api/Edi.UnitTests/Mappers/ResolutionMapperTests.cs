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
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Mappers;

public class ResolutionMapperTests
{
    [Theory]
    [InlineData(DataHubNames.Resolution.Monthly, CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution.Month)]
    [InlineData(DataHubNames.Resolution.Hourly, CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution.Hour)]
    [InlineData(DataHubNames.Resolution.Daily, CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution.Day)]
    public void Map_WhenValid_ReturnsExpectedChargeType(string resolution, Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution expectedResult)
    {
        // Act
        var actual = ResolutionMapper.Map(resolution);

        // Assert
        actual.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("InvalidResolution")]
    public void Map_WhenInvalid_ThrowsArgumentOutOfRangeException(string resolution)
    {
        // Act
        var act = () => ResolutionMapper.Map(resolution);

        // Assert
        act.Should().ThrowExactly<ArgumentOutOfRangeException>().And.ActualValue.Should().Be(resolution);
    }
}
