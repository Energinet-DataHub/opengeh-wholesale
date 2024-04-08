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
    [InlineData(DataHubNames.BusinessReason.BalanceFixing, null, RequestedCalculationType.BalanceFixing)]
    [InlineData(DataHubNames.BusinessReason.PreliminaryAggregation, null, RequestedCalculationType.PreliminaryAggregation)]
    [InlineData(DataHubNames.BusinessReason.WholesaleFixing, null, RequestedCalculationType.WholesaleFixing)]
    [InlineData(DataHubNames.BusinessReason.Correction, DataHubNames.SettlementVersion.FirstCorrection, RequestedCalculationType.FirstCorrection)]
    [InlineData(DataHubNames.BusinessReason.Correction, DataHubNames.SettlementVersion.SecondCorrection, RequestedCalculationType.SecondCorrection)]
    [InlineData(DataHubNames.BusinessReason.Correction, DataHubNames.SettlementVersion.ThirdCorrection, RequestedCalculationType.ThirdCorrection)]
    [InlineData(DataHubNames.BusinessReason.Correction, null, RequestedCalculationType.LatestCorrection)]
    public void ToRequestedCalculationType_WhenValidBusinessReasonAndSettlementVersion_ReturnsExpectedType(string businessReason, string? settlementVersion, RequestedCalculationType expectedType)
    {
        // Act
        var actualType = RequestedCalculationTypeMapper.ToRequestedCalculationType(businessReason, settlementVersion);

        // Assert
        actualType.Should().Be(expectedType);
    }

    [Theory]
    [InlineData(DataHubNames.BusinessReason.BalanceFixing, DataHubNames.SettlementVersion.FirstCorrection)]
    [InlineData(DataHubNames.BusinessReason.PreliminaryAggregation, "random-string")]
    [InlineData(DataHubNames.BusinessReason.WholesaleFixing, DataHubNames.SettlementVersion.FirstCorrection)]
    [InlineData(DataHubNames.BusinessReason.Correction, "")]
    [InlineData(DataHubNames.BusinessReason.Correction, "random-string")]
    [InlineData("", "")]
    [InlineData("random-string", "")]
    [InlineData("random-string", DataHubNames.SettlementVersion.FirstCorrection)]
    public void ToRequestedCalculationType_WhenInvalidBusinessReasonAndSettlementVersionCombination_ThrowsArgumentOutOfRangeException(string businessReason, string? settlementVersion)
    {
        // Act
        var act = () => RequestedCalculationTypeMapper.ToRequestedCalculationType(businessReason, settlementVersion);

        // Assert
        act.Should().ThrowExactly<ArgumentOutOfRangeException>().And.ActualValue.Should().Be(settlementVersion);
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("random-string", null)]
    public void ToRequestedCalculationType_WhenInvalidBusinessReason_ThrowsArgumentOutOfRangeException(string businessReason, string? settlementVersion)
    {
        // Act
        var act = () => RequestedCalculationTypeMapper.ToRequestedCalculationType(businessReason, settlementVersion);

        // Assert
        act.Should().ThrowExactly<ArgumentOutOfRangeException>().And.ActualValue.Should().Be(businessReason);
    }
}
