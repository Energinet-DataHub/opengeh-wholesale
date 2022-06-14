using Energinet.DataHub.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Batches;
using Energinet.DataHub.Wholesale.IntegrationTests.Core.Fixtures.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Infrastructure;

[IntegrationTest]
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
        var gridAreasIds = new List<Guid>{Guid.NewGuid(), Guid.NewGuid()};
        var batch = new Batch(WholesaleProcessType.BalanceFixing, gridAreasIds);
        var sut = new BatchRepository(writeContext);

        // Act
        await sut.AddAsync(batch);
        await writeContext.SaveChangesAsync();

        // Assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.Batches.SingleAsync(b => b.Id == batch.Id);

        actual.Should().BeEquivalentTo(batch);
        actual.GridAreaIds.Should().BeEquivalentTo(gridAreasIds);
    }
}
