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

using Energinet.DataHub.Wholesale.EDI.Mappers;
using Energinet.DataHub.Wholesale.Edi.Models;
using Energinet.DataHub.Wholesale.EDI.Models;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Mappers;

public class RequestedProcessTypeMapperTests
{
    [Theory]
    [InlineData(BusinessReason.BalanceFixing, null, RequestedProcessType.BalanceFixing)]
    [InlineData(BusinessReason.PreliminaryAggregation, null, RequestedProcessType.PreliminaryAggregation)]
    [InlineData(BusinessReason.WholesaleFixing, null, RequestedProcessType.WholesaleFixing)]
    [InlineData(BusinessReason.Correction, SettlementSeriesVersion.FirstCorrection, RequestedProcessType.FirstCorrection)]
    [InlineData(BusinessReason.Correction, SettlementSeriesVersion.SecondCorrection, RequestedProcessType.SecondCorrection)]
    [InlineData(BusinessReason.Correction, SettlementSeriesVersion.ThirdCorrection, RequestedProcessType.ThirdCorrection)]
    [InlineData(BusinessReason.Correction, null, RequestedProcessType.LatestCorrection)]
    public void ToRequestedProcessType_WhenValidBusinessReasonAndSettlementSeriesVersion_ReturnsExpectedType(string businessReason, string? settlementSeriesVersion, RequestedProcessType expectedType)
    {
        // Act
        var actualType = RequestedProcessTypeMapper.ToRequestedProcessType(businessReason, settlementSeriesVersion);

        // Assert
        actualType.Should().Be(expectedType);
    }

    [Theory]
    [InlineData(BusinessReason.BalanceFixing, SettlementSeriesVersion.FirstCorrection)]
    [InlineData(BusinessReason.PreliminaryAggregation, "random-string")]
    [InlineData(BusinessReason.WholesaleFixing, SettlementSeriesVersion.FirstCorrection)]
    [InlineData("", "")]
    [InlineData("random-string", "")]
    [InlineData("random-string", SettlementSeriesVersion.FirstCorrection)]
    [InlineData(BusinessReason.Correction, "")]
    [InlineData(BusinessReason.Correction, "random-string")]
    public void ToRequestedProcessType_WhenInvalidSettlementSeriesVersion_ThrowsArgumentOutOfRangeException(string businessReason, string? settlementSeriesVersion)
    {
        // Act
        var act = () => RequestedProcessTypeMapper.ToRequestedProcessType(businessReason, settlementSeriesVersion);

        // Assert
        act.Should().ThrowExactly<ArgumentOutOfRangeException>().And.ActualValue.Should().Be(settlementSeriesVersion);
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("random-string", null)]
    public void ToRequestedProcessType_WhenInvalidBusinessReason_ThrowsArgumentOutOfRangeException(string businessReason, string? settlementSeriesVersion)
    {
        // Act
        var act = () => RequestedProcessTypeMapper.ToRequestedProcessType(businessReason, settlementSeriesVersion);

        // Assert
        act.Should().ThrowExactly<ArgumentOutOfRangeException>().And.ActualValue.Should().Be(businessReason);
    }
}
