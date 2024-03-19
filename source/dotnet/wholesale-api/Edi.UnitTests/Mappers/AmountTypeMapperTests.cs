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

using Energinet.DataHub.Wholesale.Edi.Mappers;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Mappers;

public class AmountTypeMapperTests
{
    [Theory]
    [InlineData(CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution.Month, CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.AmountType.MonthlyAmountPerCharge)]
    [InlineData(null, CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.AmountType.AmountPerCharge)]
    public void Map_WhenValid_ReturnsExpectedChargeType(CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution? resolution, Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.AmountType expectedResult)
    {
        // Act
        var actual = AmountTypeMapper.Map(resolution);

        // Assert
        actual.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData((CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution)int.MinValue)]
    [InlineData(CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution.Day)]
    [InlineData(CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution.Hour)]
    public void Map_WhenInvalid_ThrowsArgumentOutOfRangeException(CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution? resolution)
    {
        // Act
        var act = () => AmountTypeMapper.Map(resolution);

        // Assert
        act.Should().ThrowExactly<ArgumentOutOfRangeException>().And.ActualValue.Should().Be(resolution);
    }
}
