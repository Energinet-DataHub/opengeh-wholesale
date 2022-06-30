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

using Energinet.DataHub.Wholesale.IntegrationTests.Core.Fixtures.Database;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Persistence.Processes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Sender;

public class ProcessRepositoryTests : IClassFixture<SenderDatabaseFixture>
{
    private readonly SenderDatabaseManager _databaseManager;

    public ProcessRepositoryTests(SenderDatabaseFixture fixture)
    {
        _databaseManager = fixture.DatabaseManager;
    }

    [Fact]
    public async Task AddAsync_AddsBatch()
    {
        // Arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var sut = new ProcessRepository(writeContext);
        var expectedProcess = new Process(new MessageHubReference(Guid.NewGuid()), "805");

        // Act
        await sut.AddAsync(expectedProcess);
        await writeContext.SaveChangesAsync();

        // Assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.Processes.SingleAsync(p => p.Id == expectedProcess.Id);

        actual.Should().BeEquivalentTo(expectedProcess);
    }
}
