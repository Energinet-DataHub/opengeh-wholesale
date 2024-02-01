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
using System.IO.Compression;
using System.Text;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SettlementReport;

public class SettlementReportApplicationServiceTests
{
    [Theory]
    [AutoMoqData]
    public static async Task CreateCompressedSettlementReportAsync_GivenRows_CreatesValidZipArchive(
        [Frozen] Mock<ICalculationsClient> batchesClientMock,
        [Frozen] Mock<ISettlementReportResultsCsvWriter> settlementReportResultsCsvWriterMock,
        [Frozen] Mock<ISettlementReportRepository> settlementReportRepositoryMock,
        [Frozen] Mock<ISettlementReportResultQueries> settlementReportResultRepositoryMock)
    {
        // Arrange
        await using var memoryStream = new MemoryStream();
        var sut = new SettlementReportClient(
            batchesClientMock.Object,
            settlementReportResultsCsvWriterMock.Object,
            settlementReportRepositoryMock.Object,
            settlementReportResultRepositoryMock.Object);

        const string fileContent = "Unit Test File Contents";

        settlementReportResultsCsvWriterMock
            .Setup(x => x.WriteAsync(It.IsAny<Stream>(), It.IsAny<IEnumerable<SettlementReportResultRow>>(), It.IsAny<CultureInfo>()))
            .Returns<Stream, IEnumerable<SettlementReportResultRow>, CultureInfo>((stream, _, _) =>
            {
                using var textWriter = new StreamWriter(stream, Encoding.UTF8);
                return textWriter.WriteAsync(fileContent);
            });

        // Act
        await sut.CreateCompressedSettlementReportAsync(
            () => memoryStream,
            new[] { "500" },
            CalculationType.BalanceFixing,
            DateTimeOffset.MinValue,
            DateTimeOffset.MaxValue,
            null,
            null);

        // Assert
        using var archive = new ZipArchive(new MemoryStream(memoryStream.ToArray()), ZipArchiveMode.Read);
        Assert.Single(archive.Entries);
        Assert.Equal("Result.csv", archive.Entries[0].Name);

        using var streamReader = new StreamReader(archive.Entries[0].Open());
        var contents = await streamReader.ReadToEndAsync();
        Assert.Equal(fileContent, contents);
    }

    [Theory]
    [AutoMoqData]
    public static async Task CreateCompressedSettlementReportAsync_GivenNoLanguage_DefaultsToEnUs(
        [Frozen] Mock<ICalculationsClient> batchesClientMock,
        [Frozen] Mock<ISettlementReportResultsCsvWriter> settlementReportResultsCsvWriterMock,
        [Frozen] Mock<ISettlementReportRepository> settlementReportRepositoryMock,
        [Frozen] Mock<ISettlementReportResultQueries> settlementReportResultRepositoryMock)
    {
        // Arrange
        await using var memoryStream = new MemoryStream();
        var sut = new SettlementReportClient(
            batchesClientMock.Object,
            settlementReportResultsCsvWriterMock.Object,
            settlementReportRepositoryMock.Object,
            settlementReportResultRepositoryMock.Object);

        const string fileContent = "Unit Test File Contents";

        settlementReportResultsCsvWriterMock
            .Setup(x => x.WriteAsync(It.IsAny<Stream>(), It.IsAny<IEnumerable<SettlementReportResultRow>>(), It.IsAny<CultureInfo>()))
            .Returns<Stream, IEnumerable<SettlementReportResultRow>, CultureInfo>((stream, _, _) =>
            {
                using var textWriter = new StreamWriter(stream, Encoding.UTF8);
                return textWriter.WriteAsync(fileContent);
            });

        // Act
        await sut.CreateCompressedSettlementReportAsync(
            () => memoryStream,
            new[] { "500" },
            CalculationType.BalanceFixing,
            DateTimeOffset.MinValue,
            DateTimeOffset.MaxValue,
            null,
            null);

        // Assert
        settlementReportResultsCsvWriterMock.Verify(x => x.WriteAsync(It.IsAny<Stream>(), It.IsAny<IEnumerable<SettlementReportResultRow>>(), new CultureInfo("en-US")), Times.Once);
    }

    [Theory]
    [AutoMoqData]
    public static async Task CreateCompressedSettlementReportAsync_GivenUnsupportedCalculationType_ThrowValidationException(
        [Frozen] Mock<ICalculationsClient> batchesClientMock,
        [Frozen] Mock<ISettlementReportResultsCsvWriter> settlementReportResultsCsvWriterMock,
        [Frozen] Mock<ISettlementReportRepository> settlementReportRepositoryMock,
        [Frozen] Mock<ISettlementReportResultQueries> settlementReportResultRepositoryMock)
    {
        // Arrange
        var sut = new SettlementReportClient(
            batchesClientMock.Object,
            settlementReportResultsCsvWriterMock.Object,
            settlementReportRepositoryMock.Object,
            settlementReportResultRepositoryMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<BusinessValidationException>(() => sut.CreateCompressedSettlementReportAsync(
            () => Stream.Null,
            new[] { "500" },
            CalculationType.Aggregation,
            DateTimeOffset.MinValue,
            DateTimeOffset.MaxValue,
            null,
            null));
    }
}
