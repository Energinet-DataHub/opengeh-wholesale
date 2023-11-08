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

using Energinet.DataHub.Wholesale.Common.Models;
using Energinet.DataHub.Wholesale.Edi.Mappers;
using Energinet.DataHub.Wholesale.Edi.Models;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Mappers;

public class ProcessTypeMapperTests
{
    [Theory]
    [InlineData(RequestedProcessType.BalanceFixing, ProcessType.BalanceFixing)]
    [InlineData(RequestedProcessType.PreliminaryAggregation, ProcessType.Aggregation)]
    [InlineData(RequestedProcessType.WholesaleFixing, ProcessType.WholesaleFixing)]
    [InlineData(RequestedProcessType.FirstCorrection, ProcessType.FirstCorrectionSettlement)]
    [InlineData(RequestedProcessType.SecondCorrection, ProcessType.SecondCorrectionSettlement)]
    [InlineData(RequestedProcessType.ThirdCorrection, ProcessType.ThirdCorrectionSettlement)]
    public void FromRequestedProcessType_WhenValidProcessType_ReturnsExpectedProcessType(RequestedProcessType requestedProcessType, ProcessType expectedResult)
    {
        // Act
        var actualProcessType = ProcessTypeMapper.FromRequestedProcessType(requestedProcessType);

        // Assert
        actualProcessType.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(RequestedProcessType.LatestCorrection)]
    [InlineData((RequestedProcessType)999999)]
    public void FromRequestedProcessType_WhenInvalidValidProcessType_ThrowsArgumentOutOfRangeException(RequestedProcessType requestedProcessType)
    {
        var act = () => ProcessTypeMapper.FromRequestedProcessType(requestedProcessType);

        act.Should().ThrowExactly<ArgumentOutOfRangeException>().And.ActualValue.Should().Be(requestedProcessType);
    }
}
