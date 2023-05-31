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
using Energinet.DataHub.Wholesale.Batches.IntegrationTests.Fixture.Database;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application.Model;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Infrastructure.Persistence.Batches;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Energinet.DataHub.Wholesale.IntegrationEventPublishing.IntegrationTests.Infrastructure.Persistence.Batches;

public class CompletedBatchRepositoryTests : IClassFixture<WholesaleDatabaseFixture<IntegrationEventPublishingDatabaseContext>>
{
    private readonly WholesaleDatabaseManager<IntegrationEventPublishingDatabaseContext> _databaseManager;

    public CompletedBatchRepositoryTests(WholesaleDatabaseFixture<IntegrationEventPublishingDatabaseContext> fixture)
    {
        _databaseManager = fixture.DatabaseManager;
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task AddAsync_AddsCompletedBatchWithExpectedData(CompletedBatch expectedBatch)
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var sut = new CompletedBatchRepository(writeContext);

        // Act
        await sut.AddAsync(new[] { expectedBatch });
        await writeContext.SaveChangesAsync();

        // Assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.CompletedBatches.SingleAsync(b => b.Id == expectedBatch.Id);

        actual.Should().BeEquivalentTo(expectedBatch);
    }
}
