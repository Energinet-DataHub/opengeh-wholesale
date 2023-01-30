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
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Application.Batches.Model;
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Tests.Application.Processes;

public class ProcessCompletedEventDtoFactoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public void Create_ReturnsCorrectEvent(ProcessCompletedEventDtoFactory sut)
    {
        // Arrange
        var completedBatch = new BatchCompletedEventDto(Guid.NewGuid(), new List<string> { "153", "805" }, ProcessType.BalanceFixing, SystemClock.Instance.GetCurrentInstant(), SystemClock.Instance.GetCurrentInstant());
        var expected = new List<ProcessCompletedEventDto>
        {
            new(
                "153",
                completedBatch.BatchId,
                Contracts.ProcessType.BalanceFixing,
                completedBatch.PeriodStart,
                completedBatch.PeriodEnd),
            new(
                "805",
                completedBatch.BatchId,
                Contracts.ProcessType.BalanceFixing,
                completedBatch.PeriodStart,
                completedBatch.PeriodEnd),
        };

        // Act
        var actual = sut.CreateFromBatchCompletedEvent(completedBatch);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }
}
