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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Test.Core.Fixture.Database;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SettlementReports_v2.Persistence;

public class SettlementReportRepositoryTests : IClassFixture<WholesaleDatabaseFixture<SettlementReportDatabaseContext>>
{
    private readonly WholesaleDatabaseManager<SettlementReportDatabaseContext> _databaseManager;

    public SettlementReportRepositoryTests(WholesaleDatabaseFixture<SettlementReportDatabaseContext> fixture)
    {
        _databaseManager = fixture.DatabaseManager;
    }

    [Fact]
    public async Task AddOrUpdateAsync_ValidRequest_PersistsChanges()
    {
        // arrange
        await using var writeContext = _databaseManager.CreateDbContext();
        var target = new SettlementReportRepository(writeContext);

        var calculationFilter = new Dictionary<string, CalculationId?>
        {
            { "805", new CalculationId(Guid.Parse("D116DD8A-898E-48F1-8200-D31D12F82545")) },
            { "806", new CalculationId(Guid.Parse("D116DD8A-898E-48F1-8200-D31D12F82545")) },
        };

        var requestFilterDto = new SettlementReportRequestFilterDto(
            calculationFilter,
            new DateTimeOffset(2024, 1, 1, 22, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 2, 1, 22, 0, 0, TimeSpan.Zero),
            CalculationType.BalanceFixing,
            null,
            null);

        var settlementReportRequest = new SettlementReport(
            SystemClock.Instance,
            Guid.NewGuid(),
            Guid.NewGuid(),
            false,
            new SettlementReportRequestId(Guid.NewGuid().ToString()),
            new SettlementReportRequestDto(false, false, false, false, requestFilterDto));

        // act
        await target.AddOrUpdateAsync(settlementReportRequest);

        // assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.SettlementReports.SingleOrDefaultAsync(x => x.Id == settlementReportRequest.Id);

        Assert.NotNull(actual);
        Assert.Equal(settlementReportRequest.Id, actual.Id);
        Assert.Equal(settlementReportRequest.RequestId, actual.RequestId);
        Assert.Equal(settlementReportRequest.UserId, actual.UserId);
        Assert.Equal(settlementReportRequest.ActorId, actual.ActorId);
        Assert.Equal(settlementReportRequest.CreatedDateTime, actual.CreatedDateTime);
        Assert.Equal(settlementReportRequest.CalculationType, actual.CalculationType);
        Assert.Equal(settlementReportRequest.ContainsBasisData, actual.ContainsBasisData);
        Assert.Equal(settlementReportRequest.PeriodStart, actual.PeriodStart);
        Assert.Equal(settlementReportRequest.PeriodEnd, actual.PeriodEnd);
        Assert.Equal(settlementReportRequest.GridAreaCount, actual.GridAreaCount);
        Assert.Equal(settlementReportRequest.Status, actual.Status);
        Assert.Equal(settlementReportRequest.BlobFileName, actual.BlobFileName);
    }

    [Fact]
    public async Task DeleteAsync_GivenRequest_RequestIsDeleted()
    {
        // Arrange
        var calculationFilter = new Dictionary<string, CalculationId?>
        {
            { "805", new CalculationId(Guid.Parse("D116DD8A-898E-48F1-8200-D31D12F82545")) },
            { "806", new CalculationId(Guid.Parse("D116DD8A-898E-48F1-8200-D31D12F82545")) },
        };

        var requestFilterDto = new SettlementReportRequestFilterDto(
            calculationFilter,
            new DateTimeOffset(2024, 1, 1, 22, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 2, 1, 22, 0, 0, TimeSpan.Zero),
            CalculationType.BalanceFixing,
            null,
            null);

        var settlementReport = new SettlementReport(
            SystemClock.Instance,
            Guid.NewGuid(),
            Guid.NewGuid(),
            false,
            new SettlementReportRequestId(Guid.NewGuid().ToString()),
            new SettlementReportRequestDto(false, false, false, false, requestFilterDto));

        await using var writeContext = _databaseManager.CreateDbContext();
        var arrangeRepository = new SettlementReportRepository(writeContext);
        await arrangeRepository.AddOrUpdateAsync(settlementReport);

        await using var deleteContext = _databaseManager.CreateDbContext();
        var target = new SettlementReportRepository(deleteContext);

        // Act
        await target.DeleteAsync(settlementReport);

        // Assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.SettlementReports.SingleOrDefaultAsync(x => x.Id == settlementReport.Id);
        Assert.Null(actual);
    }

    [Fact]
    public async Task GetAsync_RequestExistsWithSuppliedRequestId_ReturnsRequest()
    {
        // arrange
        var expectedRequest = await PrepareNewRequestAsync();

        await using var context = _databaseManager.CreateDbContext();
        var repository = new SettlementReportRepository(context);

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
        IEnumerable<SettlementReport> preparedRequests =
        [
            await PrepareNewRequestAsync(),
            await PrepareNewRequestAsync(),
        ];

        await using var context = _databaseManager.CreateDbContext();
        var repository = new SettlementReportRepository(context);

        // act
        var actual = (await repository.GetAsync()).ToList();

        // assert
        foreach (var request in preparedRequests)
        {
            Assert.Contains(actual, x => x.Id == request.Id);
        }
    }

    [Fact]
    public async Task GetAsync_ActorIdMatches_ReturnsRequests()
    {
        // arrange
        await PrepareNewRequestAsync();
        await PrepareNewRequestAsync();

        var expectedRequest = await PrepareNewRequestAsync();

        await using var context = _databaseManager.CreateDbContext();
        var repository = new SettlementReportRepository(context);

        // act
        var actual = (await repository.GetAsync(expectedRequest.ActorId)).ToList();

        // assert
        Assert.Single(actual);
        Assert.Equal(expectedRequest.Id, actual[0].Id);
    }

    [Fact]
    public async Task GetAsync_HiddenReport_ReturnsRequests()
    {
        // Arrange
        var expectedRequest = await PrepareNewRequestAsync();
        await PrepareNewRequestAsync(requestFilterDto => new SettlementReport(
            SystemClock.Instance,
            Guid.NewGuid(),
            Guid.NewGuid(),
            true,
            new SettlementReportRequestId(Guid.NewGuid().ToString()),
            new SettlementReportRequestDto(false, false, false, false, requestFilterDto)));

        await using var context = _databaseManager.CreateDbContext();
        var repository = new SettlementReportRepository(context);

        // Act
        var actual = (await repository.GetAsync(expectedRequest.ActorId)).ToList();

        // Assert
        Assert.Single(actual);
        Assert.Equal(expectedRequest.Id, actual[0].Id);
    }

    private async Task<SettlementReport> PrepareNewRequestAsync(Func<SettlementReportRequestFilterDto, SettlementReport>? createReport = null)
    {
        await using var setupContext = _databaseManager.CreateDbContext();
        var setupRepository = new SettlementReportRepository(setupContext);

        var calculationFilter = new Dictionary<string, CalculationId?>
        {
            { "805", new CalculationId(Guid.Parse("D116DD8A-898E-48F1-8200-D31D12F82545")) },
            { "806", new CalculationId(Guid.Parse("D116DD8A-898E-48F1-8200-D31D12F82545")) },
        };

        var requestFilterDto = new SettlementReportRequestFilterDto(
            calculationFilter,
            new DateTimeOffset(2024, 1, 1, 22, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 2, 1, 22, 0, 0, TimeSpan.Zero),
            CalculationType.BalanceFixing,
            null,
            null);

        var settlementReportRequest = new SettlementReport(
            SystemClock.Instance,
            Guid.NewGuid(),
            Guid.NewGuid(),
            false,
            new SettlementReportRequestId(Guid.NewGuid().ToString()),
            new SettlementReportRequestDto(false, false, false, false, requestFilterDto));

        if (createReport != null)
        {
            settlementReportRequest = createReport(requestFilterDto);
        }

        await setupRepository.AddOrUpdateAsync(settlementReportRequest);
        return settlementReportRequest;
    }
}
