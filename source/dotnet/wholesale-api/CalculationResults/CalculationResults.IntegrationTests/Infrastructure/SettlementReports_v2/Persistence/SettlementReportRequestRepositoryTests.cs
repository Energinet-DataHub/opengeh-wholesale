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

using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Persistence.SettlementReportRequest;
using Energinet.DataHub.Wholesale.Test.Core.Fixture.Database;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SettlementReports_v2.Persistence;

public class SettlementReportRequestRepositoryTests : IClassFixture<WholesaleDatabaseFixture<DatabaseContext>>
{
    private readonly WholesaleDatabaseManager<DatabaseContext> _databaseManager;

    public SettlementReportRequestRepositoryTests(WholesaleDatabaseFixture<DatabaseContext> fixture)
    {
        _databaseManager = fixture.DatabaseManager;
    }

    [Fact]
    public async Task AddOrUpdateAsync_ValidRequest_PersistsChanges()
    {
        // arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var target = new SettlementReportRequestRepository(writeContext);
        var settlementReportRequest = new SettlementReportRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString());

        // act
        await target.AddOrUpdateAsync(settlementReportRequest);

        // assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.SettlementReportRequests.SingleOrDefaultAsync(x => x.Id == settlementReportRequest.Id);
        Assert.NotNull(actual);
        Assert.Equal(settlementReportRequest.Id, actual.Id);
        Assert.Equal(settlementReportRequest.UserId, actual.UserId);
        Assert.Equal(settlementReportRequest.ActorId, actual.ActorId);
        Assert.Equal(settlementReportRequest.RequestId, actual.RequestId);
        Assert.Equal(settlementReportRequest.CreatedDateTime, actual.CreatedDateTime);
        Assert.Equal(settlementReportRequest.Status, actual.Status);
        Assert.Equal(settlementReportRequest.BlobFilename, actual.BlobFilename);
    }

    [Fact]
    public async Task GetAsync_RequestExistsWithSuppliedRequestId_ReturnsRequest()
    {
        // arrange
        var expectedRequest = await PrepareNewRequestAsync();

        await using var context = _databaseManager.CreateDbContext();
        var repository = new SettlementReportRequestRepository(context);

        // act
        var actual = await repository.GetAsync(expectedRequest.RequestId);

        // assert
        Assert.NotNull(actual);
        Assert.Equal(expectedRequest.Id, actual.Id);
    }

    [Fact]
    public async Task GetAsync_RequestsExists_ReturnsRequests()
    {
        // arrange
        IEnumerable<SettlementReportRequest> preparedRequests =
        [
            await PrepareNewRequestAsync(),
            await PrepareNewRequestAsync(),
        ];

        await using var context = _databaseManager.CreateDbContext();
        var repository = new SettlementReportRequestRepository(context);

        // act
        var actual = (await repository.GetAsync()).ToList();

        // assert
        foreach (var request in preparedRequests)
        {
            Assert.Contains(actual, x => x.Id == request.Id);
        }
    }

    [Fact]
    public async Task GetAsync_UserIdActorIdMatches_ReturnsRequests()
    {
        // arrange
        await PrepareNewRequestAsync();
        await PrepareNewRequestAsync();

        var expectedRequest = await PrepareNewRequestAsync();

        await using var context = _databaseManager.CreateDbContext();
        var repository = new SettlementReportRequestRepository(context);

        // act
        var actual = (await repository.GetAsync(expectedRequest.UserId, expectedRequest.ActorId)).ToList();

        // assert
        Assert.Single(actual);
        Assert.Equal(expectedRequest.Id, actual[0].Id);
    }

    private async Task<SettlementReportRequest> PrepareNewRequestAsync()
    {
        await using var setupContext = _databaseManager.CreateDbContext();
        var setupRepository = new SettlementReportRequestRepository(setupContext);
        var settlementReportRequest = new SettlementReportRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString());
        await setupRepository.AddOrUpdateAsync(settlementReportRequest);
        return settlementReportRequest;
    }
}
