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

using Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence.ReceivedIntegrationEvent;
using Energinet.DataHub.Wholesale.Batches.IntegrationTests.Fixture.Database;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Batches.IntegrationTests.Infrastructure.Persistence.ReceivedIntegrationEvent;

public class ReceivedIntegrationEventTests : IClassFixture<WholesaleDatabaseFixture<DatabaseContext>>
{
    private readonly WholesaleDatabaseManager<DatabaseContext> _databaseManager;

    public ReceivedIntegrationEventTests(WholesaleDatabaseFixture<DatabaseContext> fixture)
    {
        _databaseManager = fixture.DatabaseManager;
    }

    [Fact]
    public async Task AddAsync_AddsIntegrationEventId()
    {
        // Arrange
        var eventType = "Test";
        var id = Guid.NewGuid();

        await using var writeContext = _databaseManager.CreateDbContext();
        var sut = new ReceivedIntegrationEventRepository(writeContext);

        // Act
        await sut.AddAsync(id, eventType);
        await writeContext.SaveChangesAsync();

        // Assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.ReceivedIntegrationEvents.SingleAsync(b => b.Id.Equals(id));

        using var assertionScope = new AssertionScope();
        actual.Should().NotBeNull();
        actual.Id.Should().Be(id);
        actual.EventType.Should().Be(eventType);
    }

    [Fact]
    public async Task AddAsync_WhenReceivingTheSameEventTwice_ThrowsExceptionOnSecondEventRegistration()
    {
        // Arrange
        var eventType = "Test";
        var id = Guid.NewGuid();

        var writeContext = _databaseManager.CreateDbContext();
        var sut = new ReceivedIntegrationEventRepository(writeContext);

        // Act
        await sut.AddAsync(id, eventType);
        await writeContext.SaveChangesAsync();
        var act = () => sut.AddAsync(id, eventType);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AddAsync_WhenReceivingTheSameEventTwiceOnDifferentContexts_ThrowsExceptionOnSecondEventRegistration()
    {
        // Arrange
        var eventType = "Test";
        var id = Guid.NewGuid();

        var writeContext = _databaseManager.CreateDbContext();
        var sut = new ReceivedIntegrationEventRepository(writeContext);

        // Act
        await sut.AddAsync(id, eventType);
        await writeContext.SaveChangesAsync();

        writeContext = _databaseManager.CreateDbContext();
        sut = new ReceivedIntegrationEventRepository(writeContext);
        await sut.AddAsync(id, eventType);
        var act = () => writeContext.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task ExistsAsync_WhenEventDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();

        await using var writeContext = _databaseManager.CreateDbContext();
        var sut = new ReceivedIntegrationEventRepository(writeContext);

        // Act
        var actual = await sut.ExistsAsync(id);

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WhenEventDoExist_ReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();

        await using var writeContext = _databaseManager.CreateDbContext();
        writeContext.ReceivedIntegrationEvents.Add(new Application.IntegrationEvents.ReceivedIntegrationEvent(id, "Test", SystemClock.Instance.GetCurrentInstant()));
        await writeContext.SaveChangesAsync();

        await using var readContext = _databaseManager.CreateDbContext();
        var sut = new ReceivedIntegrationEventRepository(readContext);

        // Act
        var actual = await sut.ExistsAsync(id);

        // Assert
        actual.Should().BeTrue();
    }
}
