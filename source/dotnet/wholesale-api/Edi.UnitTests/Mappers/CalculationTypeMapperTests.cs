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

using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Edi.Mappers;
using Energinet.DataHub.Wholesale.Edi.Models;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Mappers;

public class CalculationTypeMapperTests
{
    [Theory]
    [InlineData(RequestedCalculationType.BalanceFixing, CalculationType.BalanceFixing)]
    [InlineData(RequestedCalculationType.PreliminaryAggregation, CalculationType.Aggregation)]
    [InlineData(RequestedCalculationType.WholesaleFixing, CalculationType.WholesaleFixing)]
    [InlineData(RequestedCalculationType.FirstCorrection, CalculationType.FirstCorrectionSettlement)]
    [InlineData(RequestedCalculationType.SecondCorrection, CalculationType.SecondCorrectionSettlement)]
    [InlineData(RequestedCalculationType.ThirdCorrection, CalculationType.ThirdCorrectionSettlement)]
    public void FromRequestedCalculationType_WhenValidRequestedCalculationType_ReturnsExpectedCalculationType(RequestedCalculationType requestedCalculationType, CalculationType expectedResult)
    {
        // Act
        var actualCalculationType = CalculationTypeMapper.FromRequestedCalculationType(requestedCalculationType);

        // Assert
        actualCalculationType.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(RequestedCalculationType.LatestCorrection)]
    [InlineData((RequestedCalculationType)999999)]
    public void FromRequestedCalculationType_WhenInvalidRequestedCalculationType_ThrowsArgumentOutOfRangeException(RequestedCalculationType requestedCalculationType)
    {
        // Act
        var act = () => CalculationTypeMapper.FromRequestedCalculationType(requestedCalculationType);

        // Assert
        act.Should().ThrowExactly<ArgumentOutOfRangeException>().And.ActualValue.Should().Be(requestedCalculationType);
    }
}
