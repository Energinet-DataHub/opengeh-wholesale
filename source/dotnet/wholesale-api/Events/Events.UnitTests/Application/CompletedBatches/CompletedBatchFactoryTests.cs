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
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using FluentAssertions;
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Application.CompletedBatches;

public class CompletedBatchFactoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public void CreateFromBatch_ReturnsCompletedBatch(BatchDto batch, CompletedBatchFactory sut)
    {
        // Arrange
        var expectedCompletedBatch = new CompletedBatch(
            batch.BatchId,
            batch.GridAreaCodes.ToList(),
            batch.ProcessType,
            batch.PeriodStart.ToInstant(),
            batch.PeriodEnd.ToInstant(),
            batch.ExecutionTimeEnd!.Value.ToInstant());

        // Act
        var actual = sut.CreateFromBatch(batch);

        // Assert
        actual.Should().BeEquivalentTo(expectedCompletedBatch);
    }
}
