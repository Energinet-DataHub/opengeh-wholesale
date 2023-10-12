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

using AutoFixture.Xunit2;
using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.DataLake;
using FluentAssertions;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.Integration.CalculationOutput;

public class DataLakeClientTests
{
    [Theory]
    [AutoMoqData]
    public async Task FindAndOpenFileAsync_WhenDirectoryDoesNotExist_ThrowsException(
        [Frozen] Mock<DataLakeFileSystemClient> dataLakeFileSystemClientMock,
        [Frozen] Mock<DataLakeDirectoryClient> dataLakeDirectoryClientMock,
        [Frozen] Mock<Response<bool>> responseMock)
    {
        // Arrange
        dataLakeFileSystemClientMock.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
            .Returns(dataLakeDirectoryClientMock.Object);
        dataLakeDirectoryClientMock.Setup(dirClient => dirClient.ExistsAsync(default))
            .ReturnsAsync(responseMock.Object);
        responseMock.Setup(res => res.Value).Returns(false);

        var sut = new DataLakeClient(dataLakeFileSystemClientMock.Object);

        // Act and Assert
        await sut
            .Invoking(s => s.FindFileAsync(string.Empty, string.Empty))
            .Should()
            .ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [AutoMoqData]
    public async Task FindAndOpenFileAsync_WhenFileExtensionNotFound_ThrowException(
        [Frozen] Mock<DataLakeFileSystemClient> dataLakeFileSystemClientMock,
        [Frozen] Mock<DataLakeDirectoryClient> dataLakeDirectoryClientMock,
        [Frozen] Mock<Response<bool>> responseMock)
    {
        // Arrange
        const string pathWithUnknownExtension = "my_file.xxx";

        var asyncPageableWithOnePathItem = CreateAsyncPageableWithOnePathItem(pathWithUnknownExtension);

        dataLakeFileSystemClientMock.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
            .Returns(dataLakeDirectoryClientMock.Object);
        dataLakeDirectoryClientMock.Setup(dirClient => dirClient.ExistsAsync(default))
            .ReturnsAsync(responseMock.Object);
        responseMock.Setup(res => res.Value).Returns(true);
        dataLakeDirectoryClientMock
            .Setup(client => client.GetPathsAsync(false, false, It.IsAny<CancellationToken>()))
            .Returns(asyncPageableWithOnePathItem);

        var sut = new DataLakeClient(dataLakeFileSystemClientMock.Object);

        // Act and Assert
        await sut
            .Invoking(s => s.FindFileAsync(string.Empty, string.Empty))
            .Should()
            .ThrowAsync<Exception>();
    }

    [Theory]
    [AutoMoqData]
    public async Task FindAndOpenFileAsync_WhenFileExtensionFound_IsNotNull(
        [Frozen] Mock<DataLakeFileSystemClient> dataLakeFileSystemClientMock,
        [Frozen] Mock<DataLakeDirectoryClient> dataLakeDirectoryClientMock,
        [Frozen] Mock<DataLakeFileClient> dataLakeFileClientMock,
        [Frozen] Mock<Response<bool>> responseMock)
    {
        // Arrange
        const string fileName = "my_file";
        const string fileExtension = ".json";
        const string pathWithUnknownExtension = $"{fileName}{fileExtension}";

        var asyncPageableWithOnePathItem = CreateAsyncPageableWithOnePathItem(pathWithUnknownExtension);

        dataLakeFileSystemClientMock.Setup(x => x.GetDirectoryClient(fileName))
            .Returns(dataLakeDirectoryClientMock.Object);
        dataLakeFileSystemClientMock.Setup(x => x.GetFileClient(pathWithUnknownExtension))
            .Returns(dataLakeFileClientMock.Object);
        dataLakeDirectoryClientMock.Setup(dirClient => dirClient.ExistsAsync(default))
            .ReturnsAsync(responseMock.Object);
        responseMock.Setup(res => res.Value).Returns(true);
        dataLakeDirectoryClientMock
            .Setup(client => client.GetPathsAsync(false, false, It.IsAny<CancellationToken>()))
            .Returns(asyncPageableWithOnePathItem);

        var stream = new Mock<Stream>();
        dataLakeFileClientMock
            .Setup(x => x.OpenReadAsync(It.IsAny<bool>(), It.IsAny<long>(), It.IsAny<int?>(), default))
            .ReturnsAsync(stream.Object);

        var sut = new DataLakeClient(dataLakeFileSystemClientMock.Object);

        // Act
        var dataLakeFileClient = await sut.FindFileAsync(fileName, fileExtension);

        // Assert
        dataLakeFileClient.Should().NotBeNull();
    }

    [Theory]
    [AutoMoqData]
    public async Task GetWriteableFileStreamAsync_ReturnsStream(
        Mock<DataLakeFileClient> dataLakeFileClientMock,
        Mock<DataLakeFileSystemClient> dataLakeFileSystemClientMock)
    {
        // arrange
        const string filename = "A0FEE5F8-1938-4363-9AAF-3B4DBBCCBD71.cvs";

        using var stream = new MemoryStream();

        dataLakeFileSystemClientMock
            .Setup(x => x.GetFileClient(filename))
            .Returns(dataLakeFileClientMock.Object);

        dataLakeFileClientMock
            .Setup(x => x.OpenWriteAsync(false, null, default))
            .ReturnsAsync(stream);

        var sut = new DataLakeClient(dataLakeFileSystemClientMock.Object);

        // act
        var actual = await sut.GetWriteableFileStreamAsync(filename);

        // assert
        actual.Should().BeSameAs(stream);
    }

    [Theory]
    [AutoMoqData]
    public async Task GetReadableFileStreamAsync_ReturnsStream(
        Mock<DataLakeFileClient> dataLakeFileClientMock,
        Mock<DataLakeFileSystemClient> dataLakeFileSystemClientMock)
    {
        // arrange
        const string filename = "C5B4B4FA-8808-40F5-A635-39A281E6866A.csv";

        using var stream = new MemoryStream();

        dataLakeFileSystemClientMock
            .Setup(x => x.GetFileClient(filename))
            .Returns(dataLakeFileClientMock.Object);

        dataLakeFileClientMock
            .Setup(x => x.OpenReadAsync(false, default, default, default))
            .ReturnsAsync(stream);

        var sut = new DataLakeClient(dataLakeFileSystemClientMock.Object);

        // act
        var actual = await sut.GetReadableFileStreamAsync(filename);

        // assert
        actual.Should().BeSameAs(stream);
    }

    private static AsyncPageable<PathItem> CreateAsyncPageableWithOnePathItem(string path)
    {
        var pathItem = DataLakeModelFactory
            .PathItem(path, false, DateTimeOffset.Now, ETag.All, 1, "owner", "group", "permissions");
        var page = Page<PathItem>.FromValues(new[] { pathItem }, null, Mock.Of<Response>());
        var asyncPageable = AsyncPageable<PathItem>.FromPages(new[] { page });
        return asyncPageable;
    }
}
