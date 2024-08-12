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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Edi.Mappers;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Mappers;

public class AmountTypeMapperTests
{
    [Theory]
    [InlineData(Resolution.Month, true, new[] { AmountType.MonthlyAmountPerCharge, AmountType.TotalMonthlyAmount })]
    [InlineData(Resolution.Month, false, new[] { AmountType.MonthlyAmountPerCharge })]
    [InlineData(null, false, new[] { AmountType.AmountPerCharge })]
    [InlineData(null, true, new[] { AmountType.AmountPerCharge })]
    public void Map_WhenValid_ReturnsExpectedChargeType(Resolution? resolution, bool includeTotalMonthlyAmount, AmountType[] expectedResults)
    {
        // Act
        var actual = AmountTypeMapper.Map(resolution, includeTotalMonthlyAmount);

        // Assert
        actual.Should().BeEquivalentTo(expectedResults);
    }

    [Theory]
    [InlineData((Resolution)int.MinValue, false)]
    [InlineData(Resolution.Day, false)]
    [InlineData(Resolution.Hour, false)]
    [InlineData((Resolution)int.MinValue, true)]
    [InlineData(Resolution.Day, true)]
    [InlineData(Resolution.Hour, true)]
    public void Map_WhenInvalid_ThrowsArgumentOutOfRangeException(Resolution? resolution, bool includeTotalMonthlyAmount)
    {
        // Act
        var act = () => AmountTypeMapper.Map(resolution, includeTotalMonthlyAmount);

        // Assert
        act.Should().ThrowExactly<ArgumentOutOfRangeException>().And.ActualValue.Should().Be(resolution);
    }
}
