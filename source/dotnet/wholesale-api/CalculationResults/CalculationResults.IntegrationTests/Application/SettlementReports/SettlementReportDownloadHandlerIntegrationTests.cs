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
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Application.SettlementReports;

[Collection(nameof(SettlementReportFileCollectionFixture))]
public sealed class SettlementReportDownloadHandlerIntegrationTests : TestBase<SettlementReportDownloadHandler>,
    IClassFixture<WholesaleDatabaseFixture<SettlementReportDatabaseContext>>
{
    private readonly WholesaleDatabaseFixture<SettlementReportDatabaseContext> _wholesaleDatabaseFixture;
    private readonly SettlementReportFileBlobStorageFixture _settlementReportFileBlobStorageFixture;

    private readonly SettlementReportRequestDto _mockedSettlementReportRequest = new(
        CalculationType.BalanceFixing,
        false,
        new SettlementReportRequestFilterDto(new Dictionary<GridAreaCode, CalculationId>(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null, null));

    public SettlementReportDownloadHandlerIntegrationTests(
        WholesaleDatabaseFixture<SettlementReportDatabaseContext> wholesaleDatabaseFixture,
        SettlementReportFileBlobStorageFixture settlementReportFileBlobStorageFixture)
    {
        _wholesaleDatabaseFixture = wholesaleDatabaseFixture;
        _settlementReportFileBlobStorageFixture = settlementReportFileBlobStorageFixture;

        Fixture.Inject<ISettlementReportRepository>(new SettlementReportRepository(wholesaleDatabaseFixture.DatabaseManager.CreateDbContext()));

        var blobContainerClient = settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        Fixture.Inject<ISettlementReportFileRepository>(new SettlementReportFileBlobStorage(blobContainerClient));
    }

    [Fact]
    public async Task DownloadAsync_ReturnsStream()
    {
        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        await MakeTestFileAsync(requestId);

        var generatedSettlementReport = new GeneratedSettlementReportDto(
            requestId,
            "Report.zip",
            []);

        var userId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var settlementReport = new SettlementReport(SystemClock.Instance, userId, actorId, requestId, _mockedSettlementReportRequest);
        settlementReport.MarkAsCompleted(generatedSettlementReport);

        await using var dbContext = _wholesaleDatabaseFixture.DatabaseManager.CreateDbContext();
        await dbContext.SettlementReports.AddAsync(settlementReport);
        await dbContext.SaveChangesAsync();

        // Act
        await using var downloadStream = new MemoryStream();
        await Sut.DownloadReportAsync(requestId, downloadStream, userId, actorId, false);

        // Assert
        Assert.NotNull(downloadStream);
        Assert.NotEqual(0, downloadStream.Length);
    }

    [Fact]
    public async Task DownloadAsync_NoAccess_ThrowsException()
    {
        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        await MakeTestFileAsync(requestId);

        var generatedSettlementReport = new GeneratedSettlementReportDto(
            requestId,
            "Report.zip",
            []);

        var userId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var settlementReport =
            new SettlementReport(SystemClock.Instance, userId, actorId, requestId, _mockedSettlementReportRequest);
        settlementReport.MarkAsCompleted(generatedSettlementReport);

        await using var dbContext = _wholesaleDatabaseFixture.DatabaseManager.CreateDbContext();
        await dbContext.SettlementReports.AddAsync(settlementReport);
        await dbContext.SaveChangesAsync();

        // Act + Assert
        await using var downloadStream = new MemoryStream();
        await Assert.ThrowsAsync<InvalidOperationException>(() => Sut.DownloadReportAsync(requestId, downloadStream, Guid.NewGuid(), Guid.NewGuid(), false));
    }

    [Fact]
    public async Task DownloadAsync_AsFAS_ReturnsStream()
    {
        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        await MakeTestFileAsync(requestId);

        var generatedSettlementReport = new GeneratedSettlementReportDto(
            requestId,
            "Report.zip",
            []);

        var userId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var settlementReport =
            new SettlementReport(SystemClock.Instance, userId, actorId, requestId, _mockedSettlementReportRequest);
        settlementReport.MarkAsCompleted(generatedSettlementReport);

        await using var dbContext = _wholesaleDatabaseFixture.DatabaseManager.CreateDbContext();
        await dbContext.SettlementReports.AddAsync(settlementReport);
        await dbContext.SaveChangesAsync();

        // Act
        await using var downloadStream = new MemoryStream();
        await Sut.DownloadReportAsync(requestId, downloadStream, Guid.NewGuid(), Guid.NewGuid(), true);

        // Assert
        Assert.NotNull(downloadStream);
        Assert.NotEqual(0, downloadStream.Length);
    }

    private Task MakeTestFileAsync(SettlementReportRequestId requestId)
    {
        var containerClient = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        var blobClient = containerClient.GetBlobClient($"settlement-reports/{requestId.Id}/Report.zip");
        return blobClient.UploadAsync(new BinaryData("Content: Report.zip"));
    }
}
