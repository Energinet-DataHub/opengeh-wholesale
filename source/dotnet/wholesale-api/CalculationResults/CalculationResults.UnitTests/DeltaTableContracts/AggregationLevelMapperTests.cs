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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using FluentAssertions;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.DeltaTableContracts;

public class AggregationLevelMapperTests
{
    [Theory]
    [InlineData(TimeSeriesType.Production, null, null)]
    [InlineData(TimeSeriesType.Production, "es_id", null)]
    [InlineData(TimeSeriesType.Production, "es_id", "brp_id")]
    [InlineData(TimeSeriesType.Production, null, "brp_id")]
    [InlineData(TimeSeriesType.FlexConsumption, null, null)]
    [InlineData(TimeSeriesType.FlexConsumption, "es_id", null)]
    [InlineData(TimeSeriesType.FlexConsumption, "es_id", "brp_id")]
    [InlineData(TimeSeriesType.FlexConsumption, null, "brp_id")]
    [InlineData(TimeSeriesType.NonProfiledConsumption, null, null)]
    [InlineData(TimeSeriesType.NonProfiledConsumption, "es_id", null)]
    [InlineData(TimeSeriesType.NonProfiledConsumption, "es_id", "brp_id")]
    [InlineData(TimeSeriesType.NonProfiledConsumption, null, "brp_id")]
    public async Task AggregationLevelMapper_ReturnsValidDeltaValue(TimeSeriesType timeSeriesType, string? energySupplierId, string? balanceResponsibleId)
    {
        // Arrange
        await using var stream = EmbeddedResources.GetStream("DeltaTableContracts.Contracts.aggregation-level.json");
        var validDeltaValues = await ContractComplianceTestHelper.GetCodeListValuesAsync(stream);

        // Act
        var actual = AggregationLevelMapper.ToDeltaTableValue(timeSeriesType, energySupplierId, balanceResponsibleId);

        // Assert
        actual.Should().BeOneOf(validDeltaValues);
    }
}
