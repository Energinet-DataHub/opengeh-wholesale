﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using FluentAssertions;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.DeltaTableContracts;

public class SettlementMethodMapperTests
{
    [Fact]
    public async Task SettlementMethodMapper_AcceptsAllValidDeltaTableValues()
    {
        // Arrange
        await using var stream = EmbeddedResources.GetStream("DeltaTableContracts.Contracts.Enums.time-series-type.json");
        var validDeltaValues = await ContractComplianceTestHelper.GetCodeListValuesAsync(stream);

        foreach (var validDeltaValue in validDeltaValues)
        {
            // Act & Assert: Does not throw
            SettlementMethodMapper.FromDeltaTableValue(validDeltaValue);
        }
    }

    [Theory]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.Production, null!)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.NonProfiledConsumption, SettlementMethod.NonProfiled)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.NetExchangePerNeighboringGridArea, null!)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.NetExchangePerGridArea, null!)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.FlexConsumption, SettlementMethod.Flex)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.GridLoss, null!)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.NegativeGridLoss, null!)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.PositiveGridLoss, SettlementMethod.Flex)]
    public void SettlementMethodMapper_ReturnsValidSettlementMethod(string deltaValue, SettlementMethod? expected)
    {
        // Act
        var actual = SettlementMethodMapper.FromDeltaTableValue(deltaValue);

        // Assert
        actual.Should().Be(expected);
    }
}
