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

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.BasisData;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Infrastructure.BasisData;

[UnitTest]
public class BatchFileManagerTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task GetResultFileStreamAsync_BatchId_ReadsFromCorrectPath(
        IWebFilesZipper wfz,
        ILogger<BatchFileManager> logger)
    {
        // Arrange
        var dataLakeClient = CreateDataLakeFileSystemClientMock();
        var blobContainerClient = new Mock<BlobContainerClient>();
        var target = new BatchFileManager(dataLakeClient.Object, blobContainerClient.Object, wfz, logger);
        var gridAreaCode = new GridAreaCode("123");
        var batchId = Guid.NewGuid();

        // Act
        await target.GetResultFileStreamAsync(batchId, gridAreaCode);

        // Assert
        // This expected path must match the directory used by Databricks (see calculator.py).
        var expectedPath = $"results/batch_id={batchId}/grid_area={gridAreaCode.Code}/";

        dataLakeClient.Verify(
            client => client.GetDirectoryClient(It.Is<string>(dir => dir == expectedPath)),
            Times.Once);
    }

    private static Mock<DataLakeFileSystemClient> CreateDataLakeFileSystemClientMock()
    {
        var dataLakeClient = new Mock<DataLakeFileSystemClient>();
        var directoryClient = new Mock<DataLakeDirectoryClient>();
        var fileClient = new Mock<DataLakeFileClient>();
        var asyncPageable = new Mock<AsyncPageable<PathItem>>();

        dataLakeClient
            .Setup(client => client.GetDirectoryClient(It.IsAny<string>()))
            .Returns(directoryClient.Object);

        dataLakeClient
            .Setup(client => client.GetFileClient(It.IsAny<string>()))
            .Returns(fileClient.Object);

        directoryClient
            .Setup(dirClient => dirClient.GetPathsAsync(false, false, default))
            .Returns(asyncPageable.Object);

        fileClient
            .Setup(filClient => filClient.OpenReadAsync(0, null, null, default))
            .ReturnsAsync(new MemoryStream());

        asyncPageable
            .Setup(page => page.GetAsyncEnumerator(default))
            .Returns(CreateMockedCollectionAsync().GetAsyncEnumerator());

        return dataLakeClient;
    }

    private static async IAsyncEnumerable<PathItem> CreateMockedCollectionAsync()
    {
        var pathItem = DataLakeModelFactory.PathItem(
            "fake_value.json",
            false,
            DateTimeOffset.UtcNow,
            new ETag("fake_value"),
            0,
            "fake_value",
            "fake_value",
            "fake_value");

        yield return await Task.FromResult(pathItem);
    }
}
