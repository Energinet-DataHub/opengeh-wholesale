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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Test.Core.Fixture.Database;
using Microsoft.EntityFrameworkCore;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Application.SettlementReports;

[Collection(nameof(SettlementReportFileCollectionFixture))]
public sealed class GetSettlementReportsHandlerIntegrationTests : TestBase<GetSettlementReportsHandler>,
    IClassFixture<WholesaleDatabaseFixture<SettlementReportDatabaseContext>>
{
    private readonly WholesaleDatabaseFixture<SettlementReportDatabaseContext> _wholesaleDatabaseFixture;
    private readonly SettlementReportFileBlobStorageFixture _settlementReportFileBlobStorageFixture;

    private readonly SettlementReportRequestDto _mockedSettlementReportRequest = new(
        CalculationType.BalanceFixing,
        false,
        new SettlementReportRequestFilterDto([], DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null, null));

    public GetSettlementReportsHandlerIntegrationTests(
        WholesaleDatabaseFixture<SettlementReportDatabaseContext> wholesaleDatabaseFixture,
        SettlementReportFileBlobStorageFixture settlementReportFileBlobStorageFixture)
    {
        _wholesaleDatabaseFixture = wholesaleDatabaseFixture;
        _settlementReportFileBlobStorageFixture = settlementReportFileBlobStorageFixture;
        Fixture.Inject<ISettlementReportRepository>(new SettlementReportRepository(wholesaleDatabaseFixture.DatabaseManager.CreateDbContext()));

        var blobContainerClient = settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        Fixture.Inject<IRemoveExpiredSettlementReports>(new RemoveExpiredSettlementReports(
            SystemClock.Instance,
            new SettlementReportRepository(wholesaleDatabaseFixture.DatabaseManager.CreateDbContext()),
            new SettlementReportFileBlobStorage(blobContainerClient)));
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
            await dbContext.SettlementReports.AddAsync(CreateMockedSettlementReport(Guid.NewGuid(), Guid.NewGuid(), requestId));
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

        await dbContext.SettlementReports.AddAsync(CreateMockedSettlementReport(targetUserId, Guid.NewGuid(), new SettlementReportRequestId(Guid.NewGuid().ToString())));
        await dbContext.SettlementReports.AddAsync(CreateMockedSettlementReport(targetUserId, Guid.NewGuid(), new SettlementReportRequestId(Guid.NewGuid().ToString())));

        await dbContext.SettlementReports.AddAsync(CreateMockedSettlementReport(targetUserId, targetActorId, requestId));

        await dbContext.SettlementReports.AddAsync(CreateMockedSettlementReport(Guid.NewGuid(), targetActorId, new SettlementReportRequestId(Guid.NewGuid().ToString())));
        await dbContext.SettlementReports.AddAsync(CreateMockedSettlementReport(Guid.NewGuid(), targetActorId, new SettlementReportRequestId(Guid.NewGuid().ToString())));

        await dbContext.SaveChangesAsync();

        // Act
        var items = (await Sut.GetAsync(targetUserId, targetActorId)).ToList();

        // Assert
        Assert.Single(items);
        Assert.Contains(items, item => item.RequestId == requestId);
    }

    [Fact]
    public async Task GetAsync_HasFailedReport_ReportIsRemoved()
    {
        // Arrange
        var clockMock = new Mock<IClock>();
        clockMock
            .Setup(clock => clock.GetCurrentInstant())
            .Returns(Instant.FromUtc(2021, 1, 1, 0, 0));

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var report = new SettlementReport(
            clockMock.Object,
            Guid.NewGuid(),
            Guid.NewGuid(),
            requestId,
            _mockedSettlementReportRequest);

        report.MarkAsFailed();

        await using var dbContext = _wholesaleDatabaseFixture.DatabaseManager.CreateDbContext();
        await dbContext.SettlementReports.AddAsync(report);
        await dbContext.SaveChangesAsync();

        // Act
        var items = (await Sut.GetAsync()).ToList();

        // Assert
        Assert.DoesNotContain(items, item => item.RequestId == requestId);

        await using var assertContext = _wholesaleDatabaseFixture.DatabaseManager.CreateDbContext();
        var actualReport = await assertContext.SettlementReports.SingleOrDefaultAsync(r => r.RequestId == requestId.Id);
        Assert.Null(actualReport);
    }

    [Fact]
    public async Task GetAsync_HasExpiredReport_ReportIsRemoved()
    {
        // Arrange
        var clockMock = new Mock<IClock>();
        clockMock
            .Setup(clock => clock.GetCurrentInstant())
            .Returns(Instant.FromUtc(2021, 1, 1, 0, 0));

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var report = new SettlementReport(
            clockMock.Object,
            Guid.NewGuid(),
            Guid.NewGuid(),
            requestId,
            _mockedSettlementReportRequest);

        var generatedSettlementReportDto = new GeneratedSettlementReportDto(
            requestId,
            new GeneratedSettlementReportFileDto(requestId, new SettlementReportPartialFileInfo("TestFile.csv"), "TestFile.csv"),
            []);

        report.MarkAsCompleted(generatedSettlementReportDto);

        await using var dbContext = _wholesaleDatabaseFixture.DatabaseManager.CreateDbContext();
        await dbContext.SettlementReports.AddAsync(report);
        await dbContext.SaveChangesAsync();

        var blobClient = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        var blobName = $"settlement-reports/{requestId.Id}/{generatedSettlementReportDto.FinalReport.StorageFileName}";
        await blobClient.UploadBlobAsync(blobName, new BinaryData("data"));

        // Act
        var items = (await Sut.GetAsync()).ToList();

        // Assert
        Assert.DoesNotContain(items, item => item.RequestId == requestId);

        await using var assertContext = _wholesaleDatabaseFixture.DatabaseManager.CreateDbContext();
        var actualReport = await assertContext.SettlementReports.SingleOrDefaultAsync(r => r.RequestId == requestId.Id);
        Assert.Null(actualReport);

        Assert.False(await blobClient.GetBlobClient(blobName).ExistsAsync());
    }

    private SettlementReport CreateMockedSettlementReport(Guid userId, Guid actorId, SettlementReportRequestId requestId)
    {
        return new SettlementReport(SystemClock.Instance, userId, actorId, requestId, _mockedSettlementReportRequest);
    }
}
