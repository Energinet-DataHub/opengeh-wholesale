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
using Energinet.DataHub.Wholesale.WebApi.V3.Calculation;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.WebApi.V3;

public static class BatchDtoMapperTests
{
    [Theory]
    [InlineAutoMoqData]
    public static void MapDto_Returns_correct(Calculations.Interfaces.Models.CalculationDto source)
    {
        // Act
        var actual = CalculationDtoMapper.Map(source);

        // Assert
        actual.ProcessType.Should().Be(ProcessTypeMapper.Map(source.ProcessType));
        actual.ExecutionState.Should().Be(CalculationStateMapper.MapState(source.ExecutionState));
        actual.Resolution.Should().Be(source.Resolution);
        actual.RunId.Should().Be(source.RunId);
        actual.Unit.Should().Be(source.Unit.ToString());
        actual.PeriodStart.Should().Be(source.PeriodStart);
        actual.PeriodEnd.Should().Be(source.PeriodEnd);
        actual.ExecutionTimeStart.Should().Be(source.ExecutionTimeStart);
        actual.ExecutionTimeEnd.Should().Be(source.ExecutionTimeEnd);
        actual.GridAreaCodes.Should().Contain(source.GridAreaCodes);
        actual.AreSettlementReportsCreated.Should().Be(source.AreSettlementReportsCreated);
        actual.BatchId.Should().Be(source.BatchId);
        actual.BatchId.Should().Be(source.BatchId);
        actual.CreatedByUserId.Should().Be(source.CreatedByUserId);
    }
}
