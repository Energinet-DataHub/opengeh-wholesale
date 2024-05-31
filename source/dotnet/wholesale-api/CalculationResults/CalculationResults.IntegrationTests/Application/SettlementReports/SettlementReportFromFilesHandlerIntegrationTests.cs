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

[Collection(nameof(SettlementReportFileCollectionFixture))]
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
            new(requestId, new("fileA.csv"), "fileA_0.csv"),
            new(requestId, new("fileB.csv"), "fileB_0.csv"),
            new(requestId, new("fileC.csv"), "fileC_0.csv"),
        };

        await Task.WhenAll(inputFiles.Select(MakeTestFileAsync));

        // Act
        var actual = await Sut.CombineAsync(inputFiles);

        // Assert
        Assert.Equal(requestId, actual.RequestId);
        Assert.Equal(inputFiles, actual.TemporaryFiles);

        var container = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        var generatedFileBlob = container.GetBlobClient($"settlement-reports/{requestId.Id}/{actual.FinalReport.StorageFileName}");
        var generatedFile = await generatedFileBlob.DownloadContentAsync();

        using var archive = new ZipArchive(generatedFile.Value.Content.ToStream(), ZipArchiveMode.Read);

        foreach (var inputFile in inputFiles)
        {
            var entry = archive.GetEntry(inputFile.FileInfo.FileName);
            Assert.NotNull(entry);

            using var streamReader = new StreamReader(entry.Open());
            var inputFileContents = await streamReader.ReadToEndAsync();
            Assert.Equal($"Content: {inputFile.FileInfo.FileName}", inputFileContents);
        }
    }

    private Task MakeTestFileAsync(GeneratedSettlementReportFileDto file)
    {
        var containerClient = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        var blobClient = containerClient.GetBlobClient($"settlement-reports/{file.RequestId.Id}/{file.StorageFileName}");
        return blobClient.UploadAsync(new BinaryData($"Content: {file.FileInfo.FileName}"));
    }
}
