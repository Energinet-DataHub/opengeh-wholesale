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
using Energinet.DataHub.Wholesale.WebApi.V3.Batch;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Azure.Amqp.Framing;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.WebApi.V3;

public class BatchDtoMapperTests
{
    [Theory]
    [InlineAutoMoqData]
    public void MapDto_Returns_correct(Batches.Interfaces.Models.BatchDto source)
    {
        // Act
        var actual = BatchDtoMapper.Map(source);

        // Assert
        actual.ProcessType.Should().Be(ProcessTypeMapper.Map(source.ProcessType));
        actual.ExecutionState.Should().Be(BatchStateMapper.MapState(source.ExecutionState));
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

    [Theory]
    [InlineAutoMoqData]
    public void VerifyTestOutputWhenFailing(Batches.Interfaces.Models.BatchDto source)
    {
        // Act
        var actual = BatchDtoMapper.Map(source);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.BatchId.Should().Be(Guid.Empty);
        actual.AreSettlementReportsCreated.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void VerifyTestOutputShowInputDataWhenUsingTheory(TimeSpan waitTimeLimit)
    {
        waitTimeLimit.Should().BeLessThan(TimeSpan.FromHours(1));
    }

    public static IEnumerable<object[]> Data =>
        new List<object[]>
        {
            new object[] { TimeSpan.FromHours(5) },
        };
}
