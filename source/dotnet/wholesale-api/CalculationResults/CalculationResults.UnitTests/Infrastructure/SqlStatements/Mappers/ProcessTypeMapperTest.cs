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
using Energinet.DataHub.Wholesale.Common.Models;
using FluentAssertions;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SqlStatements.Mappers;

public class ProcessTypeMapperTests
{
    [Fact]
    public async Task ProcessType_Matches_Contract()
    {
        await using var stream = EmbeddedResources.GetStream<Root>("DeltaTableContracts.enums.process-type.json");
        await ContractComplianceTestHelper.VerifyEnumCompliesWithContractAsync<ProcessType>(stream);
    }

    [Theory]
    [InlineData(ProcessType.Aggregation)]
    [InlineData(ProcessType.BalanceFixing)]
    [InlineData(ProcessType.WholesaleFixing)]
    [InlineData(ProcessType.FirstCorrectionSettlement)]
    [InlineData(ProcessType.SecondCorrectionSettlement)]
    [InlineData(ProcessType.ThirdCorrectionSettlement)]
    public async Task ToDeltaTableValue_ReturnsValidDeltaValue(ProcessType processType)
    {
        // Arrange
        await using var stream = EmbeddedResources.GetStream<Root>("DeltaTableContracts.enums.process-type.json");
        var validDeltaValues = await ContractComplianceTestHelper.GetCodeListValuesAsync(stream);

        // Act
        var actual = ProcessTypeMapper.ToDeltaTableValue(processType);

        // Assert
        actual.Should().BeOneOf(validDeltaValues);
    }

    [Theory]
    [InlineData(ProcessType.Aggregation, DeltaTableProcessType.Aggregation)]
    [InlineData(ProcessType.BalanceFixing, DeltaTableProcessType.BalanceFixing)]
    [InlineData(ProcessType.WholesaleFixing, DeltaTableProcessType.WholesaleFixing)]
    [InlineData(ProcessType.FirstCorrectionSettlement, DeltaTableProcessType.FirstCorrectionSettlement)]
    [InlineData(ProcessType.SecondCorrectionSettlement, DeltaTableProcessType.SecondCorrectionSettlement)]
    [InlineData(ProcessType.ThirdCorrectionSettlement, DeltaTableProcessType.ThirdCorrectionSettlement)]
    public void ToDeltaTableValue_ReturnsExpectedString(ProcessType type, string expected)
    {
        // Act
        var actual = ProcessTypeMapper.ToDeltaTableValue(type);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(DeltaTableProcessType.Aggregation, ProcessType.Aggregation)]
    [InlineData(DeltaTableProcessType.BalanceFixing, ProcessType.BalanceFixing)]
    [InlineData(DeltaTableProcessType.WholesaleFixing, ProcessType.WholesaleFixing)]
    [InlineData(DeltaTableProcessType.FirstCorrectionSettlement, ProcessType.FirstCorrectionSettlement)]
    [InlineData(DeltaTableProcessType.SecondCorrectionSettlement, ProcessType.SecondCorrectionSettlement)]
    [InlineData(DeltaTableProcessType.ThirdCorrectionSettlement, ProcessType.ThirdCorrectionSettlement)]
    public void FromDeltaTableValue_ReturnsExpectedType(string deltaTableValue, ProcessType expected)
    {
        // Act
        var actual = ProcessTypeMapper.FromDeltaTableValue(deltaTableValue);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void FromDeltaTableValue_WhenInvalidDeltaTableValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidDeltaTableValue = Guid.NewGuid().ToString();

        // Act
        var act = () => ProcessTypeMapper.FromDeltaTableValue(invalidDeltaTableValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidDeltaTableValue);
    }

    [Fact]
    public void ToDeltaTableValue_WhenInvalidEnumNumberForProcessType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (ProcessType)99;

        // Act
        var act = () => ProcessTypeMapper.ToDeltaTableValue(invalidValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ActualValue.Should().Be(invalidValue);
    }
}
