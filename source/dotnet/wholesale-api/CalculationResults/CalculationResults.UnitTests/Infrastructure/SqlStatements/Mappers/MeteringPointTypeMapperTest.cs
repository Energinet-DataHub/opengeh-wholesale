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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using FluentAssertions;
using Test.Core;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SqlStatements.Mappers;

[UnitTest]
public class MeteringPointTypeMapperTests
{
    [Fact]
    public async Task FromDeltaTableValue_ReturnsValidMeteringPointType()
    {
        // Arrange
        await using var stream = EmbeddedResources.GetStream("DeltaTableContracts.Contracts.Enums.time-series-type.json");
        var validDeltaValues = await ContractComplianceTestHelper.GetCodeListValuesAsync(stream);

        foreach (var validDeltaValue in validDeltaValues)
        {
            // Act
            var actual = MeteringPointTypeMapper.FromDeltaTableValue(validDeltaValue);

            // Assert it's a defined enum value (and not null)
            actual.Should().BeDefined();
        }
    }

    [Theory]
    [InlineData(DeltaTableTimeSeriesType.FlexConsumption, MeteringPointType.Consumption)]
    [InlineData(DeltaTableTimeSeriesType.Production, MeteringPointType.Production)]
    [InlineData(DeltaTableTimeSeriesType.NonProfiledConsumption, MeteringPointType.Consumption)]
    public void ToDeltaTableValue_ReturnsExpectedString(string timeSeriesType, MeteringPointType expected)
    {
        // Act
        var actual = MeteringPointTypeMapper.FromDeltaTableValue(timeSeriesType);

        // Assert
        actual.Should().Be(expected);
    }
}
