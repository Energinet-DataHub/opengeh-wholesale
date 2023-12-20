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
using Energinet.DataHub.Wholesale.Batches.Application.Model.Batches;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence.Batches;
using Energinet.DataHub.Wholesale.Batches.UnitTests.Infrastructure.BatchAggregate;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Batches.UnitTests.Infrastructure.Persistence.Batches;

public class BatchRepositoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task GetCompletedAfterAsync_WhenCompletedTimeIsNullAndNoBatchMatches_ReturnsNone(
        Mock<DatabaseContext> databaseContextMock)
    {
        // Arrange
        databaseContextMock
            .Setup<DbSet<Calculation>>(context => context.Batches)
            .ReturnsDbSet(new List<Calculation>());
        var sut = new BatchRepository(databaseContextMock.Object);

        // Act
        var actual = await sut.GetCompletedAfterAsync(null);

        // Assert
        actual.Should().BeEmpty();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetCompletedAfterAsync_WhenCompletedTimeIsNotNullAndNoBatchMatches_ReturnsNone(
        Instant completedTime,
        Mock<DatabaseContext> databaseContextMock)
    {
        // Arrange
        databaseContextMock
            .Setup<DbSet<Calculation>>(context => context.Batches)
            .ReturnsDbSet(new List<Calculation>());
        var sut = new BatchRepository(databaseContextMock.Object);

        // Act
        var actual = await sut.GetCompletedAfterAsync(completedTime);

        // Assert
        actual.Should().BeEmpty();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetCompletedAfterAsync_WhenCompletedTimeIsNullAndSomeBatchesExist_ReturnsThem(
        Mock<DatabaseContext> databaseContextMock)
    {
        // Arrange
        var batch1 = new BatchBuilder().WithStateCompleted().Build();
        var batch2 = new BatchBuilder().WithStateCompleted().Build();
        databaseContextMock
            .Setup<DbSet<Calculation>>(context => context.Batches)
            .ReturnsDbSet(new List<Calculation> { batch1, batch2 });

        var sut = new BatchRepository(databaseContextMock.Object);

        // Act
        var actual = await sut.GetCompletedAfterAsync(null);

        // Assert
        actual.Should().Contain(batch1);
        actual.Should().Contain(batch2);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetCompletedAfterAsync_WhenBatchCompletedBeforeCompletedTime_DoesNotReturnIt(
        Mock<DatabaseContext> databaseContextMock)
    {
        // Arrange
        var batch = new BatchBuilder().WithStateCompleted().Build();
        databaseContextMock
            .Setup<DbSet<Calculation>>(context => context.Batches)
            .ReturnsDbSet(new List<Calculation> { batch });
        var futureCompletedTime = batch.ExecutionTimeEnd!.Value.Plus(Duration.FromMinutes(1));

        var sut = new BatchRepository(databaseContextMock.Object);

        // Act
        var actual = await sut.GetCompletedAfterAsync(futureCompletedTime);

        // Assert
        actual.Should().NotContain(batch);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetCompletedAfterAsync_WhenBatchIsNotCompleted_DoesNotReturnIt(
        Mock<DatabaseContext> databaseContextMock)
    {
        // Arrange
        var nonCompletedBatch = new BatchBuilder().Build();
        databaseContextMock
            .Setup<DbSet<Calculation>>(context => context.Batches)
            .ReturnsDbSet(new List<Calculation> { nonCompletedBatch });

        var sut = new BatchRepository(databaseContextMock.Object);

        // Act
        var actual = await sut.GetCompletedAfterAsync(null);

        // Assert
        actual.Should().NotContain(nonCompletedBatch);
    }
}
