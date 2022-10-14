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

using System.Diagnostics;
using System.IO.Compression;
using Azure.Storage.Blobs;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.BasisData;
using Energinet.DataHub.Wholesale.IntegrationTests.Hosts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Test.Core;
using Xunit;

// TODO: Tests:
// - Creates zip without missing files
// - Logs error when files are missing
namespace Energinet.DataHub.Wholesale.IntegrationTests.ProcessManager;

// TODO: Why collection as a string?
[Collection("ProcessManagerIntegrationTest")]
public sealed class BasisDataApplicationServiceTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task When_BatchIsCompleted_Then_CalculationFilesAreZipped(BatchCompletedEventDto batchCompletedEvent)
    {
        // Arrange
        using var host = await ProcessManagerIntegrationTestHost.CreateAsync();

        var gridAreaCode = new GridAreaCode("805");
        var batch = new Batch(ProcessType.BalanceFixing, new[] { gridAreaCode }, SystemClock.Instance.GetCurrentInstant(), SystemClock.Instance.GetCurrentInstant(), SystemClock.Instance);
        batch.SetPrivateProperty(b => b.Id, batchCompletedEvent.BatchId);

        // TODO: Create all calculation files (basis data and result)
        var blobContainerClient =
            new BlobContainerClient(
                ProcessManagerIntegrationTestHost.CalculationStorageConnectionString,
                ProcessManagerIntegrationTestHost.CalculationStorageContainerName);
        await blobContainerClient.CreateIfNotExistsAsync();
        var (directory, extension, masterDataEntryPath) = BatchFileManager.GetMasterBasisDataDirectory(batchCompletedEvent.BatchId, gridAreaCode);
        var blobClient = blobContainerClient.GetBlobClient($"{directory}/master-basis-data{extension}");
        await blobClient.UploadAsync(new BinaryData("master basis data"));

        // TODO: Why create this scope?
        await using var scope = host.BeginScope();
        var sut = scope.ServiceProvider.GetRequiredService<IBasisDataApplicationService>();

        // Act
        await sut.ZipBasisDataAsync(batchCompletedEvent);

        // Assert
        var zipBlobName = BatchFileManager.GetZipBlobName(batch);
        var zipBlobClient = blobContainerClient.GetBlobClient(zipBlobName);
        var zipFilePath = Path.GetTempFileName();
        await using (var stream = File.OpenWrite(zipFilePath))
        {
            await zipBlobClient.DownloadToAsync(stream);
        }

        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        ZipFile.ExtractToDirectory(zipFilePath, dir);
        Debugger.Break();
        File.Exists(Path.Combine(dir, masterDataEntryPath)).Should().BeTrue();
        // TODO: assert expected content
    }
}
