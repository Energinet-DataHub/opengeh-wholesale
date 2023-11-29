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
using Energinet.DataHub.Wholesale.Batches.Application.GridArea;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence.GridArea;
using Energinet.DataHub.Wholesale.Batches.IntegrationTests.Fixture.Database;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Batches.IntegrationTests.Infrastructure.Persistence.GridArea;

public class GridAreaOwnerRepositoryTests
{
    private readonly WholesaleDatabaseManager<DatabaseContext> _databaseManager;

    public GridAreaOwnerRepositoryTests(WholesaleDatabaseFixture<DatabaseContext> fixture)
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

    [Theory]
    [InlineAutoMoqData]
    public async Task GetCurrentOwner_ReturnsExpectedOwner(GridAreaOwner expectedGridAreaOwner)
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        await writeContext.GridAreaOwners.AddAsync(expectedGridAreaOwner);
        await writeContext.SaveChangesAsync();

        await using var readContext = _databaseManager.CreateDbContext();
        var sut = new GridAreaOwnerRepository(readContext);

        // Act
        var actual = await sut.GetCurrentOwnerAsync(expectedGridAreaOwner.Code, CancellationToken.None);

        // Assert
        actual.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCurrentOwner_WhenOneOwnerIsValidTomorrow_ReturnsExpectedOwner()
    {
        // Arrange
        var gridAreaCode = "303";
        var validGridAreaOwner = new GridAreaOwner(
            Id: Guid.NewGuid(),
            OwnerActorNumber: "1234567891234",
            Code: gridAreaCode,
            ValidFrom: Instant.FromUtc(2023, 10, 1, 0, 0, 0),
            SequenceNumber: 1);

        var validTomorrow = validGridAreaOwner with { ValidFrom = DateTime.UtcNow.AddDays(1).ToInstant() };

        await using var writeContext = _databaseManager.CreateDbContext();
        await writeContext.GridAreaOwners.AddRangeAsync(new List<GridAreaOwner> { validGridAreaOwner, validTomorrow });
        await writeContext.SaveChangesAsync();

        await using var readContext = _databaseManager.CreateDbContext();
        var sut = new GridAreaOwnerRepository(readContext);

        // Act
        var actual = await sut.GetCurrentOwnerAsync(gridAreaCode, CancellationToken.None);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(validGridAreaOwner);
    }
}
