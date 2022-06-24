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

using Energinet.DataHub.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Tests.Domain.BatchAggregate;

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
        // ReSharper disable once CollectionNeverUpdated.Local
        var emptyGridAreaCodes = new List<GridAreaCode>();
        Assert.Throws<ArgumentException>(() => new Batch(WholesaleProcessType.BalanceFixing, emptyGridAreaCodes));
    }

    [Fact]
    public void Complete_WhenPending_ThrowsInvalidOperationException()
    {
        var sut = new BatchBuilder().WithState(BatchExecutionState.Pending).Build();
        Assert.Throws<InvalidOperationException>(() => sut.Complete());
    }

    [Fact]
    public void Complete_WhenComplete_ThrowsInvalidOperationException()
    {
        var sut = new BatchBuilder().WithState(BatchExecutionState.Completed).Build();
        Assert.Throws<InvalidOperationException>(() => sut.Complete());
    }

    [Fact]
    public void Complete_WhenExecuting_CompletesBatch()
    {
        var sut = new BatchBuilder().WithState(BatchExecutionState.Executing).Build();
        sut.Complete();
        sut.ExecutionState.Should().Be(BatchExecutionState.Completed);
    }

    [Fact]
    public void SetExecuting_WhenExecuting_ThrowsInvalidOperationException()
    {
        var sut = new BatchBuilder().WithState(BatchExecutionState.Executing).Build();
        Assert.Throws<InvalidOperationException>(() => sut.SetExecuting());
    }

    [Fact]
    public void SetExecuting_WhenComplete_ThrowsInvalidOperationException()
    {
        var sut = new BatchBuilder().WithState(BatchExecutionState.Completed).Build();
        Assert.Throws<InvalidOperationException>(() => sut.SetExecuting());
    }

    [Fact]
    public void SetExecuting_WhenPending_ExecutesBatch()
    {
        var sut = new BatchBuilder().WithState(BatchExecutionState.Pending).Build();
        sut.SetExecuting();
        sut.ExecutionState.Should().Be(BatchExecutionState.Executing);
    }
}
