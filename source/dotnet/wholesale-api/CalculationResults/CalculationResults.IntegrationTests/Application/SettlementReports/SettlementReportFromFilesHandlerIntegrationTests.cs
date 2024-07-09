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

using System.IO.Compression;
using AutoFixture;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Application.SettlementReports;

[Collection(nameof(SettlementReportCollectionFixture))]
public sealed class SettlementReportFromFilesHandlerIntegrationTests : TestBase<SettlementReportFromFilesHandler>
{
    private readonly SettlementReportFileBlobStorageFixture _settlementReportFileBlobStorageFixture;

    public SettlementReportFromFilesHandlerIntegrationTests(
        SettlementReportFileBlobStorageFixture settlementReportFileBlobStorageFixture)
    {
        _settlementReportFileBlobStorageFixture = settlementReportFileBlobStorageFixture;

        var blobContainerClient = settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        Fixture.Inject<ISettlementReportFileRepository>(new SettlementReportFileBlobStorage(blobContainerClient));
    }

    [Fact]
    public async Task CombineAsync_GivenSeveralFiles_ReturnsCompressedReport()
    {
        // Arrange
        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var inputFiles = new GeneratedSettlementReportFileDto[]
        {
            new(requestId, new("fileA.csv", false), "fileA_0.csv"),
            new(requestId, new("fileB.csv", false), "fileB_0.csv"),
            new(requestId, new("fileC.csv", false), "fileC_0.csv"),
        };

        await Task.WhenAll(inputFiles.Select(file => MakeTestFileAsync(file)));

        // Act
        var actual = await Sut.CombineAsync(requestId, inputFiles);

        // Assert
        Assert.Equal(requestId, actual.RequestId);
        Assert.Equal(inputFiles, actual.TemporaryFiles);

        var container = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        var generatedFileBlob = container.GetBlobClient($"settlement-reports/{requestId.Id}/{actual.ReportFileName}");
        var generatedFile = await generatedFileBlob.DownloadContentAsync();

        using var archive = new ZipArchive(generatedFile.Value.Content.ToStream(), ZipArchiveMode.Read);

        foreach (var inputFile in inputFiles)
        {
            var entry = archive.GetEntry(inputFile.FileInfo.FileName);
            Assert.NotNull(entry);

            using var streamReader = new StreamReader(entry.Open());
            var inputFileContents = await streamReader.ReadToEndAsync();
            Assert.Equal($"Content: {inputFile.FileInfo.FileName}\n", inputFileContents);
        }
    }

    [Fact]
    public async Task CombineAsync_GivenSeveralChunks_ReturnsCombinedFile()
    {
        // Arrange
        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var inputFiles = new GeneratedSettlementReportFileDto[]
        {
            new(requestId, new("target_file.csv", false) { FileOffset = 0 }, "fileA_0.csv"),
            new(requestId, new("target_file.csv", false) { FileOffset = 1 }, "fileA_1.csv"),
            new(requestId, new("target_file.csv", false) { FileOffset = 2 }, "fileA_2.csv"),
        };

        await Task.WhenAll(inputFiles.Select(file => MakeTestFileAsync(file)));

        // Act
        var actual = await Sut.CombineAsync(requestId, inputFiles);

        // Assert
        Assert.Equal(requestId, actual.RequestId);
        Assert.Equal(inputFiles, actual.TemporaryFiles);

        var container = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        var generatedFileBlob = container.GetBlobClient($"settlement-reports/{requestId.Id}/{actual.ReportFileName}");
        var generatedFile = await generatedFileBlob.DownloadContentAsync();

        using var archive = new ZipArchive(generatedFile.Value.Content.ToStream(), ZipArchiveMode.Read);
        var combinedEntry = archive.Entries.Single();

        using var streamReader = new StreamReader(combinedEntry.Open());
        var inputFileContents = await streamReader.ReadToEndAsync();
        var expectedContents = string.Concat(Enumerable.Repeat("Content: target_file.csv\n", 3));

        Assert.Equal(expectedContents, inputFileContents);
    }

    [Fact]
    public async Task CombineAsync_GivenLargeChunks_SplitsFilePerMillionRows()
    {
        // Arrange
        const int largeTextFileThreshold = 1_000_000;

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var inputFiles = new GeneratedSettlementReportFileDto[]
        {
            new(requestId, new("target_file.csv", true) { ChunkOffset = 0 }, "fileA_0.csv"),
            new(requestId, new("target_file.csv", true) { ChunkOffset = 1 }, "fileA_1.csv"),
            new(requestId, new("target_file.csv", true) { ChunkOffset = 2 }, "fileA_2.csv"),
        };

        await Task.WhenAll(inputFiles.Select(file => MakeTestFileAsync(file, true)));

        // Act
        var actual = await Sut.CombineAsync(requestId, inputFiles);

        // Assert
        Assert.Equal(requestId, actual.RequestId);
        Assert.Equal(inputFiles, actual.TemporaryFiles);

        var container = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        var generatedFileBlob = container.GetBlobClient($"settlement-reports/{requestId.Id}/{actual.ReportFileName}");
        var generatedFile = await generatedFileBlob.DownloadContentAsync();

        using var archive = new ZipArchive(generatedFile.Value.Content.ToStream(), ZipArchiveMode.Read);
        Assert.Equal(Math.Ceiling(1_500_000.0 * 3.0 / largeTextFileThreshold), archive.Entries.Count);

        var combinedEntry = archive.Entries.Skip(1).First();

        using var streamReader = new StreamReader(combinedEntry.Open());
        var inputFileContents = await streamReader.ReadToEndAsync();
        var expectedContents = string.Concat(Enumerable.Repeat($"Content: target_file.csv{Environment.NewLine}", largeTextFileThreshold));

        Assert.Equal(expectedContents, inputFileContents);
    }

    private Task MakeTestFileAsync(GeneratedSettlementReportFileDto file, bool makeLargeFiles = false)
    {
        var binaryData = new BinaryData($"Content: {file.FileInfo.FileName}\n");

        if (makeLargeFiles)
        {
            binaryData = new BinaryData(string.Concat(Enumerable.Repeat("Content: target_file.csv\n", 1_500_000)));
        }

        var containerClient = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        var blobClient = containerClient.GetBlobClient($"settlement-reports/{file.RequestId.Id}/{file.StorageFileName}");
        return blobClient.UploadAsync(binaryData);
    }
}
