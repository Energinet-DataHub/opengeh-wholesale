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
using Energinet.DataHub.Wholesale.IntegrationTests.Fixture.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
        var batch = new Batch(ProcessType.BalanceFixing, someGridAreasIds);
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
}
