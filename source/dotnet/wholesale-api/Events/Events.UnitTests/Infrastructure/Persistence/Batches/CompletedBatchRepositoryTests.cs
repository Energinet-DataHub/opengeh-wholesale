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

using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence.CompletedBatches;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using Moq;
using Moq.EntityFrameworkCore;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.Persistence.Batches;

public class CompletedBatchRepositoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task GetLastCompletedOrNullAsync_WhenNone_ReturnsNull(
        Mock<EventsDatabaseContext> databaseContextMock)
    {
        // Arrange
        databaseContextMock
            .Setup<DbSet<CompletedBatch>>(context => context.CompletedBatches)
            .ReturnsDbSet(new List<CompletedBatch>());
        var sut = new CompletedBatchRepository(databaseContextMock.Object);

        // Act
        var actual = await sut.GetLastCompletedOrNullAsync();

        // Assert
        actual.Should().BeNull();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetLastCompletedOrNullAsync_WhenMore_ReturnsLastByCompletedTime(
        CompletedBatch batchCompletedFirst,
        CompletedBatch batchCompletedLast,
        [Frozen] Mock<EventsDatabaseContext> databaseContextMock)
    {
        // Arrange
        batchCompletedLast.SetPrivateProperty(b => b.CompletedTime, batchCompletedFirst.CompletedTime.PlusSeconds(1));

        databaseContextMock
            .Setup<DbSet<CompletedBatch>>(context => context.CompletedBatches)
            .ReturnsDbSet(new List<CompletedBatch> { batchCompletedFirst, batchCompletedLast });
        var sut = new CompletedBatchRepository(databaseContextMock.Object);

        // Act
        var actual = await sut.GetLastCompletedOrNullAsync();

        // Assert
        actual.Should().Be(batchCompletedLast);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetNextUnpublishedOrNullAsync_WhenNone_ReturnsNull(
        Mock<EventsDatabaseContext> databaseContextMock)
    {
        // Arrange
        databaseContextMock
            .Setup<DbSet<CompletedBatch>>(context => context.CompletedBatches)
            .ReturnsDbSet(new List<CompletedBatch>());
        var sut = new CompletedBatchRepository(databaseContextMock.Object);

        // Act
        var actual = await sut.GetNextUnpublishedOrNullAsync();

        // Assert
        actual.Should().BeNull();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetNextUnpublishedOrNullAsync_WhenMore_ReturnsFirstByCompletedTime(
        CompletedBatch batchCompletedFirst,
        CompletedBatch batchCompletedLast,
        [Frozen] Mock<EventsDatabaseContext> databaseContextMock)
    {
        // Arrange
        batchCompletedFirst.PublishedTime = null;
        batchCompletedLast.PublishedTime = null;
        batchCompletedLast.SetPrivateProperty(b => b.CompletedTime, batchCompletedFirst.CompletedTime.PlusSeconds(1));

        databaseContextMock
            .Setup<DbSet<CompletedBatch>>(context => context.CompletedBatches)
            .ReturnsDbSet(new List<CompletedBatch> { batchCompletedFirst, batchCompletedLast });
        var sut = new CompletedBatchRepository(databaseContextMock.Object);

        // Act
        var actual = await sut.GetNextUnpublishedOrNullAsync();

        // Assert
        actual.Should().Be(batchCompletedFirst);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetNextUnpublishedOrNullAsync_WhenAllAlreadyPublished_ReturnsNull(
        CompletedBatch publishedBatch,
        [Frozen] Mock<EventsDatabaseContext> databaseContextMock)
    {
        // Arrange
        databaseContextMock
            .Setup<DbSet<CompletedBatch>>(context => context.CompletedBatches)
            .ReturnsDbSet(new List<CompletedBatch> { publishedBatch });
        var sut = new CompletedBatchRepository(databaseContextMock.Object);

        // Act
        var actual = await sut.GetNextUnpublishedOrNullAsync();

        // Assert
        actual.Should().BeNull();
    }
}
