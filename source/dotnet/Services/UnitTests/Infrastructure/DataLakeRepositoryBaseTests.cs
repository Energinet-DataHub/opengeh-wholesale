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
using Energinet.DataHub.Wholesale.Application.ProcessResult;
using Energinet.DataHub.Wholesale.Application.ProcessResult.Model;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Energinet.DataHub.Wholesale.Infrastructure;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.DataLake;
using Energinet.DataHub.Wholesale.Infrastructure.Processes;
using Energinet.DataHub.Wholesale.Tests.Infrastructure.SettlementReport;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Categories;
using TimeSeriesType = Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Tests.Infrastructure;

[UnitTest]
public class DatalakeRepositoryBaseTests
{
    [Theory]
    [AutoMoqData]
    public async Task GetDataLakeFileClientAsync_WhenDirectoryDoesNotExist_ThrowsException(
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
        var sut = new DataLakeRepositoryBase(dataLakeFileSystemClientMock.Object);

        // Act and Assert
        await sut
            .Invoking(s => s.GetDataLakeFileClientAsync(string.Empty, string.Empty))
            .Should()
            .ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [AutoMoqData]
    public async Task GetDataLakeFileClientAsync_WhenFileExtensionNotFound_ThrowException(
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

        var sut = new DataLakeRepositoryBase(dataLakeFileSystemClientMock.Object);

        // Act and Assert
        await sut
            .Invoking(s => s.GetDataLakeFileClientAsync(string.Empty, string.Empty))
            .Should()
            .ThrowAsync<Exception>();
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
