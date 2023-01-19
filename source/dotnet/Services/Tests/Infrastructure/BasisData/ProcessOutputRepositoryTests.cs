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

using System.Globalization;
using System.Text;
using AutoFixture.Xunit2;
using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Application.ProcessResult;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.BasisData;
using Energinet.DataHub.Wholesale.Infrastructure.Processes;
using Energinet.DataHub.Wholesale.Tests.Domain.BatchAggregate;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Infrastructure.BasisData;

[UnitTest]
public class ProcessOutputRepositoryTests
{
    [Theory]
    [AutoMoqData]
    public async Task GetAsync_ReturnsProcessActorResult(
        [Frozen] Mock<IStreamZipper> streamZipperMock,
        [Frozen] Mock<IProcessResultPointFactory> processResultFactoryMock,
        [Frozen] Mock<DataLakeFileSystemClient> dataLakeFileSystemClientMock,
        [Frozen] Mock<DataLakeDirectoryClient> dataLakeDirectoryClientMock,
        [Frozen] Mock<DataLakeFileClient> dataLakeFileClientMock,
        [Frozen] Mock<Response<bool>> responseMock)
    {
        // Arrange
        const string pathWithKnownExtension = "my_file.json";
        var asyncPageable = CreateAsyncPageableWithOnePathItem(pathWithKnownExtension);
        var stream = new Mock<Stream>();

        dataLakeDirectoryClientMock
            .Setup(client => client.GetPathsAsync(false, false, It.IsAny<CancellationToken>()))
            .Returns(asyncPageable);
        responseMock.Setup(res => res.Value).Returns(true);
        dataLakeDirectoryClientMock.Setup(dirClient => dirClient.ExistsAsync(default))
            .ReturnsAsync(responseMock.Object);
        dataLakeFileSystemClientMock.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
            .Returns(dataLakeDirectoryClientMock.Object);
        dataLakeFileSystemClientMock.Setup(x => x.GetFileClient(pathWithKnownExtension))
            .Returns(dataLakeFileClientMock.Object);
        dataLakeFileClientMock
            .Setup(x => x.OpenReadAsync(It.IsAny<bool>(), It.IsAny<long>(), It.IsAny<int?>(), default))
            .ReturnsAsync(stream.Object);
        var processResultPoint = new ProcessResultPoint("1.00", "A04", "2022-05-31T22:00:00");
        processResultFactoryMock.Setup(x => x.GetPointsFromJsonStreamAsync(stream.Object))
            .ReturnsAsync(new List<ProcessResultPoint>
            {
                processResultPoint,
            });

        var sut = new ProcessOutputRepository(dataLakeFileSystemClientMock.Object, streamZipperMock.Object, processResultFactoryMock.Object);

        // Act
        var actual = await sut.GetAsync(Guid.NewGuid(), new GridAreaCode("123"));

        // Assert
        var timeSeriesPoint = actual.TimeSeriesPoints.First();
        timeSeriesPoint.Quality.Should().Be(processResultPoint.quality);
        timeSeriesPoint.Quantity.Should().Be(decimal.Parse(processResultPoint.quantity, CultureInfo.InvariantCulture));
        timeSeriesPoint.Time.Should().Be(DateTimeOffset.Parse(processResultPoint.quarter_time));
    }

    [Theory]
    [AutoMoqData]
    public async Task GetResultFileStreamAsync_WhenDirectoryDoesNotExist_ThrowsException(
        [Frozen] Mock<IStreamZipper> streamZipperMock,
        [Frozen] Mock<DataLakeFileSystemClient> dataLakeFileSystemClientMock,
        [Frozen] Mock<DataLakeDirectoryClient> dataLakeDirectoryClientMock,
        [Frozen] Mock<Response<bool>> responseMock)
    {
        // Arrange
        var asyncPageable = CreateAsyncPageableWithOnePathItem("my_file.json");

        dataLakeDirectoryClientMock
            .Setup(client => client.GetPathsAsync(false, false, It.IsAny<CancellationToken>()))
            .Returns(asyncPageable);
        dataLakeFileSystemClientMock.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
            .Returns(dataLakeDirectoryClientMock.Object);
        dataLakeFileSystemClientMock.Setup(x => x.GetFileClient(It.IsAny<string>()))
            .Returns((Func<DataLakeFileClient>)null!);
        dataLakeDirectoryClientMock.Setup(dirClient => dirClient.ExistsAsync(default))
            .ReturnsAsync(responseMock.Object);
        responseMock.Setup(res => res.Value).Returns(true);

        var sut = new ProcessOutputRepository(dataLakeFileSystemClientMock.Object, streamZipperMock.Object);

        // Act and Assert
        await sut
            .Invoking(s => s.GetAsync(Guid.NewGuid(), new GridAreaCode("123")))
            .Should()
            .ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [AutoMoqData]
    public async Task GetResultFileStreamAsync_WhenNoFileClientFound_ThrowsException(
        [Frozen] Mock<IStreamZipper> streamZipperMock,
        [Frozen] Mock<DataLakeFileSystemClient> dataLakeFileSystemClientMock,
        [Frozen] Mock<DataLakeDirectoryClient> dataLakeDirectoryClientMock,
        [Frozen] Mock<Response<bool>> responseMock)
    {
        // Arrange
        responseMock.Setup(res => res.Value).Returns(true);
        dataLakeFileSystemClientMock.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
            .Returns(dataLakeDirectoryClientMock.Object);
        dataLakeDirectoryClientMock.Setup(dirClient => dirClient.ExistsAsync(default))
            .ReturnsAsync(responseMock.Object);

        var sut = new ProcessOutputRepository(dataLakeFileSystemClientMock.Object, streamZipperMock.Object);

        // Act and Assert
        await sut
            .Invoking(s => s.GetAsync(Guid.NewGuid(), new GridAreaCode("123")))
            .Should()
            .ThrowAsync<Exception>();
    }

    [Theory]
    [AutoMoqData]
    public async Task GetResultFileStreamAsync_WhenFileExtensionNotFound_ThrowException(
        [Frozen] Mock<IStreamZipper> streamZipperMock,
        [Frozen] Mock<DataLakeFileSystemClient> dataLakeFileSystemClientMock,
        [Frozen] Mock<DataLakeDirectoryClient> dataLakeDirectoryClientMock,
        [Frozen] Mock<Response<bool>> responseMock)
    {
        // Arrange
        const string pathWithUnknownExtension = "my_file.xxx";
        var pathItem = DataLakeModelFactory.PathItem(
            pathWithUnknownExtension,
            false,
            DateTimeOffset.Now,
            ETag.All,
            1,
            "owner",
            "group",
            "permissions");
        var page = Page<PathItem>.FromValues(new[] { pathItem }, null, Mock.Of<Response>());
        var asyncPageable = AsyncPageable<PathItem>.FromPages(new[] { page });

        dataLakeDirectoryClientMock
            .Setup(client => client.GetPathsAsync(false, false, It.IsAny<CancellationToken>()))
            .Returns(asyncPageable);
        responseMock.Setup(res => res.Value).Returns(true);
        dataLakeFileSystemClientMock.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
            .Returns(dataLakeDirectoryClientMock.Object);

        dataLakeDirectoryClientMock.Setup(dirClient => dirClient.ExistsAsync(default))
            .ReturnsAsync(responseMock.Object);

        var sut = new ProcessOutputRepository(dataLakeFileSystemClientMock.Object, streamZipperMock.Object);

        // Act and Assert
        await sut
            .Invoking(s => s.GetAsync(Guid.NewGuid(), new GridAreaCode("123")))
            .Should()
            .ThrowAsync<Exception>();
    }

    [Fact]
    public static async Task GetResultFileSpecification_MatchesContract()
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.ResultFile;

        // Act
        var actual = ProcessOutputRepository.GetResultFileSpecification(new Guid(batchId), new GridAreaCode(gridAreaCode));

        // Assert
        actual.Extension.Should().Be(expected.Extension);
        actual.Directory.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Fact]
    public static async Task GetMasterBasisDataFileSpecification_MatchesContract()
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.MasterBasisDataFile;

        // Act
        var actual =
            ProcessOutputRepository.GetMasterBasisDataFileSpecification(new Guid(batchId), new GridAreaCode(gridAreaCode));

        // Assert
        actual.Extension.Should().Be(expected.Extension);
        actual.Directory.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Fact]
    public static async Task GetTimeSeriesHourBasisDataFileSpecification_MatchesContract()
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.TimeSeriesHourBasisDataFile;

        // Act
        var actual =
            ProcessOutputRepository.GetTimeSeriesHourBasisDataFileSpecification(
                new Guid(batchId),
                new GridAreaCode(gridAreaCode));

        // Assert
        actual.Extension.Should().Be(expected.Extension);
        actual.Directory.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Fact]
    public static async Task GetTimeSeriesQuarterBasisDataFileSpecification_MatchesContract()
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.TimeSeriesQuarterBasisDataFile;

        // Act
        var actual =
            ProcessOutputRepository.GetTimeSeriesQuarterBasisDataFileSpecification(
                new Guid(batchId),
                new GridAreaCode(gridAreaCode));

        // Assert
        actual.Extension.Should().Be(expected.Extension);
        actual.Directory.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Theory]
    [AutoMoqData]
    public async Task GetZippedBasisDataStreamAsync_WhenGivenBatch_ReturnCorrectStream(
        [Frozen] Mock<IStreamZipper> streamZipperMock,
        [Frozen] Mock<DataLakeFileSystemClient> dataLakeFileSystemClientMock,
        [Frozen] Mock<DataLakeFileClient> dataLakeFileClientMock)
    {
        // Arrange
        dataLakeFileSystemClientMock.Setup(x => x.GetFileClient(It.IsAny<string>()))
            .Returns(dataLakeFileClientMock.Object);
        var basisDataBuffer = Encoding.UTF8.GetBytes("test");
        var memoryStream = new MemoryStream(basisDataBuffer);
        const string anyStringValue = "anyStringValue";
        var fileDownloadResponse = Response.FromValue(
            DataLakeModelFactory.FileDownloadInfo(
                memoryStream.Length,
                memoryStream,
                null,
                DataLakeModelFactory.FileDownloadDetails(
                    DateTimeOffset.Now,
                    new Dictionary<string,
                        string>(),
                    anyStringValue,
                    ETag.All,
                    anyStringValue,
                    anyStringValue,
                    anyStringValue,
                    anyStringValue,
                    DateTimeOffset.Now,
                    anyStringValue,
                    anyStringValue,
                    anyStringValue,
                    new Uri("https://notarealadresszxc.com"),
                    CopyStatus.Success,
                    DataLakeLeaseDuration.Fixed,
                    DataLakeLeaseState.Available,
                    DataLakeLeaseStatus.Locked,
                    anyStringValue,
                    false,
                    anyStringValue,
                    basisDataBuffer)),
            null!);
        dataLakeFileClientMock.Setup(x => x.ReadAsync()).ReturnsAsync(fileDownloadResponse);
        var sut = new ProcessOutputRepository(dataLakeFileSystemClientMock.Object, streamZipperMock.Object);
        var batch = new BatchBuilder().Build();

        // Act
        var actual = await new StreamReader(await sut.GetZippedBasisDataStreamAsync(batch).ConfigureAwait(false))
            .ReadLineAsync();

        // Assert
        actual.Should().Be("test");
    }

    [Theory]
    [AutoMoqData]
    public async Task CreateBasisDataZipAsync_CreatesZipFile_WhenDataDirectoryIsNotFound(
        [Frozen] Mock<IStreamZipper> streamZipperMock,
        [Frozen] Mock<DataLakeFileSystemClient> dataLakeFileSystemClientMock,
        [Frozen] Mock<DataLakeDirectoryClient> dataLakeDirectoryClientMock,
        [Frozen] Mock<DataLakeFileClient> dataLakeFileClientMock,
        [Frozen] Mock<Response<bool>> responseMock)
    {
        // Arrange
        var completedBatch = new BatchBuilder().Build();
        var stream = new Mock<Stream>();
        responseMock.Setup(res => res.Value).Returns(false);
        dataLakeFileSystemClientMock.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
            .Returns(dataLakeDirectoryClientMock.Object);

        dataLakeDirectoryClientMock.Setup(x => x.ExistsAsync(default)).ReturnsAsync(responseMock.Object);

        dataLakeFileSystemClientMock.Setup(x => x.GetFileClient(It.IsAny<string>()))
            .Returns(dataLakeFileClientMock.Object);

        dataLakeFileClientMock
            .Setup(x => x.OpenWriteAsync(default, null, default))
            .ReturnsAsync(stream.Object);

        var sut = new ProcessOutputRepository(dataLakeFileSystemClientMock.Object, streamZipperMock.Object);

        // Act & Assert
        await sut.Invoking(s => s.CreateBasisDataZipAsync(completedBatch)).Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetResultAsync_Time_IsRead()
    {
        // Arrange
        var expected = new DateTimeOffset(2022, 05, 15, 22, 15, 0, TimeSpan.Zero);
        var sut = ProcessResultApplicationService();

        // Act
        var actual = await sut.GetResultAsync(
            new ProcessStepResultRequestDto(
                Guid.NewGuid(),
                GridAreaCode,
                ProcessStepType.AggregateProductionPerGridArea));

        // Assert
        actual.TimeSeriesPoints[1].Time.Should().Be(expected);
    }

    [Fact]
    public async Task GetResultAsync_Quantity_IsRead()
    {
        // Arrange
        var sut = ProcessResultApplicationService();

        // Act
        var actual = await sut.GetResultAsync(
            new ProcessStepResultRequestDto(
                Guid.NewGuid(),
                GridAreaCode,
                ProcessStepType.AggregateProductionPerGridArea));

        // Assert
        actual.TimeSeriesPoints.First().Quantity.Should().Be(1.000m);
    }

    [Fact]
    public async Task GetResultAsync_Quality_IsRead()
    {
        // Arrange
        var sut = ProcessResultApplicationService();

        // Act
        var actual = await sut.GetResultAsync(
            new ProcessStepResultRequestDto(
                Guid.NewGuid(),
                GridAreaCode,
                ProcessStepType.AggregateProductionPerGridArea));

        // Assert
        actual.TimeSeriesPoints.First().Quality.Should().Be("A04");
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
