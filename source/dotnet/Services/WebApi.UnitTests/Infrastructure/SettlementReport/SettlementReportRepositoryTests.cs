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

using System.Text;
using AutoFixture.Xunit2;
using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Application.ProcessStep;
using Energinet.DataHub.Wholesale.Application.ProcessStep.Model;
using Energinet.DataHub.Wholesale.Domain.ActorAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.WebApi.UnitTests.Domain.BatchAggregate;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Categories;
using TimeSeriesType = Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Infrastructure.SettlementReport;

[UnitTest]
public class SettlementReportRepositoryTests
{
    private const string AnyActorId = "1234567890123";

    [Fact]
    public static async Task GetMasterBasisDataForTotalGridAreaFileSpecification_MatchesContract()
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.MasterBasisDataFileForTotalGridArea;

        // Act
        var actual =
            SettlementReportRepository.GetMasterBasisDataFileForTotalGridAreaSpecification(new Guid(batchId), new GridAreaCode(gridAreaCode));

        // Assert
        actual.Extension.Should().Be(expected.Extension);
        actual.Directory.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Fact]
    public static async Task GetTimeSeriesHourBasisDataForTotalGridAreaFileSpecification_MatchesContract()
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.TimeSeriesHourBasisDataFileForTotalGridArea;

        // Act
        var actual =
            SettlementReportRepository.GetTimeSeriesHourBasisDataForTotalGridAreaFileSpecification(
                new Guid(batchId),
                new GridAreaCode(gridAreaCode));

        // Assert
        actual.Extension.Should().Be(expected.Extension);
        actual.Directory.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Fact]
    public static async Task GetTimeSeriesQuarterBasisDataForTotalGridAreaFileSpecification_MatchesContract()
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.TimeSeriesQuarterBasisDataFileForTotalGridArea;

        // Act
        var actual =
            SettlementReportRepository.GetTimeSeriesQuarterBasisDataForTotalGridAreaFileSpecification(
                new Guid(batchId),
                new GridAreaCode(gridAreaCode));

        // Assert
        actual.Extension.Should().Be(expected.Extension);
        actual.Directory.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Fact]
    public static async Task GetMasterBasisDataForEsPerGaFileSpecification_MatchesContract()
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.MasterBasisDataFileForEsPerGa;

        // Act
        var actual =
            SettlementReportRepository.GetMasterBasisDataFileForForEsPerGaSpecification(new Guid(batchId), new GridAreaCode(gridAreaCode), AnyActorId);

        // Assert
        actual.Extension.Should().Be(expected.Extension);
        actual.Directory.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Fact]
    public static async Task GetTimeSeriesHourBasisDataForEsPerGaFileSpecification_MatchesContract()
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.TimeSeriesHourBasisDataFileForEsPerGa;

        // Act
        var actual =
            SettlementReportRepository.GetTimeSeriesHourBasisDataForEsPerGaGridAreaFileSpecification(
                new Guid(batchId),
                new GridAreaCode(gridAreaCode),
                AnyActorId);

        // Assert
        actual.Extension.Should().Be(expected.Extension);
        actual.Directory.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Fact]
    public static async Task GetTimeSeriesQuarterBasisDataForEsPerGaFileSpecification_MatchesContract()
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.TimeSeriesQuarterBasisDataFileForEsPerGa;

        // Act
        var actual =
            SettlementReportRepository.GetTimeSeriesQuarterBasisDataForEsPerGaFileSpecification(
                new Guid(batchId),
                new GridAreaCode(gridAreaCode),
                AnyActorId);

        // Assert
        actual.Extension.Should().Be(expected.Extension);
        actual.Directory.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Theory]
    [AutoMoqData]
    public async Task GetSettlementReportAsync_WhenGivenBatch_ReturnCorrectStream(
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
        var sut = new SettlementReportRepository(
            dataLakeFileSystemClientMock.Object,
            streamZipperMock.Object);
        var batch = new BatchBuilder().Build();

        // Act
        var report = await sut.GetSettlementReportAsync(batch).ConfigureAwait(false);
        var actual = await new StreamReader(report.Stream)
            .ReadLineAsync();

        // Assert
        actual.Should().Be("test");
    }

    [Theory]
    [AutoMoqData]
    public async Task CreateSettlementReportAsync_CreatesSettlementReportFile_WhenDataDirectoryIsNotFound(
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

        var sut = new SettlementReportRepository(
            dataLakeFileSystemClientMock.Object,
            streamZipperMock.Object);

        // Act & Assert
        await sut.Invoking(s => s.CreateSettlementReportsAsync(completedBatch)).Should().NotThrowAsync();
    }

    [Theory]
    [AutoMoqData]
    public async Task GetResultAsync_TimeSeriesPoint_IsRead(
        [Frozen] Mock<IProcessStepResultRepository> processActorResultRepositoryMock,
        [Frozen] Mock<IActorRepository> actorRepositoryMock)
    {
        // Arrange
        var time = new DateTimeOffset(2022, 05, 15, 22, 15, 0, TimeSpan.Zero);
        var quantity = 1.000m;
        var quality = TimeSeriesPointQuality.Measured;

        const string gridAreaCode = "805";
        var batchId = Guid.NewGuid();

        var sut = new ProcessStepApplicationService(
            processActorResultRepositoryMock.Object,
            new ProcessStepResultMapper(),
            actorRepositoryMock.Object);

        processActorResultRepositoryMock.Setup(p => p.GetAsync(batchId, new GridAreaCode(gridAreaCode), TimeSeriesType.Production, null, null))
            .ReturnsAsync(new ProcessStepResult(TimeSeriesType.Production, new[] { new TimeSeriesPoint(time, quantity, quality) }));

        // Act
        var actual = await sut.GetResultAsync(
                batchId,
                gridAreaCode,
                Energinet.DataHub.Wholesale.Contracts.TimeSeriesType.Production,
                null,
                null);

        // Assert
        actual.TimeSeriesPoints.First().Time.Should().Be(time);
        actual.TimeSeriesPoints.First().Quantity.Should().Be(quantity);
        actual.TimeSeriesPoints.First().Quality.Should().Be(quality.ToString());
    }
}
