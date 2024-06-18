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
        var mockedGenerator = new Mock<ISettlementReportFileGenerator>();
        mockedGenerator
            .Setup(generator => generator.CountChunksAsync(It.IsAny<SettlementReportRequestFilterDto>()))
            .ReturnsAsync(1);

        var mockedFactory = new Mock<ISettlementReportFileGeneratorFactory>();
        mockedFactory
            .Setup(factory => factory.Create(It.IsAny<SettlementReportFileContent>()))
            .Returns(mockedGenerator.Object);

        Fixture.Inject(mockedFactory.Object);
    }

    [Fact]
    public async Task RequestReportAsync_ForBalanceFixingChunked_ReturnsPartialFiles()
    {
        // Arrange
        var calculationFilter = new Dictionary<string, CalculationId>
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

        var mockedGenerator = new Mock<ISettlementReportFileGenerator>();
        mockedGenerator
            .Setup(generator => generator.CountChunksAsync(It.IsAny<SettlementReportRequestFilterDto>()))
            .ReturnsAsync(2);

        var mockedFactory = new Mock<ISettlementReportFileGeneratorFactory>();
        mockedFactory
            .Setup(factory => factory.Create(It.IsAny<SettlementReportFileContent>()))
            .Returns(mockedGenerator.Object);

        var sut = new SettlementReportRequestHandler(mockedFactory.Object);

        // Act
        var actual = (await sut.RequestReportAsync(requestId, reportRequest)).ToList();

        // Assert
        var chunkA = actual[0];
        Assert.Equal(requestId, chunkA.RequestId);
        Assert.Equal(calculationFilter.Single(), chunkA.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Energy", chunkA.PartialFileInfo.FileName);
        Assert.Equal(0, chunkA.PartialFileInfo.FileOffset);
        Assert.Equal(0, chunkA.PartialFileInfo.ChunkOffset);
        Assert.Equal(SettlementReportFileContent.EnergyResultLatestPerDay, chunkA.FileContent);

        var chunkB = actual[1];
        Assert.Equal(requestId, chunkB.RequestId);
        Assert.Equal(calculationFilter.Single(), chunkB.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Energy", chunkB.PartialFileInfo.FileName);
        Assert.Equal(0, chunkB.PartialFileInfo.FileOffset);
        Assert.Equal(1, chunkB.PartialFileInfo.ChunkOffset);
        Assert.Equal(SettlementReportFileContent.EnergyResultLatestPerDay, chunkB.FileContent);
    }

    [Fact]
    public async Task RequestReportAsync_ForBalanceFixingWithoutBasisData_ReturnsExpectedFiles()
    {
        // Arrange
        var calculationFilter = new Dictionary<string, CalculationId>
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
        Assert.Equal(SettlementReportFileContent.EnergyResultLatestPerDay, energyResult.FileContent);
    }

    [Fact]
    public async Task RequestReportAsync_ForBalanceFixingWithoutBasisDataWithSplitResult_ReturnsSplitFiles()
    {
        // Arrange
        var calculationFilter = new Dictionary<string, CalculationId>
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
        Assert.Equal(SettlementReportFileContent.EnergyResultLatestPerDay, energyResultA.FileContent);

        var energyResultB = actual[1];
        Assert.Equal(requestId, energyResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), energyResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Energy (806)", energyResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResultLatestPerDay, energyResultB.FileContent);
    }

    [Theory]
    [InlineData(CalculationType.WholesaleFixing, SettlementReportFileContent.WholesaleResult)]
    [InlineData(CalculationType.FirstCorrectionSettlement, SettlementReportFileContent.FirstCorrectionResult)]
    [InlineData(CalculationType.SecondCorrectionSettlement, SettlementReportFileContent.SecondCorrectionResult)]
    [InlineData(CalculationType.ThirdCorrectionSettlement, SettlementReportFileContent.ThirdCorrectionResult)]
    public async Task RequestReportAsync_ForWholesaleFixingWithoutBasisData_ReturnsExpectedFiles(CalculationType calculationType, SettlementReportFileContent fileContent)
    {
        // Arrange
        var calculationFilter = new Dictionary<string, CalculationId>
        {
            { "805", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
        };

        var filter = new SettlementReportRequestFilterDto(
            calculationFilter,
            DateTimeOffset.UtcNow.Date,
            DateTimeOffset.UtcNow.Date.AddDays(2),
            calculationType,
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
        Assert.Equal(SettlementReportFileContent.EnergyResultForCalculationId, energyResult.FileContent);

        var wholesaleResult = actual[1];
        Assert.Equal(requestId, wholesaleResult.RequestId);
        Assert.Equal(calculationFilter.Single(), wholesaleResult.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Wholesale", wholesaleResult.PartialFileInfo.FileName);
        Assert.Equal(fileContent, wholesaleResult.FileContent);
    }

    [Theory]
    [InlineData(CalculationType.WholesaleFixing, SettlementReportFileContent.WholesaleResult)]
    [InlineData(CalculationType.FirstCorrectionSettlement, SettlementReportFileContent.FirstCorrectionResult)]
    [InlineData(CalculationType.SecondCorrectionSettlement, SettlementReportFileContent.SecondCorrectionResult)]
    [InlineData(CalculationType.ThirdCorrectionSettlement, SettlementReportFileContent.ThirdCorrectionResult)]
    public async Task RequestReportAsync_ForWholesaleFixingWithoutBasisDataWithSplitResult_ReturnsExpectedFiles(CalculationType calculationType, SettlementReportFileContent fileContent)
    {
        // Arrange
        var calculationFilter = new Dictionary<string, CalculationId>
        {
            { "805", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
            { "806", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
        };

        var filter = new SettlementReportRequestFilterDto(
            calculationFilter,
            DateTimeOffset.UtcNow.Date,
            DateTimeOffset.UtcNow.Date.AddDays(2),
            calculationType,
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
        Assert.Equal(SettlementReportFileContent.EnergyResultForCalculationId, energyResultA.FileContent);

        var energyResultB = actual[1];
        Assert.Equal(requestId, energyResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), energyResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Energy (806)", energyResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResultForCalculationId, energyResultB.FileContent);

        var wholesaleResultA = actual[2];
        Assert.Equal(requestId, wholesaleResultA.RequestId);
        Assert.Equal(calculationFilter.First(), wholesaleResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Wholesale (805)", wholesaleResultA.PartialFileInfo.FileName);
        Assert.Equal(fileContent, wholesaleResultA.FileContent);

        var wholesaleResultB = actual[3];
        Assert.Equal(requestId, wholesaleResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), wholesaleResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Wholesale (806)", wholesaleResultB.PartialFileInfo.FileName);
        Assert.Equal(fileContent, wholesaleResultB.FileContent);
    }

    [Theory]
    [InlineData(CalculationType.WholesaleFixing, SettlementReportFileContent.WholesaleResult)]
    [InlineData(CalculationType.FirstCorrectionSettlement, SettlementReportFileContent.FirstCorrectionResult)]
    [InlineData(CalculationType.SecondCorrectionSettlement, SettlementReportFileContent.SecondCorrectionResult)]
    [InlineData(CalculationType.ThirdCorrectionSettlement, SettlementReportFileContent.ThirdCorrectionResult)]
    public async Task RequestReportAsync_ForWholesaleFixingWithBasisData_ReturnsExpectedFiles(CalculationType calculationType, SettlementReportFileContent fileContent)
    {
        // Arrange
        var calculationFilter = new Dictionary<string, CalculationId>
        {
            { "805", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
        };

        var filter = new SettlementReportRequestFilterDto(
            calculationFilter,
            DateTimeOffset.UtcNow.Date,
            DateTimeOffset.UtcNow.Date.AddDays(2),
            calculationType,
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
        Assert.Equal(SettlementReportFileContent.EnergyResultForCalculationId, energyResult.FileContent);

        var wholesaleResult = actual[1];
        Assert.Equal(requestId, wholesaleResult.RequestId);
        Assert.Equal(calculationFilter.Single(), wholesaleResult.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Wholesale", wholesaleResult.PartialFileInfo.FileName);
        Assert.Equal(fileContent, wholesaleResult.FileContent);

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
    }

    [Theory]
    [InlineData(CalculationType.WholesaleFixing, SettlementReportFileContent.WholesaleResult)]
    [InlineData(CalculationType.FirstCorrectionSettlement, SettlementReportFileContent.FirstCorrectionResult)]
    [InlineData(CalculationType.SecondCorrectionSettlement, SettlementReportFileContent.SecondCorrectionResult)]
    [InlineData(CalculationType.ThirdCorrectionSettlement, SettlementReportFileContent.ThirdCorrectionResult)]
    public async Task RequestReportAsync_ForWholesaleFixingWithBasisDataWithSplitResult_ReturnsExpectedFiles(CalculationType calculationType, SettlementReportFileContent fileContent)
    {
        // Arrange
        var calculationFilter = new Dictionary<string, CalculationId>
        {
            { "805", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
            { "806", new CalculationId(Guid.Parse("45B9732A-49F8-450B-AA68-ED4661879D6F")) },
        };

        var filter = new SettlementReportRequestFilterDto(
            calculationFilter,
            DateTimeOffset.UtcNow.Date,
            DateTimeOffset.UtcNow.Date.AddDays(2),
            calculationType,
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
        Assert.Equal(SettlementReportFileContent.EnergyResultForCalculationId, energyResultA.FileContent);

        var energyResultB = actual[1];
        Assert.Equal(requestId, energyResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), energyResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Energy (806)", energyResultB.PartialFileInfo.FileName);
        Assert.Equal(SettlementReportFileContent.EnergyResultForCalculationId, energyResultB.FileContent);

        var wholesaleResultA = actual[2];
        Assert.Equal(requestId, wholesaleResultA.RequestId);
        Assert.Equal(calculationFilter.First(), wholesaleResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Wholesale (805)", wholesaleResultA.PartialFileInfo.FileName);
        Assert.Equal(fileContent, wholesaleResultA.FileContent);

        var wholesaleResultB = actual[3];
        Assert.Equal(requestId, wholesaleResultB.RequestId);
        Assert.Equal(calculationFilter.Last(), wholesaleResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Wholesale (806)", wholesaleResultB.PartialFileInfo.FileName);
        Assert.Equal(fileContent, wholesaleResultB.FileContent);

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
    }
}
