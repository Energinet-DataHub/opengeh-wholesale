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

using AutoFixture;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Persistence.SettlementReportRequest;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Test.Core.Fixture.Database;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Application.SettlementReports;

public sealed class GetSettlementReportsHandlerIntegrationTests : TestBase<GetSettlementReportsHandler>,
    IClassFixture<WholesaleDatabaseFixture<SettlementReportDatabaseContext>>
{
    private readonly WholesaleDatabaseFixture<SettlementReportDatabaseContext> _wholesaleDatabaseFixture;

    private readonly SettlementReportRequestDto _mockedSettlementReportRequest = new(
        CalculationType.BalanceFixing,
        false,
        new SettlementReportRequestFilterDto([], DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null));

    public GetSettlementReportsHandlerIntegrationTests(
        WholesaleDatabaseFixture<SettlementReportDatabaseContext> wholesaleDatabaseFixture)
    {
        _wholesaleDatabaseFixture = wholesaleDatabaseFixture;
        Fixture.Inject<ISettlementReportRepository>(new SettlementReportRepository(wholesaleDatabaseFixture.DatabaseManager.CreateDbContext()));
    }

    [Fact]
    public async Task GetAsync_MultiTenancy_ReturnsAllRows()
    {
        var expectedReports = new List<SettlementReportRequestId>();

        await using var dbContext = _wholesaleDatabaseFixture.DatabaseManager.CreateDbContext();

        for (var i = 0; i < 5; i++)
        {
            var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
            expectedReports.Add(requestId);
            await dbContext.SettlementReports.AddAsync(new SettlementReport(Guid.NewGuid(), Guid.NewGuid(), requestId, _mockedSettlementReportRequest));
        }

        await dbContext.SaveChangesAsync();

        // Act
        var items = (await Sut.GetAsync()).ToList();

        // Assert
        foreach (var expectedReport in expectedReports)
        {
            Assert.Contains(items, item => item.RequestId == expectedReport);
        }
    }

    [Fact]
    public async Task GetAsync_SingleUser_ReturnsOwnRows()
    {
        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var targetUserId = Guid.NewGuid();
        var targetActorId = Guid.NewGuid();

        await using var dbContext = _wholesaleDatabaseFixture.DatabaseManager.CreateDbContext();

        await dbContext.SettlementReports.AddAsync(new SettlementReport(targetUserId, Guid.NewGuid(), new SettlementReportRequestId(Guid.NewGuid().ToString()), _mockedSettlementReportRequest));
        await dbContext.SettlementReports.AddAsync(new SettlementReport(targetUserId, Guid.NewGuid(), new SettlementReportRequestId(Guid.NewGuid().ToString()), _mockedSettlementReportRequest));

        await dbContext.SettlementReports.AddAsync(new SettlementReport(targetUserId, targetActorId, requestId, _mockedSettlementReportRequest));

        await dbContext.SettlementReports.AddAsync(new SettlementReport(Guid.NewGuid(), targetActorId, new SettlementReportRequestId(Guid.NewGuid().ToString()), _mockedSettlementReportRequest));
        await dbContext.SettlementReports.AddAsync(new SettlementReport(Guid.NewGuid(), targetActorId, new SettlementReportRequestId(Guid.NewGuid().ToString()), _mockedSettlementReportRequest));

        await dbContext.SaveChangesAsync();

        // Act
        var items = (await Sut.GetAsync(targetUserId, targetActorId)).ToList();

        // Assert
        Assert.Single(items);
        Assert.Contains(items, item => item.RequestId == requestId);
    }
}
