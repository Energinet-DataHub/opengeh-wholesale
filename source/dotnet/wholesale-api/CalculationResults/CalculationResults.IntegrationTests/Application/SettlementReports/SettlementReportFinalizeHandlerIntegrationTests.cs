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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Application.SettlementReports;

public sealed class SettlementReportFinalizeHandlerIntegrationTests : TestBase<SettlementReportFinalizeHandler>,
    IClassFixture<SettlementReportFileBlobStorageFixture>
{
    private readonly SettlementReportFileBlobStorageFixture _settlementReportFileBlobStorageFixture;

    public SettlementReportFinalizeHandlerIntegrationTests(
        SettlementReportFileBlobStorageFixture settlementReportFileBlobStorageFixture)
    {
        _settlementReportFileBlobStorageFixture = settlementReportFileBlobStorageFixture;

        var blobContainerClient = settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        Fixture.Inject<ISettlementReportFileRepository>(new SettlementReportFileBlobStorage(blobContainerClient));
    }

    [Fact]
    public async Task FinalizeAsync_WithInputFiles_RemovesInputFiles()
    {
        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var inputFiles = new GeneratedSettlementReportFileDto[]
        {
            new(requestId, "fileA.csv"),
            new(requestId, "fileB.csv"),
            new(requestId, "fileC.csv"),
        };

        await Task.WhenAll(inputFiles.Select(MakeTestFileAsync));

        var generatedSettlementReport = new GeneratedSettlementReportDto(
            requestId,
            new GeneratedSettlementReportFileDto(requestId, "Report.zip"),
            inputFiles);

        // Act
        await Fixture
            .Create<SettlementReportFinalizeHandler>()
            .FinalizeAsync(generatedSettlementReport);

        // Assert
        var container = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();

        foreach (var inputFile in inputFiles)
        {
            var generatedFileBlob = container.GetBlobClient($"settlement-reports/{requestId.Id}/{inputFile.FileName}");
            Assert.False(await generatedFileBlob.ExistsAsync());
        }
    }

    private Task MakeTestFileAsync(GeneratedSettlementReportFileDto file)
    {
        var containerClient = _settlementReportFileBlobStorageFixture.CreateBlobContainerClient();
        var blobClient = containerClient.GetBlobClient($"settlement-reports/{file.RequestId.Id}/{file.FileName}");
        return blobClient.UploadAsync(new BinaryData($"Content: {file.FileName}"));
    }
}
