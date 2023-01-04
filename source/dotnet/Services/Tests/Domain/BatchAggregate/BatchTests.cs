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

using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
using FluentAssertions;
using NodaTime;
using NodaTime.Extensions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Domain.BatchAggregate;

[UnitTest]
public class BatchTests
{
    [Fact]
    public void Ctor_CreatesImmutableGridAreaCodes()
    {
        // Arrange
        var someGridAreaCodes = new List<GridAreaCode> { new("004"), new("805") };
        var sut = new BatchBuilder().WithGridAreaCodes(someGridAreaCodes).Build();

        // Act
        var unexpectedGridAreaCode = new GridAreaCode("777");
        someGridAreaCodes.Add(unexpectedGridAreaCode);

        // Assert
        sut.GridAreaCodes.Should().NotContain(unexpectedGridAreaCode);
    }

    [Fact]
    public void Ctor_WhenNoGridAreaCodes_ThrowsArgumentException()
    {
        // Arrange
        // ReSharper disable once CollectionNeverUpdated.Local
        var emptyGridAreaCodes = new List<GridAreaCode>();
        var clock = SystemClock.Instance;

        // Act
        var batch = new Batch(
                ProcessType.BalanceFixing,
                emptyGridAreaCodes,
                Instant.FromDateTimeOffset(DateTimeOffset.Now),
                Instant.FromDateTimeOffset(DateTimeOffset.Now),
                clock);

        // Assert
        batch.IsValid(out var validationErrors).Should().BeFalse();
        validationErrors.Should().Contain("Batch must contain at least one grid area code.");
    }

    [Fact]
    public void Ctor_SetsExecutionTimeEndToNull()
    {
        var sut = new BatchBuilder().WithStatePending().Build();
        sut.ExecutionTimeEnd.Should().BeNull();
    }

    [Fact]
    public void Ctor_ExecutionTimeStartNotNull()
    {
        var sut = new BatchBuilder().WithStatePending().Build();
        sut.ExecutionTimeStart.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsCompleted_WhenComplete_ThrowsInvalidOperationException()
    {
        var sut = new BatchBuilder().WithStateCompleted().Build();
        Assert.Throws<InvalidOperationException>(() => sut.MarkAsCompleted());
    }

    [Theory]
    [InlineData("2023-01-31T23:00Z")]
    [InlineData("2023-01-31T22:59:59Z")]
    [InlineData("2023-01-31T22:59:59.9999999Z")]
    [InlineData("2023-01-31")]
    public void Ctor_WhenPeriodEndIsNot1MillisecondBeforeMidnight_ThrowsArgumentException(string periodEndString)
    {
        // Arrange
        var periodEnd = DateTimeOffset.Parse(periodEndString).ToInstant();
        var gridAreaCode = new GridAreaCode("113");

        // Act
        var actual = new Batch(
            ProcessType.BalanceFixing,
            new[] { gridAreaCode },
            Instant.MinValue,
            periodEnd,
            SystemClock.Instance);

        // Assert
        actual.IsValid(out var validationErrors).Should().BeFalse();
        validationErrors.Should()
            .Contain($"The period end '{actual.PeriodEnd.ToString()}' must be one millisecond before midnight.");
    }

    [Fact]
    public void MarkAsCompleted_WhenExecuting_CompletesBatch()
    {
        var sut = new BatchBuilder().WithStateExecuting().Build();
        sut.MarkAsCompleted();
        sut.ExecutionState.Should().Be(BatchExecutionState.Completed);
    }

    [Fact]
    public void MarkAsCompleted_SetsExecutionTimeEnd()
    {
        var sut = new BatchBuilder().WithStateExecuting().Build();
        sut.MarkAsCompleted();
        sut.ExecutionTimeEnd.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsExecuting_WhenExecuting_ThrowsInvalidOperationException()
    {
        var sut = new BatchBuilder().WithStateExecuting().Build();
        Assert.Throws<InvalidOperationException>(() => sut.MarkAsExecuting());
    }

    [Fact]
    public void MarkAsExecuting_WhenComplete_ThrowsInvalidOperationException()
    {
        var sut = new BatchBuilder().WithStateCompleted().Build();
        Assert.Throws<InvalidOperationException>(() => sut.MarkAsExecuting());
    }

    [Fact]
    public void MarkAsExecuting_WhenPending_ExecutesBatch()
    {
        var sut = new BatchBuilder().WithStatePending().Build();
        sut.MarkAsExecuting();
        sut.ExecutionState.Should().Be(BatchExecutionState.Executing);
    }

    [Fact]
    public void MarkAsExecuting_ExecutionTimeIsSetToNull()
    {
        var sut = new BatchBuilder().WithStatePending().Build();
        sut.MarkAsExecuting();
        sut.ExecutionTimeEnd.Should().BeNull();
    }

    [Fact]
    public void Reset_WhenSubmitted_SetsStateCreated()
    {
        var sut = new BatchBuilder().WithStateSubmitted().Build();
        sut.Reset();
        sut.ExecutionState.Should().Be(BatchExecutionState.Created);
    }

    [Fact]
    public void Reset_WhenPending_SetsStateCreated()
    {
        var sut = new BatchBuilder().WithStatePending().Build();
        sut.Reset();
        sut.ExecutionState.Should().Be(BatchExecutionState.Created);
    }

    [Fact]
    public void Reset_WhenExecuting_SetsStateCreated()
    {
        var sut = new BatchBuilder().WithStateExecuting().Build();
        sut.Reset();
        sut.ExecutionState.Should().Be(BatchExecutionState.Created);
    }

    [Fact]
    public void Reset_WhenCompleted_Throws()
    {
        var sut = new BatchBuilder().WithStateCompleted().Build();
        Assert.Throws<InvalidOperationException>(() => sut.Reset());
    }
}
