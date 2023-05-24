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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using FluentAssertions;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.DeltaTableContracts;

public class TimeSeriesTypeMapperTests
{
    [Fact]
    public async Task TimeSeriesType_Matches_Contract()
    {
        await using var stream = EmbeddedResources.GetStream("DeltaTableContracts.Contracts.time-series-type.json");
        await ContractComplianceTestHelper.VerifyEnumCompliesWithContractAsync<TimeSeriesType>(stream);
    }

    [Fact]
    public async Task TimeSeriesTypeMapper_MapsAllValidDeltaTableValues()
    {
        // Arrange
        await using var stream = EmbeddedResources.GetStream("DeltaTableContracts.Contracts.time-series-type.json");
        var validDeltaValues = await ContractComplianceTestHelper.GetCodeListValuesAsync(stream);

        foreach (var validDeltaValue in validDeltaValues)
        {
            // Act
            var actual = TimeSeriesTypeMapper.FromDeltaTableValue(validDeltaValue);

            // Assert it's a defined enum value (and not null)
            actual.Should().BeDefined();
        }
    }

    [Theory]
    [InlineAutoMoqData("production", TimeSeriesType.Production)]
    [InlineAutoMoqData("non_profiled_consumption", TimeSeriesType.NonProfiledConsumption)]
    [InlineAutoMoqData("net_exchange_per_neighboring_ga", TimeSeriesType.NetExchangePerNeighboringGa)]
    [InlineAutoMoqData("net_exchange_per_ga", TimeSeriesType.NetExchangePerGa)]
    [InlineAutoMoqData("flex_consumption", TimeSeriesType.FlexConsumption)]
    [InlineAutoMoqData("grid_loss", TimeSeriesType.GridLoss)]
    [InlineAutoMoqData("negative_grid_loss", TimeSeriesType.NegativeGridLoss)]
    [InlineAutoMoqData("positive_grid_loss", TimeSeriesType.PositiveGridLoss)]
    public void TimeSeriesTypeMapper_ReturnsValidTimeSeriesType(string deltaValue, TimeSeriesType? expected)
    {
        // Act
        var actual = TimeSeriesTypeMapper.FromDeltaTableValue(deltaValue);

        // Assert
        actual.Should().Be(expected);
    }
}
