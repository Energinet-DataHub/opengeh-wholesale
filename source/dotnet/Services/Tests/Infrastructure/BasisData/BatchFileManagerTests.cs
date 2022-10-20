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
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.BasisData;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Infrastructure.BasisData;

[UnitTest]
public class BatchFileManagerTests
{
    [Fact]
    public async Task GetResultFileStreamAsync_WhenDirectoryDoesNotExist_ThrowsException()
    {
        // Arrange
        var webFileZipperMock = new Mock<IWebFilesZipper>();
        var response = new Mock<Response<bool>>();
        var dataLakeFileSystemClientMock = new Mock<DataLakeFileSystemClient>();
        var dataLakeDirectoryClientMock = new Mock<DataLakeDirectoryClient>();

        response.Setup(res => res.Value).Returns(false);

        dataLakeFileSystemClientMock.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
            .Returns(dataLakeDirectoryClientMock.Object);

        dataLakeDirectoryClientMock.Setup(dirClient => dirClient.ExistsAsync(default))
            .ReturnsAsync(response.Object);

        var sut = new BatchFileManager(dataLakeFileSystemClientMock.Object, webFileZipperMock.Object);

        // Act and Assert
        await sut
            .Invoking(s => s.GetResultFileStreamAsync(Guid.NewGuid(), new GridAreaCode("123")))
            .Should()
            .ThrowAsync<Exception>();
    }

    [Fact]
    public async Task GetResultFileStreamAsync_WhenNoFilesMatchExtension_ThrowsException()
    {
        // Arrange
        var webFileZipperMock = new Mock<IWebFilesZipper>();
        var response = new Mock<Response<bool>>();
        var dataLakeFileSystemClientMock = new Mock<DataLakeFileSystemClient>();
        var dataLakeDirectoryClientMock = new Mock<DataLakeDirectoryClient>();

        response.Setup(res => res.Value).Returns(true);

        dataLakeFileSystemClientMock.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
            .Returns(dataLakeDirectoryClientMock.Object);

        dataLakeDirectoryClientMock.Setup(dirClient => dirClient.ExistsAsync(default))
            .ReturnsAsync(response.Object);

        var sut = new BatchFileManager(dataLakeFileSystemClientMock.Object, webFileZipperMock.Object);

        // Act and Assert
        await sut
            .Invoking(s => s.GetResultFileStreamAsync(Guid.NewGuid(), new GridAreaCode("123")))
            .Should()
            .ThrowAsync<Exception>();
    }

    [Fact]
    public async Task GetResultFileStreamAsync_WhenNoFilesMatchExtension_ThrowsException()
    {
        var pathItemMock = new Mock<PathItem>();
        // dataLakeDirectoryClientMock.Setup(dirClient => dirClient.GetPathsAsync()).Returns(pathItemMock.Object);
        var paths = new Mock<AsyncPageable>();

        // Arrange
        var webFileZipperMock = new Mock<IWebFilesZipper>();
        var response = new Mock<Response<bool>>();
        var dataLakeFileSystemClientMock = new Mock<DataLakeFileSystemClient>();
        var dataLakeDirectoryClientMock = new Mock<DataLakeDirectoryClient>();

        response.Setup(res => res.Value).Returns(true);

        dataLakeFileSystemClientMock.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
            .Returns(dataLakeDirectoryClientMock.Object);

        dataLakeDirectoryClientMock.Setup(dirClient => dirClient.ExistsAsync(default))
            .ReturnsAsync(response.Object);

        var sut = new BatchFileManager(dataLakeFileSystemClientMock.Object, webFileZipperMock.Object);

        // Act and Assert
        await sut
            .Invoking(s => s.GetResultFileStreamAsync(Guid.NewGuid(), new GridAreaCode("123")))
            .Should()
            .ThrowAsync<Exception>();
    }
}
