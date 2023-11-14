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
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.DataLake;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using FluentAssertions;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SettlementReport;

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
            SettlementReportRepository.GetMasterBasisDataFileForTotalGridAreaSpecification(new Guid(batchId), gridAreaCode);

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
                gridAreaCode);

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
                gridAreaCode);

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
            SettlementReportRepository.GetMasterBasisDataFileForForEsPerGaSpecification(new Guid(batchId), gridAreaCode, AnyActorId);

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
                gridAreaCode,
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
                gridAreaCode,
                AnyActorId);

        // Assert
        actual.Extension.Should().Be(expected.Extension);
        actual.Directory.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Theory]
    [AutoMoqData]
    public async Task GetSettlementReportAsync_WhenGivenBatch_ReturnCorrectStream(
        BatchInfo batchInfo,
        [Frozen] Mock<IStreamZipper> streamZipperMock,
        [Frozen] Mock<IDataLakeClient> dataLakeClientMock)
    {
        // Arrange
        var basisDataBuffer = Encoding.UTF8.GetBytes("test");
        var memoryStream = new MemoryStream(basisDataBuffer);

        dataLakeClientMock
            .Setup(x => x.GetReadableFileStreamAsync(It.IsAny<string>()))
            .ReturnsAsync(memoryStream);

        var sut = new SettlementReportRepository(
            dataLakeClientMock.Object,
            streamZipperMock.Object);

        // Act
        var report = await sut.GetSettlementReportAsync(batchInfo);
        var actual = await new StreamReader(report.Stream).ReadLineAsync();

        // Assert
        actual.Should().Be("test");
    }

    [Theory]
    [AutoMoqData]
    public async Task GetSettlementReportAsync_WhenGivenBatchAndGridAreaCode_WritesToOutputStream(
        BatchInfo batchInfo,
        [Frozen] Mock<IStreamZipper> streamZipperMock,
        [Frozen] Mock<IDataLakeClient> dataLakeClientMock)
    {
        // Arrange
        const string expectedOutput = "test";
        const string filepath = "E31F50D8-8309-4CC4-A8DF-074D933329AD";
        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedOutput));
        var gridAreaCode = "001";

        dataLakeClientMock.Setup(x => x.FindFileAsync(It.IsAny<string>(), ".csv"))
            .ReturnsAsync(filepath);
        dataLakeClientMock.Setup(x => x.GetReadableFileStreamAsync(filepath))
            .ReturnsAsync(fileStream);

        using var outputStream = new MemoryStream();

        streamZipperMock.Setup(x =>
                // ReSharper disable AccessToDisposedClosure
                x.ZipAsync(
                    It.Is<IEnumerable<(Stream Stream, string Name)>>(files =>
                        files.All(file => file.Stream == fileStream && file.Name.StartsWith(gridAreaCode))),
                    outputStream))
            .Callback(() =>
            {
                fileStream.CopyTo(outputStream);
                outputStream.Position = 0;
                // ReSharper restore AccessToDisposedClosure
            });

        var sut = new SettlementReportRepository(
            dataLakeClientMock.Object,
            streamZipperMock.Object);

        // Act
        await sut.GetSettlementReportAsync(batchInfo, gridAreaCode, outputStream);

        using var streamReader = new StreamReader(outputStream);
        var actual = await streamReader.ReadLineAsync();

        // Assert
        actual.Should().Be(expectedOutput);
    }
}
