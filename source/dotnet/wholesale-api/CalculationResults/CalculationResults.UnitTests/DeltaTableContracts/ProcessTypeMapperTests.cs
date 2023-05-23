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

public class ProcessTypeMapperTests
{
    // TODO BJM: Also test mapping of (remember both to and from delta values)
    // - settlement method
    // - time series type
    // - actor
    // - quality
    [Theory]
    [InlineData(ProcessType.Aggregation)]
    [InlineData(ProcessType.BalanceFixing)]
    public async Task AggregationLevelMapper_ReturnsValidDeltaValue(ProcessType processType)
    {
        // Arrange
        await using var stream = EmbeddedResources.GetStream("DeltaTableContracts.Contracts.process-type.json");
        var validDeltaValues = await ContractComplianceTestHelper.GetCodeListValuesAsync(stream);

        // Act
        var actual = ProcessTypeMapper.ToDeltaTableValue(processType);

        // Assert
        actual.Should().BeOneOf(validDeltaValues);
    }
}
