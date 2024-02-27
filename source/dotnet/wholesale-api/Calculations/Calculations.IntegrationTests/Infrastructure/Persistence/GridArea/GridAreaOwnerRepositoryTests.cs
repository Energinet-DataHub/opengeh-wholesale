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

using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.GridArea;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.GridArea;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Extensions;
using Test.Core.Fixture.Database;
using Xunit;

namespace Energinet.DataHub.Wholesale.Calculations.IntegrationTests.Infrastructure.Persistence.GridArea;

public class GridAreaOwnerRepositoryTests : IClassFixture<WholesaleDatabaseFixture<DatabaseContext>>
{
    private readonly WholesaleDatabaseManager<DatabaseContext> _databaseManager;

    public GridAreaOwnerRepositoryTests(WholesaleDatabaseFixture<DatabaseContext> fixture)
    {
        _databaseManager = fixture.DatabaseManager;
    }

    [Fact]
    public async Task Add_AddsGridAreaOwner()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var sut = new GridAreaOwnerRepository(writeContext);
        var expectedGridAreaOwner = new GridAreaOwner(
            Id: Guid.NewGuid(),
            OwnerActorNumber: "1234567891234",
            GridAreaCode: "301",
            ValidFrom: Instant.FromUtc(2023, 10, 1, 0, 0, 0),
            SequenceNumber: 1);

        // Act
        sut.Add(
            expectedGridAreaOwner.GridAreaCode,
            expectedGridAreaOwner.OwnerActorNumber,
            expectedGridAreaOwner.ValidFrom,
            expectedGridAreaOwner.SequenceNumber);
        await writeContext.SaveChangesAsync();

        // Assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.GridAreaOwners.SingleAsync(b => b.GridAreaCode.Equals(expectedGridAreaOwner.GridAreaCode));

        using var assertionScope = new AssertionScope();
        actual.Should().NotBeNull();
        actual.OwnerActorNumber.Should().Be(expectedGridAreaOwner.OwnerActorNumber);
        actual.GridAreaCode.Should().Be(expectedGridAreaOwner.GridAreaCode);
        actual.ValidFrom.Should().Be(expectedGridAreaOwner.ValidFrom);
        actual.SequenceNumber.ToString().Should().Be(expectedGridAreaOwner.SequenceNumber.ToString());
    }

    [Fact]
    public async Task Add_WhenAddingTheSameGridAreaOwnerTwice_ThrowsException()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var sut = new GridAreaOwnerRepository(writeContext);
        var gridAreaOwner = new GridAreaOwner(
            Id: Guid.NewGuid(),
            OwnerActorNumber: "1234567891235",
            GridAreaCode: "302",
            ValidFrom: Instant.FromUtc(2023, 10, 1, 0, 0, 0),
            SequenceNumber: 1);
        sut.Add(gridAreaOwner.GridAreaCode, gridAreaOwner.OwnerActorNumber, gridAreaOwner.ValidFrom, gridAreaOwner.SequenceNumber);
        await writeContext.SaveChangesAsync();

        // Act
        sut.Add(gridAreaOwner.GridAreaCode, gridAreaOwner.OwnerActorNumber, gridAreaOwner.ValidFrom, gridAreaOwner.SequenceNumber);
        var act = async () => await writeContext.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task GetCurrentOwner_ReturnsExpectedOwner()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var expectedGridAreaOwner = new GridAreaOwner(
            Id: Guid.NewGuid(),
            OwnerActorNumber: "1234567891236",
            GridAreaCode: "303",
            ValidFrom: Instant.FromUtc(2023, 10, 1, 0, 0, 0),
            SequenceNumber: 1);
        await writeContext.GridAreaOwners.AddAsync(expectedGridAreaOwner);
        await writeContext.SaveChangesAsync();

        await using var readContext = _databaseManager.CreateDbContext();
        var sut = new GridAreaOwnerRepository(readContext);

        // Act
        var actual = await sut.GetCurrentOwnerAsync(expectedGridAreaOwner.GridAreaCode, CancellationToken.None);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(expectedGridAreaOwner);
    }

    [Fact]
    public async Task GetCurrentOwner_WhenOneOwnerIsValidTomorrow_ReturnsExpectedOwner()
    {
        // Arrange
        var gridAreaCode = "304";
        var expectedOwner = new GridAreaOwner(
            Id: Guid.NewGuid(),
            OwnerActorNumber: "1234567891237",
            GridAreaCode: gridAreaCode,
            ValidFrom: Instant.FromUtc(2023, 10, 1, 0, 0, 0),
            SequenceNumber: 1);

        var validTomorrow = expectedOwner with
        {
            Id = Guid.NewGuid(),
            ValidFrom = DateTime.UtcNow.AddDays(1).ToInstant(),
            SequenceNumber = 2,
        };

        await using var writeContext = _databaseManager.CreateDbContext();
        await writeContext.GridAreaOwners.AddRangeAsync(new List<GridAreaOwner> { expectedOwner, validTomorrow });
        await writeContext.SaveChangesAsync();

        await using var readContext = _databaseManager.CreateDbContext();
        var sut = new GridAreaOwnerRepository(readContext);

        // Act
        var actual = await sut.GetCurrentOwnerAsync(gridAreaCode, CancellationToken.None);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(expectedOwner);
    }

    [Fact]
    public async Task GetCurrentOwner_WhenTwoValidOwners_ReturnsOwnerWithHighestSequenceNumber()
    {
        // Arrange
        var gridAreaCode = "305";
        var validGridAreaOwner = new GridAreaOwner(
            Id: Guid.NewGuid(),
            OwnerActorNumber: "1234567891238",
            GridAreaCode: gridAreaCode,
            ValidFrom: Instant.FromUtc(2023, 10, 1, 0, 0, 0),
            SequenceNumber: 1);

        var expectedOwner = validGridAreaOwner with
        {
            Id = Guid.NewGuid(),
            SequenceNumber = 2,
        };

        await using var writeContext = _databaseManager.CreateDbContext();
        await writeContext.GridAreaOwners.AddRangeAsync(new List<GridAreaOwner> { validGridAreaOwner, expectedOwner });
        await writeContext.SaveChangesAsync();

        await using var readContext = _databaseManager.CreateDbContext();
        var sut = new GridAreaOwnerRepository(readContext);

        // Act
        var actual = await sut.GetCurrentOwnerAsync(gridAreaCode, CancellationToken.None);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(expectedOwner);
    }

    [Fact]
    public async Task GetCurrentOwner_WhenNoValidOwnersForGridArea_ReturnsNull()
    {
        // Arrange
        var invalidGridAreaOwner = new GridAreaOwner(
            Id: Guid.NewGuid(),
            OwnerActorNumber: "1234567891239",
            GridAreaCode: "306",
            ValidFrom: DateTime.UtcNow.AddDays(1).ToInstant(),
            SequenceNumber: 1);

        await using var writeContext = _databaseManager.CreateDbContext();
        await writeContext.GridAreaOwners.AddAsync(invalidGridAreaOwner);
        await writeContext.SaveChangesAsync();

        await using var readContext = _databaseManager.CreateDbContext();
        var sut = new GridAreaOwnerRepository(readContext);

        // Act
        var actual = await sut.GetCurrentOwnerAsync(invalidGridAreaOwner.GridAreaCode, CancellationToken.None);

        // Assert
        actual.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentOwner_WhenNewGridAreaEventWithExistingGridAreaCodeAndHigherSequenceNumber_ReturnsNewOwner()
    {
        // Arrange
        var gridAreaCode = "307";
        var existingGridAreaOwner = new GridAreaOwner(
            Id: Guid.NewGuid(),
            OwnerActorNumber: "1234567891239",
            GridAreaCode: gridAreaCode,
            ValidFrom: DateTime.UtcNow.ToInstant(),
            SequenceNumber: 1);
        var newGridAreaOwnerFromThePast = new GridAreaOwner(
            Id: Guid.NewGuid(),
            OwnerActorNumber: "1234567891240",
            GridAreaCode: gridAreaCode,
            ValidFrom: DateTime.UtcNow.AddDays(-1).ToInstant(),
            SequenceNumber: 2);

        await using var writeContext = _databaseManager.CreateDbContext();
        await writeContext.GridAreaOwners.AddAsync(existingGridAreaOwner);
        await writeContext.GridAreaOwners.AddAsync(newGridAreaOwnerFromThePast);
        await writeContext.SaveChangesAsync();

        await using var readContext = _databaseManager.CreateDbContext();
        var sut = new GridAreaOwnerRepository(readContext);

        // Act
        var actual = await sut.GetCurrentOwnerAsync(gridAreaCode, CancellationToken.None);

        // Assert
        actual.Should().NotBeNull();
        actual!.OwnerActorNumber.Should().Be(newGridAreaOwnerFromThePast.OwnerActorNumber);
    }

    [Fact]
    public async Task GetCurrentOwner_WhenNewGridAreaEventWithExistingGridAreaCodeAndLowerSequenceNumberValidFromTheFuture_ReturnsExistingOwner()
    {
        // Arrange
        var gridAreaCode = "308";
        var existingGridAreaOwner = new GridAreaOwner(
            Id: Guid.NewGuid(),
            OwnerActorNumber: "1234567891239",
            GridAreaCode: gridAreaCode,
            ValidFrom: DateTime.UtcNow.ToInstant(),
            SequenceNumber: 1);
        var newGridAreaOwnerFromThePast = new GridAreaOwner(
            Id: Guid.NewGuid(),
            OwnerActorNumber: "1234567891240",
            GridAreaCode: gridAreaCode,
            ValidFrom: DateTime.UtcNow.AddDays(1).ToInstant(),
            SequenceNumber: 0);

        await using var writeContext = _databaseManager.CreateDbContext();
        await writeContext.GridAreaOwners.AddAsync(existingGridAreaOwner);
        await writeContext.GridAreaOwners.AddAsync(newGridAreaOwnerFromThePast);
        await writeContext.SaveChangesAsync();

        await using var readContext = _databaseManager.CreateDbContext();
        var sut = new GridAreaOwnerRepository(readContext);

        // Act
        var actual = await sut.GetCurrentOwnerAsync(gridAreaCode, CancellationToken.None);

        // Assert
        actual.Should().NotBeNull();
        actual!.OwnerActorNumber.Should().Be(existingGridAreaOwner.OwnerActorNumber);
    }

    [Fact]
    public async Task GetCurrentOwner_WhenNewGridAreaEventWithExistingGridAreaCodeAndExistingSequenceNumber_ReturnsExistingOwner()
    {
        // Arrange
        var gridAreaCode = "309";
        var existingGridAreaOwner = new GridAreaOwner(
            Id: Guid.NewGuid(),
            OwnerActorNumber: "1234567891239",
            GridAreaCode: gridAreaCode,
            ValidFrom: DateTime.UtcNow.ToInstant(),
            SequenceNumber: 1);
        var newGridAreaOwnerFromThePast = new GridAreaOwner(
            Id: Guid.NewGuid(),
            OwnerActorNumber: "1234567891240",
            GridAreaCode: gridAreaCode,
            ValidFrom: DateTime.UtcNow.AddDays(1).ToInstant(),
            SequenceNumber: 1);

        await using var writeContext = _databaseManager.CreateDbContext();
        await writeContext.GridAreaOwners.AddAsync(existingGridAreaOwner);
        await writeContext.GridAreaOwners.AddAsync(newGridAreaOwnerFromThePast);
        await writeContext.SaveChangesAsync();

        await using var readContext = _databaseManager.CreateDbContext();
        var sut = new GridAreaOwnerRepository(readContext);

        // Act
        var actual = await sut.GetCurrentOwnerAsync(gridAreaCode, CancellationToken.None);

        // Assert
        actual.Should().NotBeNull();
        actual!.OwnerActorNumber.Should().Be(existingGridAreaOwner.OwnerActorNumber);
    }
}
