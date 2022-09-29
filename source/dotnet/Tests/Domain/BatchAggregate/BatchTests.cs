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
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Domain.BatchAggregate;

[UnitTest]
public class BatchTests
{
    private static readonly JobRunId _fakeJobRunId = new JobRunId(1);

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
        // ReSharper disable once CollectionNeverUpdated.Local
        var emptyGridAreaCodes = new List<GridAreaCode>();
        var clock = SystemClock.Instance;
        Assert.Throws<ArgumentException>(() => new Batch(
            ProcessType.BalanceFixing,
            emptyGridAreaCodes,
            Instant.FromDateTimeOffset(DateTimeOffset.Now),
            Instant.FromDateTimeOffset(DateTimeOffset.Now),
            clock));
    }

    [Fact]
    public void Ctor_SetsExecutionTimeEndToNull()
    {
        var sut = new BatchBuilder().WithState(BatchExecutionState.Pending).Build();
        sut.ExecutionTimeEnd.Should().BeNull();
    }

    [Fact]
    public void Ctor_ExecutionTimeStartNotNull()
    {
        var sut = new BatchBuilder().WithState(BatchExecutionState.Pending).Build();
        sut.ExecutionTimeStart.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsCompleted_WhenPending_ThrowsInvalidOperationException()
    {
        var sut = new BatchBuilder().WithState(BatchExecutionState.Pending).Build();
        Assert.Throws<InvalidOperationException>(() => sut.MarkAsCompleted());
    }

    [Fact]
    public void MarkAsCompleted_WhenComplete_ThrowsInvalidOperationException()
    {
        var sut = new BatchBuilder().WithState(BatchExecutionState.Completed).Build();
        Assert.Throws<InvalidOperationException>(() => sut.MarkAsCompleted());
    }

    [Fact]
    public void MarkAsCompleted_WhenExecuting_CompletesBatch()
    {
        var sut = new BatchBuilder().WithState(BatchExecutionState.Executing).Build();
        sut.MarkAsCompleted();
        sut.ExecutionState.Should().Be(BatchExecutionState.Completed);
    }

    [Fact]
    public void MarkAsCompleted_SetsExecutionTimeEnd()
    {
        var sut = new BatchBuilder().WithState(BatchExecutionState.Executing).Build();
        sut.MarkAsCompleted();
        sut.ExecutionTimeEnd.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsExecuting_WhenExecuting_ThrowsInvalidOperationException()
    {
        var sut = new BatchBuilder().WithState(BatchExecutionState.Executing).Build();
        Assert.Throws<InvalidOperationException>(() => sut.MarkAsExecuting());
    }

    [Fact]
    public void MarkAsExecuting_WhenComplete_ThrowsInvalidOperationException()
    {
        var sut = new BatchBuilder().WithState(BatchExecutionState.Completed).Build();
        Assert.Throws<InvalidOperationException>(() => sut.MarkAsExecuting());
    }

    [Fact]
    public void MarkAsExecuting_WhenPending_ExecutesBatch()
    {
        var sut = new BatchBuilder().WithState(BatchExecutionState.Pending).Build();
        sut.MarkAsExecuting();
        sut.ExecutionState.Should().Be(BatchExecutionState.Executing);
    }

    [Fact]
    public void MarkAsExecuting_ExecutionTimeIsSetToNull()
    {
        var sut = new BatchBuilder().WithState(BatchExecutionState.Pending).Build();
        sut.MarkAsExecuting();
        sut.ExecutionTimeEnd.Should().BeNull();
    }
}
