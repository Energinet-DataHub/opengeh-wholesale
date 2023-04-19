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

using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.EventDispatching;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.DomainEvents;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestCommon.Fixture.Database;
using FluentAssertions;
using NodaTime;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Infrastructure.EventDispatching;

public class DomainEventContainerTests : IClassFixture<WholesaleDatabaseFixture>
{
    private readonly WholesaleDatabaseManager _databaseManager;

    public DomainEventContainerTests(WholesaleDatabaseFixture fixture)
    {
        _databaseManager = fixture.DatabaseManager;
    }

    [Fact]
    public void GetAllDomainEvents_WhenCalled_ReturnsDomainEvents()
    {
        // Arrange
        var databaseContext = _databaseManager.CreateDbContext();
        var batch = CreateBatch(ProcessType.BalanceFixing, new List<GridAreaCode> { new("805") });
        databaseContext.Batches.Add(batch);

        var sut = new DomainEventRepository(databaseContext);

        // Act
        var actual = sut.GetAllDomainEvents();

        // Assert
        actual.Should().NotBeEmpty();
    }

    [Fact]
    public void GetAllDomainEvents_WhenCalledFromNewDbContext_ReturnsEmpty()
    {
        // Arrange
        var writeContext = _databaseManager.CreateDbContext();
        var readContext = _databaseManager.CreateDbContext();

        var batch = CreateBatch(ProcessType.BalanceFixing, new List<GridAreaCode> { new("805") });
        writeContext.Batches.Add(batch);

        var sut = new DomainEventRepository(readContext);

        // Act
        var actual = sut.GetAllDomainEvents();

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public void GetAllDomainEvents_WhenCalledAfterClearAllDomainEvents_ReturnsEmpty()
    {
        // Arrange
        var databaseContext = _databaseManager.CreateDbContext();
        var batch = CreateBatch(ProcessType.BalanceFixing, new List<GridAreaCode> { new("805") });
        databaseContext.Batches.Add(batch);

        var sut = new DomainEventRepository(databaseContext);

        // Act
        sut.ClearAllDomainEvents();

        // Assert
        var actual = sut.GetAllDomainEvents();
        actual.Should().BeEmpty();
    }

    private static Domain.BatchAggregate.Batch CreateBatch(ProcessType processType, List<GridAreaCode> someGridAreasIds)
    {
        var period = Periods.January_EuropeCopenhagen_Instant;
        return new Domain.BatchAggregate.Batch(
            processType,
            someGridAreasIds,
            period.PeriodStart,
            period.PeriodEnd,
            SystemClock.Instance.GetCurrentInstant(),
            period.DateTimeZone);
    }
}
