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

using Energinet.DataHub.Wholesale.Domain;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestCommon.Fixture.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Infrastructure.Persistence.Outbox;

public class OutboxRepositoryTests : IClassFixture<WholesaleDatabaseFixture>
{
    private readonly WholesaleDatabaseManager _databaseManager;

    public OutboxRepositoryTests(WholesaleDatabaseFixture fixture)
    {
        _databaseManager = fixture.DatabaseManager;
    }

    [Fact]
    public async Task AddAsync_AddsAnOutboxMessage()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var expected = CreateOutOutboxMessage("type1");
        var sut = new OutboxMessageRepository(writeContext);

        // Act
        await sut.AddAsync(expected, default);
        await writeContext.SaveChangesAsync();

        // Assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.OutboxMessages.SingleAsync(x => x.Id == expected.Id);

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetByTakeAsync_WhenTwo_ReturnsTwoOutboxMessages()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var outboxMessage1 = CreateOutOutboxMessage("type1", 15);
        var outboxMessage2 = CreateOutOutboxMessage("type2", 14);
        var outboxMessage3 = CreateOutOutboxMessage("type3", 13);
        var expected = new List<OutboxMessage> { outboxMessage1, outboxMessage2 };
        var sut = new OutboxMessageRepository(writeContext);
        await sut.AddAsync(outboxMessage1, default);
        await sut.AddAsync(outboxMessage2, default);
        await sut.AddAsync(outboxMessage3, default);
        await writeContext.SaveChangesAsync();

        // Act
        var actual = await sut.GetByTakeAsync(2, default);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task DeleteByCreationDateAsync_ReturnsVoid()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var outboxMessage1 = CreateOutOutboxMessage("type1", 16);
        var outboxMessage2 = CreateOutOutboxMessage("type2", 14);
        var outboxMessage3 = CreateOutOutboxMessage("type3", 13);
        var expected = new List<OutboxMessage> { outboxMessage3 };
        var sut = new OutboxMessageRepository(writeContext);
        await sut.AddAsync(outboxMessage1, default);
        await sut.AddAsync(outboxMessage2, default);
        await sut.AddAsync(outboxMessage3, default);
        await writeContext.SaveChangesAsync();

        // Act
        sut.DeleteByCreationDate(SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(14)));
        await writeContext.SaveChangesAsync();

        // Assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = readContext.OutboxMessages;
        actual.Should().BeEquivalentTo(expected);
    }

    private static OutboxMessage CreateOutOutboxMessage(string type, int numberOfDays = 0)
    {
        return new OutboxMessage(type, new byte[10], SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(numberOfDays)));
    }
}
