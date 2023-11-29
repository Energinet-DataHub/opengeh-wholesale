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
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Persistence.ReceivedIntegrationEvent;
using Energinet.DataHub.Wholesale.Batches.IntegrationTests.Fixture.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Energinet.DataHub.Wholesale.Batches.IntegrationTests.Infrastructure.Persistence.ReceivedIntegrationEvent;

public class ReceivedIntegrationEventTests
{
    private readonly WholesaleDatabaseManager<DatabaseContext> _databaseManager;

    public ReceivedIntegrationEventTests(WholesaleDatabaseFixture<DatabaseContext> fixture)
    {
        _databaseManager = fixture.DatabaseManager;
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task AddAsync_AddsGridAreaOwners(Batches.Application.IntegrationEvents.ReceivedIntegrationEvent expectedIntegrationEvent)
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var sut = new ReceivedIntegrationEventRepository(writeContext);

        // Act
        await sut.CreateAsync(expectedIntegrationEvent.Id, expectedIntegrationEvent.EventType);
        await writeContext.SaveChangesAsync();

        // Assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.ReceivedIntegrationEvents.SingleAsync(b => b.Id.Equals(expectedIntegrationEvent.Id));

        actual.Should().BeEquivalentTo(expectedIntegrationEvent);
    }
}
