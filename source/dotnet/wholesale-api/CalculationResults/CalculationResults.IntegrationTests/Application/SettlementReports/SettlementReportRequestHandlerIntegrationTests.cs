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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Application.SettlementReports;

public sealed class SettlementReportRequestHandlerIntegrationTests : TestBase<SettlementReportRequestHandler>
{
    public SettlementReportRequestHandlerIntegrationTests()
    {
        var mockedRepository = new Mock<ILatestCalculationVersionRepository>();
        mockedRepository
            .Setup(repository => repository.GetLatestCalculationVersionAsync())
            .ReturnsAsync(1);

        var mockedGenerator = new Mock<ISettlementReportFileGenerator>();
        mockedGenerator
            .Setup(generator => generator.CountChunksAsync(It.IsAny<SettlementReportRequestFilterDto>(), 1))
            .ReturnsAsync(1);

        mockedGenerator
            .Setup(generator => generator.CountChunksAsync(It.IsAny<SettlementReportRequestFilterDto>(), long.MaxValue))
            .ReturnsAsync(1);

        var mockedFactory = new Mock<ISettlementReportFileGeneratorFactory>();
        mockedFactory
            .Setup(factory => factory.Create(It.IsAny<SettlementReportFileContent>()))
            .Returns(mockedGenerator.Object);

        Fixture.Inject(mockedFactory.Object);
        Fixture.Inject(mockedRepository.Object);
    }

    [Fact]
    public async Task RequestReportAsync_ForBalanceFixingChunked_ReturnsPartialFiles()
    {
        // Arrange
        var calculationFilter = new Dictionary<string, CalculationId?>
        {
            { "805", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
        };

        var filter = new SettlementReportRequestFilterDto(
            calculationFilter,
            DateTimeOffset.UtcNow.Date,
            DateTimeOffset.UtcNow.Date.AddDays(2),
            CalculationType.BalanceFixing,
            null,
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var reportRequest = new SettlementReportRequestDto(false, false, filter);

        var mockedRepository = new Mock<ILatestCalculationVersionRepository>();
        mockedRepository
            .Setup(repository => repository.GetLatestCalculationVersionAsync())
            .ReturnsAsync(1);

        var mockedGenerator = new Mock<ISettlementReportFileGenerator>();
        mockedGenerator
            .Setup(generator => generator.CountChunksAsync(It.IsAny<SettlementReportRequestFilterDto>(), 1))
            .ReturnsAsync(2);

        var mockedFactory = new Mock<ISettlementReportFileGeneratorFactory>();
        mockedFactory
            .Setup(factory => factory.Create(It.IsAny<SettlementReportFileContent>()))
            .Returns(mockedGenerator.Object);

        var sut = new SettlementReportRequestHandler(mockedFactory.Object, mockedRepository.Object);

        // Act
        var actual = (await sut.RequestReportAsync(requestId, reportRequest)).ToList();

        // Assert
        var chunkA = actual[0];
        Assert.Equal(requestId, chunkA.RequestId);
        Assert.Equal(calculationFilter.Single(), chunkA.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Energy", chunkA.PartialFileInfo.FileName);
        Assert.Equal(0, chunkA.PartialFileInfo.FileOffset);
        Assert.Equal(0, chunkA.PartialFileInfo.ChunkOffset);
        Assert.Equal(SettlementReportFileContent.EnergyResult, chunkA.FileContent);

        var chunkB = actual[1];
        Assert.Equal(requestId, chunkB.RequestId);
        Assert.Equal(calculationFilter.Single(), chunkB.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Energy", chunkB.PartialFileInfo.FileName);
        Assert.Equal(0, chunkB.PartialFileInfo.FileOffset);
        Assert.Equal(1, chunkB.PartialFileInfo.ChunkOffset);
        Assert.Equal(SettlementReportFileContent.EnergyResult, chunkB.FileContent);
    }

    [Fact]
    public async Task RequestReportAsync_ForBalanceFixingWithoutBasisData_ReturnsExpectedFiles()
    {
        // Arrange
        var calculationFilter = new Dictionary<string, CalculationId?>
        {
            { "805", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
        };

        var filter = new SettlementReportRequestFilterDto(
            calculationFilter,
            DateTimeOffset.UtcNow.Date,
            DateTimeOffset.UtcNow.Date.AddDays(2),
            CalculationType.BalanceFixing,
            null,
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var reportRequest = new SettlementReportRequestDto(false, false, filter);

        // Act
        var actual = (await Sut.RequestReportAsync(requestId, reportRequest)).ToList();

        // Assert
        var energyResult = actual.Single();
        Assert.Equal(requestId, energyResult.RequestId);
        Assert.Equal(filter.PeriodStart, energyResult.RequestFilter.PeriodStart);
        Assert.Equal(filter.PeriodEnd, energyResult.RequestFilter.PeriodEnd);
        Assert.Equal(filter.CsvFormatLocale, energyResult.RequestFilter.CsvFormatLocale);
        Assert.Equal(filter.EnergySupplier, energyResult.RequestFilter.EnergySupplier);
        Assert.Equal(filter.GridAreas.Single(), energyResult.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Energy", energyResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResult, energyResult.FileContent);
    }

    [Fact]
    public async Task RequestReportAsync_ForBalanceFixingWithoutBasisDataWithSplitResult_ReturnsSplitFiles()
    {
        // Arrange
        var calculationFilter = new Dictionary<string, CalculationId?>
        {
            { "805", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
            { "806", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
        };

        var filter = new SettlementReportRequestFilterDto(
            calculationFilter,
            DateTimeOffset.UtcNow.Date,
            DateTimeOffset.UtcNow.Date.AddDays(2),
            CalculationType.BalanceFixing,
            null,
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var reportRequest = new SettlementReportRequestDto(true, false, filter);

        // Act
        var actual = (await Sut.RequestReportAsync(requestId, reportRequest)).ToList();

        // Assert
        var energyResultA = actual[0];
        Assert.Equal(requestId, energyResultA.RequestId);
        Assert.Equal(calculationFilter.First(), energyResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Energy (805)", energyResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResult, energyResultA.FileContent);

        var energyResultB = actual[1];
        Assert.Equal(requestId, energyResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), energyResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Energy (806)", energyResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResult, energyResultB.FileContent);
    }

    [Fact]
    public async Task RequestReportAsync_ForWholesaleFixingWithoutBasisData_ReturnsExpectedFiles()
    {
        // Arrange
        var calculationFilter = new Dictionary<string, CalculationId?>
        {
            { "805", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
        };

        var filter = new SettlementReportRequestFilterDto(
            calculationFilter,
            DateTimeOffset.UtcNow.Date,
            DateTimeOffset.UtcNow.Date.AddDays(2),
            CalculationType.WholesaleFixing,
            null,
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var reportRequest = new SettlementReportRequestDto(false, false, filter);

        // Act
        var actual = (await Sut.RequestReportAsync(requestId, reportRequest)).ToList();

        // Assert
        var energyResult = actual[0];
        Assert.Equal(requestId, energyResult.RequestId);
        Assert.Equal(calculationFilter.Single(), energyResult.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Energy", energyResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResult, energyResult.FileContent);

        var wholesaleResult = actual[1];
        Assert.Equal(requestId, wholesaleResult.RequestId);
        Assert.Equal(calculationFilter.Single(), wholesaleResult.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Wholesale", wholesaleResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.WholesaleResult, wholesaleResult.FileContent);
    }

    [Fact]
    public async Task RequestReportAsync_ForWholesaleFixingWithoutBasisDataWithSplitResult_ReturnsExpectedFiles()
    {
        // Arrange
        var calculationFilter = new Dictionary<string, CalculationId?>
        {
            { "805", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
            { "806", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
        };

        var filter = new SettlementReportRequestFilterDto(
            calculationFilter,
            DateTimeOffset.UtcNow.Date,
            DateTimeOffset.UtcNow.Date.AddDays(2),
            CalculationType.WholesaleFixing,
            null,
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var reportRequest = new SettlementReportRequestDto(true, false, filter);

        // Act
        var actual = (await Sut.RequestReportAsync(requestId, reportRequest)).ToList();

        // Assert
        var energyResultA = actual[0];
        Assert.Equal(requestId, energyResultA.RequestId);
        Assert.Equal(calculationFilter.First(), energyResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Energy (805)", energyResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResult, energyResultA.FileContent);

        var energyResultB = actual[1];
        Assert.Equal(requestId, energyResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), energyResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Energy (806)", energyResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResult, energyResultB.FileContent);

        var wholesaleResultA = actual[2];
        Assert.Equal(requestId, wholesaleResultA.RequestId);
        Assert.Equal(calculationFilter.First(), wholesaleResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Wholesale (805)", wholesaleResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.WholesaleResult, wholesaleResultA.FileContent);

        var wholesaleResultB = actual[3];
        Assert.Equal(requestId, wholesaleResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), wholesaleResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Wholesale (806)", wholesaleResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.WholesaleResult, wholesaleResultB.FileContent);
    }

    [Fact]
    public async Task RequestReportAsync_ForWholesaleFixingWithBasisData_ReturnsExpectedFiles()
    {
        // Arrange
        var calculationFilter = new Dictionary<string, CalculationId?>
        {
            { "805", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
        };

        var filter = new SettlementReportRequestFilterDto(
            calculationFilter,
            DateTimeOffset.UtcNow.Date,
            DateTimeOffset.UtcNow.Date.AddDays(2),
            CalculationType.WholesaleFixing,
            null,
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var reportRequest = new SettlementReportRequestDto(false, true, filter);

        // Act
        var actual = (await Sut.RequestReportAsync(requestId, reportRequest)).ToList();

        // Assert
        var energyResult = actual[0];
        Assert.Equal(requestId, energyResult.RequestId);
        Assert.Equal(calculationFilter.Single(), energyResult.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Energy", energyResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResult, energyResult.FileContent);

        var wholesaleResult = actual[1];
        Assert.Equal(requestId, wholesaleResult.RequestId);
        Assert.Equal(calculationFilter.Single(), wholesaleResult.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Wholesale", wholesaleResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.WholesaleResult, wholesaleResult.FileContent);

        var chargeLinkPeriodsResult = actual[2];
        Assert.Equal(requestId, chargeLinkPeriodsResult.RequestId);
        Assert.Equal(calculationFilter.Single(), chargeLinkPeriodsResult.RequestFilter.GridAreas.Single());
        Assert.Equal("Charge links on metering points (805)", chargeLinkPeriodsResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.ChargeLinksPeriods, chargeLinkPeriodsResult.FileContent);

        var meteringPointMasterDataResult = actual[3];
        Assert.Equal(requestId, meteringPointMasterDataResult.RequestId);
        Assert.Equal(calculationFilter.Single(), meteringPointMasterDataResult.RequestFilter.GridAreas.Single());
        Assert.Equal("Master data for metering points (805)", meteringPointMasterDataResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.MeteringPointMasterData, meteringPointMasterDataResult.FileContent);

        var timeSeriesPT15MResults = actual[4];
        Assert.Equal(requestId, timeSeriesPT15MResults.RequestId);
        Assert.Equal(calculationFilter.Single(), timeSeriesPT15MResults.RequestFilter.GridAreas.Single());
        Assert.Equal("Time series PT15M (805)", timeSeriesPT15MResults.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.Pt15M, timeSeriesPT15MResults.FileContent);

        var timeSeriesResultsB = actual[5];
        Assert.Equal(requestId, timeSeriesResultsB.RequestId);
        Assert.Equal(calculationFilter.Single(), timeSeriesResultsB.RequestFilter.GridAreas.Single());
        Assert.Equal("Time series PT1H (805)", timeSeriesResultsB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.Pt1H, timeSeriesResultsB.FileContent);

        var chargePricesResult = actual[6];
        Assert.Equal(requestId, chargePricesResult.RequestId);
        Assert.Equal(calculationFilter.Single(), chargePricesResult.RequestFilter.GridAreas.Single());
        Assert.Equal("Charge Price (805)", chargePricesResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.ChargePrice, chargePricesResult.FileContent);
    }

    [Fact]
    public async Task RequestReportAsync_ForWholesaleFixingWithBasisDataWithSplitResult_ReturnsExpectedFiles()
    {
        // Arrange
        var calculationFilter = new Dictionary<string, CalculationId?>
        {
            { "805", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
            { "806", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
        };

        var filter = new SettlementReportRequestFilterDto(
            calculationFilter,
            DateTimeOffset.UtcNow.Date,
            DateTimeOffset.UtcNow.Date.AddDays(2),
            CalculationType.WholesaleFixing,
            null,
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var reportRequest = new SettlementReportRequestDto(true, true, filter);

        // Act
        var actual = (await Sut.RequestReportAsync(requestId, reportRequest)).ToList();

        // Assert
        var energyResultA = actual[0];
        Assert.Equal(requestId, energyResultA.RequestId);
        Assert.Equal(calculationFilter.First(), energyResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Energy (805)", energyResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResult, energyResultA.FileContent);

        var energyResultB = actual[1];
        Assert.Equal(requestId, energyResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), energyResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Energy (806)", energyResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResult, energyResultB.FileContent);

        var wholesaleResultA = actual[2];
        Assert.Equal(requestId, wholesaleResultA.RequestId);
        Assert.Equal(calculationFilter.First(), wholesaleResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Wholesale (805)", wholesaleResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.WholesaleResult, wholesaleResultA.FileContent);

        var wholesaleResultB = actual[3];
        Assert.Equal(requestId, wholesaleResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), wholesaleResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Wholesale (806)", wholesaleResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.WholesaleResult, wholesaleResultB.FileContent);

        var chargeLinkPeriodsResultA = actual[4];
        Assert.Equal(requestId, chargeLinkPeriodsResultA.RequestId);
        Assert.Equal(calculationFilter.First(), chargeLinkPeriodsResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("Charge links on metering points (805)", chargeLinkPeriodsResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.ChargeLinksPeriods, chargeLinkPeriodsResultA.FileContent);

        var chargeLinkPeriodsResultB = actual[5];
        Assert.Equal(requestId, chargeLinkPeriodsResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), chargeLinkPeriodsResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("Charge links on metering points (806)", chargeLinkPeriodsResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.ChargeLinksPeriods, chargeLinkPeriodsResultB.FileContent);

        var meteringPointMasterDataResultA = actual[6];
        Assert.Equal(requestId, meteringPointMasterDataResultA.RequestId);
        Assert.Equal(calculationFilter.First(), meteringPointMasterDataResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("Master data for metering points (805)", meteringPointMasterDataResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.MeteringPointMasterData, meteringPointMasterDataResultA.FileContent);

        var meteringPointMasterDataResultB = actual[7];
        Assert.Equal(requestId, meteringPointMasterDataResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), meteringPointMasterDataResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("Master data for metering points (806)", meteringPointMasterDataResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.MeteringPointMasterData, meteringPointMasterDataResultB.FileContent);

        var pt15MResultsA = actual[8];
        Assert.Equal(requestId, pt15MResultsA.RequestId);
        Assert.Equal(calculationFilter.First(), pt15MResultsA.RequestFilter.GridAreas.Single());
        Assert.Equal("Time series PT15M (805)", pt15MResultsA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.Pt15M, pt15MResultsA.FileContent);

        var pt15MResultsB = actual[9];
        Assert.Equal(requestId, pt15MResultsB.RequestId);
        Assert.Equal(calculationFilter.Last(), pt15MResultsB.RequestFilter.GridAreas.Single());
        Assert.Equal("Time series PT15M (806)", pt15MResultsB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.Pt15M, pt15MResultsB.FileContent);

        var pt1HResultsA = actual[10];
        Assert.Equal(requestId, pt1HResultsA.RequestId);
        Assert.Equal(calculationFilter.First(), pt1HResultsA.RequestFilter.GridAreas.Single());
        Assert.Equal("Time series PT1H (805)", pt1HResultsA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.Pt1H, pt1HResultsA.FileContent);

        var pt1HResultsB = actual[11];
        Assert.Equal(requestId, pt1HResultsB.RequestId);
        Assert.Equal(calculationFilter.Last(), pt1HResultsB.RequestFilter.GridAreas.Single());
        Assert.Equal("Time series PT1H (806)", pt1HResultsB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.Pt1H, pt1HResultsB.FileContent);

        var chargePricesResultA = actual[12];
        Assert.Equal(requestId, chargePricesResultA.RequestId);
        Assert.Equal(calculationFilter.First(), chargePricesResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("Charge Price (805)", chargePricesResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.ChargePrice, chargePricesResultA.FileContent);

        var chargePricesResultB = actual[13];
        Assert.Equal(requestId, chargePricesResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), chargePricesResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("Charge Price (806)", chargePricesResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.ChargePrice, chargePricesResultB.FileContent);
    }
}
