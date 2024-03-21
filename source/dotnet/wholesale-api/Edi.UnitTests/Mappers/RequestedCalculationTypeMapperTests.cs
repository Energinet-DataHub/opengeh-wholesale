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
using Energinet.DataHub.Wholesale.Edi.Models;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Mappers;

public class RequestedCalculationTypeMapperTests
{
    [Theory]
    [InlineData(DomainNames.BusinessReason.BalanceFixing, null, RequestedCalculationType.BalanceFixing)]
    [InlineData(DomainNames.BusinessReason.PreliminaryAggregation, null, RequestedCalculationType.PreliminaryAggregation)]
    [InlineData(DomainNames.BusinessReason.WholesaleFixing, null, RequestedCalculationType.WholesaleFixing)]
    [InlineData(DomainNames.BusinessReason.Correction, SettlementSeriesVersion.FirstCorrection, RequestedCalculationType.FirstCorrection)]
    [InlineData(DomainNames.BusinessReason.Correction, SettlementSeriesVersion.SecondCorrection, RequestedCalculationType.SecondCorrection)]
    [InlineData(DomainNames.BusinessReason.Correction, SettlementSeriesVersion.ThirdCorrection, RequestedCalculationType.ThirdCorrection)]
    [InlineData(DomainNames.BusinessReason.Correction, null, RequestedCalculationType.LatestCorrection)]
    public void ToRequestedCalculationType_WhenValidBusinessReasonAndSettlementSeriesVersion_ReturnsExpectedType(string businessReason, string? settlementSeriesVersion, RequestedCalculationType expectedType)
    {
        // Act
        var actualType = RequestedCalculationTypeMapper.ToRequestedCalculationType(businessReason, settlementSeriesVersion);

        // Assert
        actualType.Should().Be(expectedType);
    }

    [Theory]
    [InlineData(DomainNames.BusinessReason.BalanceFixing, SettlementSeriesVersion.FirstCorrection)]
    [InlineData(DomainNames.BusinessReason.PreliminaryAggregation, "random-string")]
    [InlineData(DomainNames.BusinessReason.WholesaleFixing, SettlementSeriesVersion.FirstCorrection)]
    [InlineData(DomainNames.BusinessReason.Correction, "")]
    [InlineData(DomainNames.BusinessReason.Correction, "random-string")]
    [InlineData("", "")]
    [InlineData("random-string", "")]
    [InlineData("random-string", SettlementSeriesVersion.FirstCorrection)]
    public void ToRequestedCalculationType_WhenInvalidBusinessReasonAndSettlementSeriesVersionCombination_ThrowsArgumentOutOfRangeException(string businessReason, string? settlementSeriesVersion)
    {
        // Act
        var act = () => RequestedCalculationTypeMapper.ToRequestedCalculationType(businessReason, settlementSeriesVersion);

        // Assert
        act.Should().ThrowExactly<ArgumentOutOfRangeException>().And.ActualValue.Should().Be(settlementSeriesVersion);
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("random-string", null)]
    public void ToRequestedCalculationType_WhenInvalidBusinessReason_ThrowsArgumentOutOfRangeException(string businessReason, string? settlementSeriesVersion)
    {
        // Act
        var act = () => RequestedCalculationTypeMapper.ToRequestedCalculationType(businessReason, settlementSeriesVersion);

        // Assert
        act.Should().ThrowExactly<ArgumentOutOfRangeException>().And.ActualValue.Should().Be(businessReason);
    }
}
