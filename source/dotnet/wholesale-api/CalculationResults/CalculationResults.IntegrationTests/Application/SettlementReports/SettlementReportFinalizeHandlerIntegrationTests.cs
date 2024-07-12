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
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Application.SettlementReports;

[Collection(nameof(SettlementReportCollectionFixture))]
public sealed class SettlementReportFinalizeHandlerIntegrationTests : TestBase<SettlementReportFinalizeHandler>,
    IClassFixture<WholesaleDatabaseFixture<SettlementReportDatabaseContext>>
{
    private readonly WholesaleDatabaseFixture<SettlementReportDatabaseContext> _wholesaleDatabaseFixture;
    private readonly SettlementReportFileBlobStorageFixture _settlementReportFileBlobStorageFixture;

    private readonly SettlementReportRequestDto _mockedSettlementReportRequest = new(
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

    public SettlementReportFinalizeHandlerIntegrationTests(
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
    public async Task FinalizeAsync_WithInputFiles_RemovesInputFiles()
    {
        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var inputFiles = new GeneratedSettlementReportFileDto[]
        {
            new(requestId, new("fileA.csv", true), "fileA_0.csv"),
            new(requestId, new("fileB.csv", true), "fileB_0.csv"),
            new(requestId, new("fileC.csv", true), "fileC_0.csv"),
        };

        await Task.WhenAll(inputFiles.Select(MakeTestFileAsync));

        var generatedSettlementReport = new GeneratedSettlementReportDto(
            requestId,
            "Report.zip",
            inputFiles);

        await using var dbContext = _wholesaleDatabaseFixture.DatabaseManager.CreateDbContext();
        await dbContext.SettlementReports.AddAsync(new SettlementReport(SystemClock.Instance, Guid.NewGuid(), Guid.NewGuid(), false, requestId, _mockedSettlementReportRequest));
        await dbContext.SaveChangesAsync();

        // Act
        await Sut.FinalizeAsync(generatedSettlementReport);

        // Assert
        var container = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();

        foreach (var inputFile in inputFiles)
        {
            var generatedFileBlob = container.GetBlobClient($"settlement-reports/{requestId.Id}/{inputFile.StorageFileName}");
            Assert.False(await generatedFileBlob.ExistsAsync());
        }
    }

    [Fact]
    public async Task FinalizeAsync_CompletesReportRequest()
    {
        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());

        var generatedSettlementReport = new GeneratedSettlementReportDto(
            requestId,
            "Report.zip",
            []);

        await using var dbContextArrange = _wholesaleDatabaseFixture.DatabaseManager.CreateDbContext();
        await dbContextArrange.SettlementReports.AddAsync(new SettlementReport(SystemClock.Instance, Guid.NewGuid(), Guid.NewGuid(), false, requestId, _mockedSettlementReportRequest));
        await dbContextArrange.SaveChangesAsync();

        // Act
        await Sut.FinalizeAsync(generatedSettlementReport);

        // Assert
        await using var dbContextAct = _wholesaleDatabaseFixture.DatabaseManager.CreateDbContext();
        var completedRequest = await dbContextAct.SettlementReports.SingleAsync(r => r.RequestId == requestId.Id);
        Assert.Equal(SettlementReportStatus.Completed, completedRequest.Status);
    }

    private Task MakeTestFileAsync(GeneratedSettlementReportFileDto file)
    {
        var containerClient = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        var blobClient = containerClient.GetBlobClient($"settlement-reports/{file.RequestId.Id}/{file.StorageFileName}");
        return blobClient.UploadAsync(new BinaryData($"Content: {file.FileInfo.FileName}"));
    }
}
