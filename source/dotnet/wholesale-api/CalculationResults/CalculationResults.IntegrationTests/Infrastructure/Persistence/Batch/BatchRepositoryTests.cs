﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Wholesale.Batches.Infrastructure.BatchAggregate;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence.Batches;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixture.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.Persistence.Batch;

public class BatchRepositoryTests : IClassFixture<WholesaleDatabaseFixture>
{
    private readonly WholesaleDatabaseManager _databaseManager;

    public BatchRepositoryTests(WholesaleDatabaseFixture fixture)
    {
        _databaseManager = fixture.DatabaseManager;
    }

    [Fact]
    public async Task AddAsync_AddsBatch()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var someGridAreasIds = new List<GridAreaCode> { new("004"), new("805") };
        var batch = CreateBatch(ProcessType.Aggregation, someGridAreasIds);
        var sut = new BatchRepository(writeContext);

        // Act
        await sut.AddAsync(batch);
        await writeContext.SaveChangesAsync();

        // Assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.Batches.SingleAsync(b => b.Id == batch.Id);

        actual.Should().BeEquivalentTo(batch);
        actual.GridAreaCodes.Should().BeEquivalentTo(someGridAreasIds);
        actual.ProcessType.Should().Be(ProcessType.Aggregation);
    }

    [Fact]
    public async Task AddAsync_BatchContainsExecutionTime()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var someGridAreasIds = new List<GridAreaCode> { new("004"), new("805") };
        var batch = CreateBatch(someGridAreasIds);
        var sut = new BatchRepository(writeContext);
        batch.MarkAsExecuting(); // This call will ensure ExecutionTimeStart is set
        batch.MarkAsCompleted(
            batch.ExecutionTimeStart.Plus(Duration.FromDays(2))); // This call will ensure ExecutionTimeEnd is set
        batch.ExecutionTimeEnd.Should().NotBeNull(); // Additional check
        batch.ExecutionTimeStart.Should().NotBeNull(); // Additional check

        // Act
        await sut.AddAsync(batch);
        await writeContext.SaveChangesAsync();

        // Assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.Batches.SingleAsync(b => b.Id == batch.Id);

        Assert.Equal(actual.ExecutionTimeStart, batch.ExecutionTimeStart);
        Assert.Equal(actual.ExecutionTimeEnd, batch.ExecutionTimeEnd);
    }

    [Fact]
    public async Task AddAsync_WhenExecutionTimeEndIsNull_BatchExecutionTimeIsNull()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var someGridAreasIds = new List<GridAreaCode> { new("004"), new("805") };
        var batch = CreateBatch(someGridAreasIds);
        var sut = new BatchRepository(writeContext);

        // Act
        await sut.AddAsync(batch);
        await writeContext.SaveChangesAsync();

        // Assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.Batches.SingleAsync(b => b.Id == batch.Id);

        actual.Should().BeEquivalentTo(batch);
        actual.GridAreaCodes.Should().BeEquivalentTo(someGridAreasIds);
        actual.ExecutionTimeEnd.Should().BeNull();
        actual.ExecutionTimeStart.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchAsync_HasGridAreaFilter_FiltersAsExpected()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var someGridAreasIds = new List<GridAreaCode> { new("004"), new("805") };
        var batch = CreateBatch(someGridAreasIds);
        var sut = new BatchRepository(writeContext);
        await sut.AddAsync(batch);
        await writeContext.SaveChangesAsync();

        // Act
        var actual = await sut.SearchAsync(
            new[] { new GridAreaCode("004") },
            Array.Empty<BatchExecutionState>(),
            null,
            null,
            null,
            null);

        // Assert
        actual.Should().Contain(batch);
    }

    [Fact]
    public async Task SearchAsync_HasBatchExecutionStateFilter_FiltersAsExpected()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var someGridAreasIds = new List<GridAreaCode> { new("004"), new("805") };
        var batch = CreateBatch(someGridAreasIds);
        var sut = new BatchRepository(writeContext);
        await sut.AddAsync(batch);
        await writeContext.SaveChangesAsync();

        // Act
        var actual = await sut.SearchAsync(
            Array.Empty<GridAreaCode>(),
            new[] { BatchExecutionState.Created },
            null,
            null,
            null,
            null);

        // Assert
        actual.Should().Contain(batch);
    }

    [Theory]
    [InlineData("2021-12-31T23:00Z", "2022-01-31T23:00Z", true)] // Period overlaps batch interval
    [InlineData("2022-01-01T23:00Z", "2022-01-30T23:00Z", true)] // Period inside of batch interval
    [InlineData("2022-01-30T23:00Z", "2022-02-02T23:00Z", true)] // Period overlaps end of batch interval
    [InlineData("2021-12-30T23:00Z", "2022-02-01T23:00Z", true)] // Period overlaps entire batch interval
    [InlineData("2021-12-25T23:00Z", "2022-01-01T23:00Z", true)] // Period overlaps start of batch interval
    [InlineData("2021-12-25T23:00Z", "2021-12-31T23:00Z", true)] // Period before start of batch interval, include batch start
    [InlineData("2021-12-21T23:00Z", "2021-12-29T23:00Z", false)] // Period before start of batch interval, complete miss
    [InlineData("2022-01-31T23:00Z", "2022-02-01T23:00Z", true)] // Period past end of batch interval, include batch end
    public async Task SearchAsync_HasPeriodFilter_FiltersAsExpected(DateTimeOffset start, DateTimeOffset end, bool expected)
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var someGridAreasIds = new List<GridAreaCode> { new("004"), new("805") };
        var batch = CreateBatch(someGridAreasIds);
        var sut = new BatchRepository(writeContext);
        await sut.AddAsync(batch);
        await writeContext.SaveChangesAsync();

        // Act
        var actual = await sut.SearchAsync(
            Array.Empty<GridAreaCode>(),
            Array.Empty<BatchExecutionState>(),
            null,
            null,
            Instant.FromDateTimeOffset(start),
            Instant.FromDateTimeOffset(end));

        // Assert
        if (expected)
            actual.Should().Contain(batch);
        else
            actual.Should().NotContain(batch);
    }

    [Theory]
    [InlineData("2022-04-01T23:00Z", "2022-06-01T23:00Z", true)] // Execution time inside period
    [InlineData("2022-05-02T23:00Z", "2022-06-01T23:00Z", false)] // Execution time before period
    [InlineData("2022-01-01T23:00Z", "2022-04-30T23:00Z", false)] // Execution time after period
    public async Task SearchAsync_HasExecutionTimeFilter_FiltersAsExpected(DateTimeOffset start, DateTimeOffset end, bool expected)
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();

        var period = Periods.January_EuropeCopenhagen_Instant;
        var batch = new Batches.Infrastructure.BatchAggregate.Batch(
           ProcessType.BalanceFixing,
           new List<GridAreaCode> { new("004") },
           period.PeriodStart,
           period.PeriodEnd,
           Instant.FromUtc(2022, 5, 1, 0, 0),
           period.DateTimeZone);

        var sut = new BatchRepository(writeContext);
        await sut.AddAsync(batch);
        await writeContext.SaveChangesAsync();

        // Act
        var actual = await sut.SearchAsync(
            Array.Empty<GridAreaCode>(),
            Array.Empty<BatchExecutionState>(),
            Instant.FromDateTimeOffset(start),
            Instant.FromDateTimeOffset(end),
            null,
            null);

        // Assert
        if (expected)
            actual.Should().Contain(batch);
        else
            actual.Should().NotContain(batch);
    }

    private static Batches.Infrastructure.BatchAggregate.Batch CreateBatch(List<GridAreaCode> someGridAreasIds)
    {
        return CreateBatch(ProcessType.BalanceFixing, someGridAreasIds);
    }

    private static Batches.Infrastructure.BatchAggregate.Batch CreateBatch(ProcessType processType, List<GridAreaCode> someGridAreasIds)
    {
        var period = Periods.January_EuropeCopenhagen_Instant;
        return new Batches.Infrastructure.BatchAggregate.Batch(
            processType,
            someGridAreasIds,
            period.PeriodStart,
            period.PeriodEnd,
            SystemClock.Instance.GetCurrentInstant(),
            period.DateTimeZone);
    }
}
