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
            .Setup(generator => generator.CountChunksAsync(It.IsAny<SettlementReportRequestFilterDto>(), It.IsAny<SettlementReportRequestedByActor>(), 1))
            .ReturnsAsync(1);

        mockedGenerator
            .Setup(generator => generator.CountChunksAsync(It.IsAny<SettlementReportRequestFilterDto>(), It.IsAny<SettlementReportRequestedByActor>(), long.MaxValue))
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
        var reportRequest = new SettlementReportRequestDto(false, false, false, false, filter);
        var actorInfo = new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null);

        var mockedRepository = new Mock<ILatestCalculationVersionRepository>();
        mockedRepository
            .Setup(repository => repository.GetLatestCalculationVersionAsync())
            .ReturnsAsync(1);

        var mockedGenerator = new Mock<ISettlementReportFileGenerator>();
        mockedGenerator
            .Setup(generator => generator.CountChunksAsync(It.IsAny<SettlementReportRequestFilterDto>(), It.IsAny<SettlementReportRequestedByActor>(), 1))
            .ReturnsAsync(2);

        var mockedFactory = new Mock<ISettlementReportFileGeneratorFactory>();
        mockedFactory
            .Setup(factory => factory.Create(It.IsAny<SettlementReportFileContent>()))
            .Returns(mockedGenerator.Object);

        var sut = new SettlementReportRequestHandler(mockedFactory.Object, mockedRepository.Object);

        // Act
        var actual = (await sut.RequestReportAsync(requestId, reportRequest, actorInfo)).ToList();

        // Assert
        var chunkA = actual[0];
        Assert.Equal(requestId, chunkA.RequestId);
        Assert.Equal(calculationFilter.Single(), chunkA.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTENERGY", chunkA.PartialFileInfo.FileName);
        Assert.Equal(0, chunkA.PartialFileInfo.FileOffset);
        Assert.Equal(0, chunkA.PartialFileInfo.ChunkOffset);
        Assert.Equal(SettlementReportFileContent.EnergyResult, chunkA.FileContent);

        var chunkB = actual[1];
        Assert.Equal(requestId, chunkB.RequestId);
        Assert.Equal(calculationFilter.Single(), chunkB.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTENERGY", chunkB.PartialFileInfo.FileName);
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
        var reportRequest = new SettlementReportRequestDto(false, false, false, false, filter);
        var actorInfo = new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null);

        // Act
        var actual = (await Sut.RequestReportAsync(requestId, reportRequest, actorInfo)).ToList();

        // Assert
        var energyResult = actual.Single();
        Assert.Equal(requestId, energyResult.RequestId);
        Assert.Equal(filter.PeriodStart, energyResult.RequestFilter.PeriodStart);
        Assert.Equal(filter.PeriodEnd, energyResult.RequestFilter.PeriodEnd);
        Assert.Equal(filter.CsvFormatLocale, energyResult.RequestFilter.CsvFormatLocale);
        Assert.Equal(filter.EnergySupplier, energyResult.RequestFilter.EnergySupplier);
        Assert.Equal(filter.GridAreas.Single(), energyResult.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTENERGY", energyResult.PartialFileInfo.FileName);
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
        var reportRequest = new SettlementReportRequestDto(true, false, false, false, filter);
        var actorInfo = new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null);

        // Act
        var actual = (await Sut.RequestReportAsync(requestId, reportRequest, actorInfo)).ToList();

        // Assert
        var energyResultA = actual[0];
        Assert.Equal(requestId, energyResultA.RequestId);
        Assert.Equal(calculationFilter.First(), energyResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTENERGY_805", energyResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResult, energyResultA.FileContent);

        var energyResultB = actual[1];
        Assert.Equal(requestId, energyResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), energyResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTENERGY_806", energyResultB.PartialFileInfo.FileName);
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
        var reportRequest = new SettlementReportRequestDto(false, false, false, false, filter);
        var actorInfo = new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null);

        // Act
        var actual = (await Sut.RequestReportAsync(requestId, reportRequest, actorInfo)).ToList();

        // Assert
        var energyResult = actual[0];
        Assert.Equal(requestId, energyResult.RequestId);
        Assert.Equal(calculationFilter.Single(), energyResult.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTENERGY", energyResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResult, energyResult.FileContent);

        var wholesaleResult = actual[1];
        Assert.Equal(requestId, wholesaleResult.RequestId);
        Assert.Equal(calculationFilter.Single(), wholesaleResult.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTWHOLESALE", wholesaleResult.PartialFileInfo.FileName);
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
        var reportRequest = new SettlementReportRequestDto(true, false, false, false, filter);
        var actorInfo = new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null);

        // Act
        var actual = (await Sut.RequestReportAsync(requestId, reportRequest, actorInfo)).ToList();

        // Assert
        var energyResultA = actual[0];
        Assert.Equal(requestId, energyResultA.RequestId);
        Assert.Equal(calculationFilter.First(), energyResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTENERGY_805", energyResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResult, energyResultA.FileContent);

        var energyResultB = actual[1];
        Assert.Equal(requestId, energyResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), energyResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTENERGY_806", energyResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResult, energyResultB.FileContent);

        var wholesaleResultA = actual[2];
        Assert.Equal(requestId, wholesaleResultA.RequestId);
        Assert.Equal(calculationFilter.First(), wholesaleResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTWHOLESALE_805", wholesaleResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.WholesaleResult, wholesaleResultA.FileContent);

        var wholesaleResultB = actual[3];
        Assert.Equal(requestId, wholesaleResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), wholesaleResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTWHOLESALE_806", wholesaleResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.WholesaleResult, wholesaleResultB.FileContent);
    }

    [Fact]
    public async Task RequestReportAsync_ForWholesaleFixingWithBasisDataWithoutMonthlyAmount_ReturnsExpectedFiles()
    {
        // Arrange
        var calculationFilter = new Dictionary<string, CalculationId?>
        {
            { "805", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
        };

        var offset = TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen").GetUtcOffset(DateTime.UtcNow);
        var startDate = new DateTimeOffset(2024, 1, 1, 1, 0, 0, offset).UtcDateTime;
        var endDate = startDate.AddMonths(1);
        var filter = new SettlementReportRequestFilterDto(
            calculationFilter,
            startDate,
            endDate,
            CalculationType.WholesaleFixing,
            null,
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var reportRequest = new SettlementReportRequestDto(false, false, true, false, filter);
        var actorInfo = new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null);

        // Act
        var actual = (await Sut.RequestReportAsync(requestId, reportRequest, actorInfo)).ToList();

        // Assert
        var energyResult = actual.FirstOrDefault(x => x.FileContent == SettlementReportFileContent.EnergyResult);
        Assert.NotNull(energyResult);
        Assert.Equal(requestId, energyResult.RequestId);
        Assert.Equal(calculationFilter.Single(), energyResult.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTENERGY", energyResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResult, energyResult.FileContent);

        var wholesaleResult = actual.FirstOrDefault(x => x.FileContent == SettlementReportFileContent.WholesaleResult);
        Assert.NotNull(wholesaleResult);
        Assert.Equal(requestId, wholesaleResult.RequestId);
        Assert.Equal(calculationFilter.Single(), wholesaleResult.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTWHOLESALE", wholesaleResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.WholesaleResult, wholesaleResult.FileContent);

        var chargeLinkPeriodsResult = actual.FirstOrDefault(x => x.FileContent == SettlementReportFileContent.ChargeLinksPeriods);
        Assert.NotNull(chargeLinkPeriodsResult);
        Assert.Equal(requestId, chargeLinkPeriodsResult.RequestId);
        Assert.Equal(calculationFilter.Single(), chargeLinkPeriodsResult.RequestFilter.GridAreas.Single());
        Assert.Equal("CHARGELINK_805", chargeLinkPeriodsResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.ChargeLinksPeriods, chargeLinkPeriodsResult.FileContent);

        var meteringPointMasterDataResult = actual.FirstOrDefault(x => x.FileContent == SettlementReportFileContent.MeteringPointMasterData);
        Assert.NotNull(meteringPointMasterDataResult);
        Assert.Equal(requestId, meteringPointMasterDataResult.RequestId);
        Assert.Equal(calculationFilter.Single(), meteringPointMasterDataResult.RequestFilter.GridAreas.Single());
        Assert.Equal("MDMP_805", meteringPointMasterDataResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.MeteringPointMasterData, meteringPointMasterDataResult.FileContent);

        var timeSeriesPT15MResult = actual.FirstOrDefault(x => x.FileContent == SettlementReportFileContent.Pt15M);
        Assert.NotNull(timeSeriesPT15MResult);
        Assert.Equal(requestId, timeSeriesPT15MResult.RequestId);
        Assert.Equal(calculationFilter.Single(), timeSeriesPT15MResult.RequestFilter.GridAreas.Single());
        Assert.Equal("TSSD15_805", timeSeriesPT15MResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.Pt15M, timeSeriesPT15MResult.FileContent);

        var timeSeriesPT1HResult = actual.FirstOrDefault(x => x.FileContent == SettlementReportFileContent.Pt1H);
        Assert.NotNull(timeSeriesPT1HResult);
        Assert.Equal(requestId, timeSeriesPT1HResult.RequestId);
        Assert.Equal(calculationFilter.Single(), timeSeriesPT1HResult.RequestFilter.GridAreas.Single());
        Assert.Equal("TSSD60_805", timeSeriesPT1HResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.Pt1H, timeSeriesPT1HResult.FileContent);

        var chargePricesResult = actual.FirstOrDefault(x => x.FileContent == SettlementReportFileContent.ChargePrice);
        Assert.NotNull(chargePricesResult);
        Assert.Equal(requestId, chargePricesResult.RequestId);
        Assert.Equal(calculationFilter.Single(), chargePricesResult.RequestFilter.GridAreas.Single());
        Assert.Equal("CHARGEPRICE_805", chargePricesResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.ChargePrice, chargePricesResult.FileContent);
    }

    [Fact]
    public async Task RequestReportAsync_ForWholesaleFixingWithBasisDataWithMonthlyAmount_ReturnsExpectedFiles()
    {
        // Arrange
        var calculationFilter = new Dictionary<string, CalculationId?>
        {
            { "805", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
        };

        var offset = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time").GetUtcOffset(DateTime.UtcNow);
        var startDate = new DateTimeOffset(2024, 1, 1, 1, 0, 0, offset).UtcDateTime;
        var endDate = startDate.AddMonths(1);
        var filter = new SettlementReportRequestFilterDto(
            calculationFilter,
            startDate,
            endDate,
            CalculationType.WholesaleFixing,
            null,
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var reportRequest = new SettlementReportRequestDto(false, false, true, true, filter);
        var actorInfo = new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null);

        // Act
        var actual = (await Sut.RequestReportAsync(requestId, reportRequest, actorInfo)).ToList();

        // Assert
        var energyResult = actual.FirstOrDefault(x => x.FileContent == SettlementReportFileContent.EnergyResult);
        Assert.NotNull(energyResult);
        Assert.Equal(requestId, energyResult.RequestId);
        Assert.Equal(calculationFilter.Single(), energyResult.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTENERGY", energyResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResult, energyResult.FileContent);

        var wholesaleResult = actual.FirstOrDefault(x => x.FileContent == SettlementReportFileContent.WholesaleResult);
        Assert.NotNull(wholesaleResult);
        Assert.Equal(requestId, wholesaleResult.RequestId);
        Assert.Equal(calculationFilter.Single(), wholesaleResult.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTWHOLESALE", wholesaleResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.WholesaleResult, wholesaleResult.FileContent);

        var chargeLinkPeriodsResult = actual.FirstOrDefault(x => x.FileContent == SettlementReportFileContent.ChargeLinksPeriods);
        Assert.NotNull(chargeLinkPeriodsResult);
        Assert.Equal(requestId, chargeLinkPeriodsResult.RequestId);
        Assert.Equal(calculationFilter.Single(), chargeLinkPeriodsResult.RequestFilter.GridAreas.Single());
        Assert.Equal("CHARGELINK_805", chargeLinkPeriodsResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.ChargeLinksPeriods, chargeLinkPeriodsResult.FileContent);

        var meteringPointMasterDataResult = actual.FirstOrDefault(x => x.FileContent == SettlementReportFileContent.MeteringPointMasterData);
        Assert.NotNull(meteringPointMasterDataResult);
        Assert.Equal(requestId, meteringPointMasterDataResult.RequestId);
        Assert.Equal(calculationFilter.Single(), meteringPointMasterDataResult.RequestFilter.GridAreas.Single());
        Assert.Equal("MDMP_805", meteringPointMasterDataResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.MeteringPointMasterData, meteringPointMasterDataResult.FileContent);

        var timeSeriesPT15MResult = actual.FirstOrDefault(x => x.FileContent == SettlementReportFileContent.Pt15M);
        Assert.NotNull(timeSeriesPT15MResult);
        Assert.Equal(requestId, timeSeriesPT15MResult.RequestId);
        Assert.Equal(calculationFilter.Single(), timeSeriesPT15MResult.RequestFilter.GridAreas.Single());
        Assert.Equal("TSSD15_805", timeSeriesPT15MResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.Pt15M, timeSeriesPT15MResult.FileContent);

        var timeSeriesPT1HResult = actual.FirstOrDefault(x => x.FileContent == SettlementReportFileContent.Pt1H);
        Assert.NotNull(timeSeriesPT1HResult);
        Assert.Equal(requestId, timeSeriesPT1HResult.RequestId);
        Assert.Equal(calculationFilter.Single(), timeSeriesPT1HResult.RequestFilter.GridAreas.Single());
        Assert.Equal("TSSD60_805", timeSeriesPT1HResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.Pt1H, timeSeriesPT1HResult.FileContent);

        var chargePricesResult = actual.FirstOrDefault(x => x.FileContent == SettlementReportFileContent.ChargePrice);
        Assert.NotNull(chargePricesResult);
        Assert.Equal(requestId, chargePricesResult.RequestId);
        Assert.Equal(calculationFilter.Single(), chargePricesResult.RequestFilter.GridAreas.Single());
        Assert.Equal("CHARGEPRICE_805", chargePricesResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.ChargePrice, chargePricesResult.FileContent);

        var wholeMonthResult = actual.FirstOrDefault(x => x.FileContent == SettlementReportFileContent.MonthlyAmount);
        Assert.NotNull(wholeMonthResult);
        Assert.Equal(requestId, wholeMonthResult.RequestId);
        Assert.Equal(calculationFilter.Single(), wholeMonthResult.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTMONTHLY_805", wholeMonthResult.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.MonthlyAmount, wholeMonthResult.FileContent);

        var wholeMonthTotalResult = actual.FirstOrDefault(x => x.FileContent == SettlementReportFileContent.MonthlyAmountTotal);
        Assert.NotNull(wholeMonthTotalResult);
        Assert.Equal(requestId, wholeMonthTotalResult.RequestId);
        Assert.Equal(calculationFilter.Single(), wholeMonthTotalResult.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTMONTHLY_805", wholeMonthTotalResult.PartialFileInfo.FileName);
        Assert.Equal(int.MaxValue, wholeMonthTotalResult.PartialFileInfo.FileOffset);
        Assert.Equal(SettlementReportFileContent.MonthlyAmountTotal, wholeMonthTotalResult.FileContent);
    }

    [Fact]
    public async Task RequestReportAsync_ForWholesaleFixingWithBasisDataWithSplitResultAndWithoutWholeMonth_ReturnsExpectedFiles()
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
        var reportRequest = new SettlementReportRequestDto(true, false, true, false, filter);
        var actorInfo = new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null);

        // Act
        var actual = (await Sut.RequestReportAsync(requestId, reportRequest, actorInfo)).ToList();

        // Assert
        var energyResultA = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.EnergyResult
                && calculationFilter.First().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(energyResultA);
        Assert.Equal(requestId, energyResultA.RequestId);
        Assert.Equal(calculationFilter.First(), energyResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTENERGY_805", energyResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResult, energyResultA.FileContent);

        var energyResultB = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.EnergyResult
                && calculationFilter.Last().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(energyResultB);
        Assert.Equal(requestId, energyResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), energyResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTENERGY_806", energyResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResult, energyResultB.FileContent);

        var wholesaleResultA = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.WholesaleResult
                && calculationFilter.First().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(wholesaleResultA);
        Assert.Equal(requestId, wholesaleResultA.RequestId);
        Assert.Equal(calculationFilter.First(), wholesaleResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTWHOLESALE_805", wholesaleResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.WholesaleResult, wholesaleResultA.FileContent);

        var wholesaleResultB = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.WholesaleResult
                && calculationFilter.Last().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(wholesaleResultB);
        Assert.Equal(requestId, wholesaleResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), wholesaleResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTWHOLESALE_806", wholesaleResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.WholesaleResult, wholesaleResultB.FileContent);

        var chargeLinkPeriodsResultA = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.ChargeLinksPeriods
                && calculationFilter.First().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(chargeLinkPeriodsResultA);
        Assert.Equal(requestId, chargeLinkPeriodsResultA.RequestId);
        Assert.Equal(calculationFilter.First(), chargeLinkPeriodsResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("CHARGELINK_805", chargeLinkPeriodsResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.ChargeLinksPeriods, chargeLinkPeriodsResultA.FileContent);

        var chargeLinkPeriodsResultB = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.ChargeLinksPeriods
                && calculationFilter.Last().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(chargeLinkPeriodsResultB);
        Assert.Equal(requestId, chargeLinkPeriodsResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), chargeLinkPeriodsResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("CHARGELINK_806", chargeLinkPeriodsResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.ChargeLinksPeriods, chargeLinkPeriodsResultB.FileContent);

        var meteringPointMasterDataResultA = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.MeteringPointMasterData
                && calculationFilter.First().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(meteringPointMasterDataResultA);
        Assert.Equal(requestId, meteringPointMasterDataResultA.RequestId);
        Assert.Equal(calculationFilter.First(), meteringPointMasterDataResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("MDMP_805", meteringPointMasterDataResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.MeteringPointMasterData, meteringPointMasterDataResultA.FileContent);

        var meteringPointMasterDataResultB = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.MeteringPointMasterData
                && calculationFilter.Last().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(meteringPointMasterDataResultB);
        Assert.Equal(requestId, meteringPointMasterDataResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), meteringPointMasterDataResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("MDMP_806", meteringPointMasterDataResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.MeteringPointMasterData, meteringPointMasterDataResultB.FileContent);

        var pt15MResultsA = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.Pt15M
                && calculationFilter.First().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(pt15MResultsA);
        Assert.Equal(requestId, pt15MResultsA.RequestId);
        Assert.Equal(calculationFilter.First(), pt15MResultsA.RequestFilter.GridAreas.Single());
        Assert.Equal("TSSD15_805", pt15MResultsA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.Pt15M, pt15MResultsA.FileContent);

        var pt15MResultsB = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.Pt15M
                && calculationFilter.Last().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(pt15MResultsB);
        Assert.Equal(requestId, pt15MResultsB.RequestId);
        Assert.Equal(calculationFilter.Last(), pt15MResultsB.RequestFilter.GridAreas.Single());
        Assert.Equal("TSSD15_806", pt15MResultsB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.Pt15M, pt15MResultsB.FileContent);

        var pt1HResultsA = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.Pt1H
                && calculationFilter.First().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(pt1HResultsA);
        Assert.Equal(requestId, pt1HResultsA.RequestId);
        Assert.Equal(calculationFilter.First(), pt1HResultsA.RequestFilter.GridAreas.Single());
        Assert.Equal("TSSD60_805", pt1HResultsA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.Pt1H, pt1HResultsA.FileContent);

        var pt1HResultsB = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.Pt1H
                && calculationFilter.Last().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(pt1HResultsB);
        Assert.Equal(requestId, pt1HResultsB.RequestId);
        Assert.Equal(calculationFilter.Last(), pt1HResultsB.RequestFilter.GridAreas.Single());
        Assert.Equal("TSSD60_806", pt1HResultsB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.Pt1H, pt1HResultsB.FileContent);

        var chargePricesResultA = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.ChargePrice
                && calculationFilter.First().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(chargePricesResultA);
        Assert.Equal(requestId, chargePricesResultA.RequestId);
        Assert.Equal(calculationFilter.First(), chargePricesResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("CHARGEPRICE_805", chargePricesResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.ChargePrice, chargePricesResultA.FileContent);

        var chargePricesResultB = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.ChargePrice
                && calculationFilter.Last().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(chargePricesResultB);
        Assert.Equal(requestId, chargePricesResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), chargePricesResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("CHARGEPRICE_806", chargePricesResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.ChargePrice, chargePricesResultB.FileContent);
    }

    [Fact]
    public async Task RequestReportAsync_ForWholesaleFixingWithBasisDataWithSplitResultWithWholeMonth_ReturnsExpectedFiles()
    {
        // Arrange
        var calculationFilter = new Dictionary<string, CalculationId?>
        {
            { "805", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
            { "806", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
        };

        var offset = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time").GetUtcOffset(DateTime.UtcNow);
        var filter = new SettlementReportRequestFilterDto(
            calculationFilter,
            new DateTimeOffset(2024, 1, 1, 1, 0, 0, offset).UtcDateTime,
            new DateTimeOffset(2024, 1, 1, 1, 0, 0, offset).UtcDateTime.AddMonths(1),
            CalculationType.WholesaleFixing,
            null,
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var reportRequest = new SettlementReportRequestDto(true, false, true, true, filter);
        var actorInfo = new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null);

        // Act
        var actual = (await Sut.RequestReportAsync(requestId, reportRequest, actorInfo)).ToList();

        // Assert
        var energyResultA = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.EnergyResult
                && calculationFilter.First().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(energyResultA);
        Assert.Equal(requestId, energyResultA.RequestId);
        Assert.Equal(calculationFilter.First(), energyResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTENERGY_805", energyResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResult, energyResultA.FileContent);

        var energyResultB = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.EnergyResult
                && calculationFilter.Last().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(energyResultB);
        Assert.Equal(requestId, energyResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), energyResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTENERGY_806", energyResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResult, energyResultB.FileContent);

        var wholesaleResultA = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.WholesaleResult
                && calculationFilter.First().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(wholesaleResultA);
        Assert.Equal(requestId, wholesaleResultA.RequestId);
        Assert.Equal(calculationFilter.First(), wholesaleResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTWHOLESALE_805", wholesaleResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.WholesaleResult, wholesaleResultA.FileContent);

        var wholesaleResultB = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.WholesaleResult
                && calculationFilter.Last().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(wholesaleResultB);
        Assert.Equal(requestId, wholesaleResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), wholesaleResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTWHOLESALE_806", wholesaleResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.WholesaleResult, wholesaleResultB.FileContent);

        var chargeLinkPeriodsResultA = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.ChargeLinksPeriods
                && calculationFilter.First().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(chargeLinkPeriodsResultA);
        Assert.Equal(requestId, chargeLinkPeriodsResultA.RequestId);
        Assert.Equal(calculationFilter.First(), chargeLinkPeriodsResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("CHARGELINK_805", chargeLinkPeriodsResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.ChargeLinksPeriods, chargeLinkPeriodsResultA.FileContent);

        var chargeLinkPeriodsResultB = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.ChargeLinksPeriods
                && calculationFilter.Last().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(chargeLinkPeriodsResultB);
        Assert.Equal(requestId, chargeLinkPeriodsResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), chargeLinkPeriodsResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("CHARGELINK_806", chargeLinkPeriodsResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.ChargeLinksPeriods, chargeLinkPeriodsResultB.FileContent);

        var meteringPointMasterDataResultA = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.MeteringPointMasterData
                && calculationFilter.First().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(meteringPointMasterDataResultA);
        Assert.Equal(requestId, meteringPointMasterDataResultA.RequestId);
        Assert.Equal(calculationFilter.First(), meteringPointMasterDataResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("MDMP_805", meteringPointMasterDataResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.MeteringPointMasterData, meteringPointMasterDataResultA.FileContent);

        var meteringPointMasterDataResultB = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.MeteringPointMasterData
                && calculationFilter.Last().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(meteringPointMasterDataResultB);
        Assert.Equal(requestId, meteringPointMasterDataResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), meteringPointMasterDataResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("MDMP_806", meteringPointMasterDataResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.MeteringPointMasterData, meteringPointMasterDataResultB.FileContent);

        var pt15MResultsA = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.Pt15M
                && calculationFilter.First().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(pt15MResultsA);
        Assert.Equal(requestId, pt15MResultsA.RequestId);
        Assert.Equal(calculationFilter.First(), pt15MResultsA.RequestFilter.GridAreas.Single());
        Assert.Equal("TSSD15_805", pt15MResultsA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.Pt15M, pt15MResultsA.FileContent);

        var pt15MResultsB = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.Pt15M
                && calculationFilter.Last().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(pt15MResultsB);
        Assert.Equal(requestId, pt15MResultsB.RequestId);
        Assert.Equal(calculationFilter.Last(), pt15MResultsB.RequestFilter.GridAreas.Single());
        Assert.Equal("TSSD15_806", pt15MResultsB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.Pt15M, pt15MResultsB.FileContent);

        var pt1HResultsA = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.Pt1H
                && calculationFilter.First().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(pt1HResultsA);
        Assert.Equal(requestId, pt1HResultsA.RequestId);
        Assert.Equal(calculationFilter.First(), pt1HResultsA.RequestFilter.GridAreas.Single());
        Assert.Equal("TSSD60_805", pt1HResultsA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.Pt1H, pt1HResultsA.FileContent);

        var pt1HResultsB = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.Pt1H
                && calculationFilter.Last().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(pt1HResultsB);
        Assert.Equal(requestId, pt1HResultsB.RequestId);
        Assert.Equal(calculationFilter.Last(), pt1HResultsB.RequestFilter.GridAreas.Single());
        Assert.Equal("TSSD60_806", pt1HResultsB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.Pt1H, pt1HResultsB.FileContent);

        var chargePricesResultA = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.ChargePrice
                && calculationFilter.First().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(chargePricesResultA);
        Assert.Equal(requestId, chargePricesResultA.RequestId);
        Assert.Equal(calculationFilter.First(), chargePricesResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("CHARGEPRICE_805", chargePricesResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.ChargePrice, chargePricesResultA.FileContent);

        var chargePricesResultB = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.ChargePrice
                && calculationFilter.Last().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(chargePricesResultB);
        Assert.Equal(requestId, chargePricesResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), chargePricesResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("CHARGEPRICE_806", chargePricesResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.ChargePrice, chargePricesResultB.FileContent);

        var wholeMonthResultResultA = actual.
            FirstOrDefault(x =>
                x.FileContent == SettlementReportFileContent.MonthlyAmount
                && calculationFilter.First().Equals(x.RequestFilter.GridAreas.Single()));

        Assert.NotNull(wholeMonthResultResultA);
        Assert.Equal(requestId, wholeMonthResultResultA.RequestId);
        Assert.Equal(calculationFilter.First(), wholeMonthResultResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTMONTHLY_805", wholeMonthResultResultA.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.MonthlyAmount, wholeMonthResultResultA.FileContent);

        var wholeMonthResultResultB = actual.FirstOrDefault(x => x.FileContent == SettlementReportFileContent.MonthlyAmount && calculationFilter.Last().Equals(x.RequestFilter.GridAreas.Single()));
        Assert.NotNull(wholeMonthResultResultB);
        Assert.Equal(requestId, wholeMonthResultResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), wholeMonthResultResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTMONTHLY_806", wholeMonthResultResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.MonthlyAmount, wholeMonthResultResultB.FileContent);
    }
}
