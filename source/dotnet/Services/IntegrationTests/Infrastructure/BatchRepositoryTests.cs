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
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Batches;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixtures.TestCommon.Fixture.Database;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixtures.TestHelpers;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.Database;
using Energinet.DataHub.Wholesale.IntegrationTests.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Infrastructure;

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

    private static Batch CreateBatch(List<GridAreaCode> someGridAreasIds)
    {
        var period = Periods.January_EuropeCopenhagen_Instant;
        return new Batch(
            ProcessType.BalanceFixing,
            someGridAreasIds,
            period.PeriodStart,
            period.PeriodEnd,
            SystemClock.Instance.GetCurrentInstant(),
            period.DateTimeZone);
    }
}
