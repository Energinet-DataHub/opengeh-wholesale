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
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Application.SettlementReports;

public sealed class UpdateFailedSettlementReportsHandlerIntegrationTests : TestBase<UpdateFailedSettlementReportsHandler>,
    IClassFixture<WholesaleDatabaseFixture<SettlementReportDatabaseContext>>
{
    private readonly WholesaleDatabaseFixture<SettlementReportDatabaseContext> _wholesaleDatabaseFixture;

    public UpdateFailedSettlementReportsHandlerIntegrationTests(
        WholesaleDatabaseFixture<SettlementReportDatabaseContext> wholesaleDatabaseFixture)
    {
        _wholesaleDatabaseFixture = wholesaleDatabaseFixture;
        Fixture.Inject<ISettlementReportRepository>(new SettlementReportRepository(wholesaleDatabaseFixture.DatabaseManager.CreateDbContext()));
    }

    [Fact]
    public async Task UpdateFailedReportAsync_UpdatesReportStatus()
    {
        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var settlementReportRequest = new SettlementReportRequestDto(
            false,
            false,
            false,
            false,
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>(),
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                CalculationType.BalanceFixing,
                null,
                null));

        await using var dbContextArrange = _wholesaleDatabaseFixture.DatabaseManager.CreateDbContext();
        await dbContextArrange.SettlementReports.AddAsync(new SettlementReport(SystemClock.Instance, Guid.NewGuid(), Guid.NewGuid(), false, requestId, settlementReportRequest));
        await dbContextArrange.SaveChangesAsync();

        // Act
        await Sut.UpdateFailedReportAsync(requestId);

        // Assert
        await using var dbContextAssert = _wholesaleDatabaseFixture.DatabaseManager.CreateDbContext();
        var actualReport = await dbContextAssert.SettlementReports.SingleAsync(r => r.RequestId == requestId.Id);
        Assert.Equal(SettlementReportStatus.Failed, actualReport.Status);
    }
}
