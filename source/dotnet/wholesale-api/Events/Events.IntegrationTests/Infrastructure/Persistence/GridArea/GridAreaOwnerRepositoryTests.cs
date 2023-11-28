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
using Energinet.DataHub.Wholesale.Events.Application.GridArea;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Events.Infrastructure.Persistence.GridArea;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.IntegrationTests.Infrastructure.Persistence.GridArea;

public class GridAreaOwnerRepositoryTests
{
    private readonly WholesaleDatabaseManager<EventsDatabaseContext> _databaseManager;

    public GridAreaOwnerRepositoryTests(WholesaleDatabaseFixture<EventsDatabaseContext> fixture)
    {
        _databaseManager = fixture.DatabaseManager;
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task AddAsync_AddsGridAreaOwners(GridAreaOwner expectedGridAreaOwner)
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var sut = new GridAreaOwnerRepository(writeContext);

        // Act
        await sut.AddAsync(expectedGridAreaOwner.Code, expectedGridAreaOwner.OwnerActorNumber, expectedGridAreaOwner.ValidFrom, expectedGridAreaOwner.SequenceNumber);
        await writeContext.SaveChangesAsync();

        // Assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.GridAreaOwners.SingleAsync(b => b.Code.Equals(expectedGridAreaOwner.Code));

        actual.Should().BeEquivalentTo(expectedGridAreaOwner);
    }
}
