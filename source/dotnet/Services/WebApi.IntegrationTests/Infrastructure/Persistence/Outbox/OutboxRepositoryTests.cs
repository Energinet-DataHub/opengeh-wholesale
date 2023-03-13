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
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
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
    public async Task AddAsync_AddsOutboxMessage()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var outboxMessage = CreateOutboxMessage(ProcessType.Aggregation);
        var sut = new OutboxMessageRepository(writeContext);

        // Act
        await sut.AddAsync(outboxMessage);
        await writeContext.SaveChangesAsync();

        // Assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.OutboxMessages.SingleAsync(x => x.Id == outboxMessage.Id);

        actual.Should().BeEquivalentTo(outboxMessage);
    }

    private static OutboxMessage CreateOutboxMessage(ProcessType processType)
    {
        return new OutboxMessage(processType.ToString(), new byte[10], SystemClock.Instance.GetCurrentInstant());
    }
}
