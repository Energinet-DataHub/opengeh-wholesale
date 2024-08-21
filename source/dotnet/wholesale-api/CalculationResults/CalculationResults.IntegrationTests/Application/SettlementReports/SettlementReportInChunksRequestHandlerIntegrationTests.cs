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

public sealed class
    SettlementReportInChunksRequestHandlerIntegrationTests : TestBase<SettlementReportInChunksRequestHandler>
{
    public SettlementReportInChunksRequestHandlerIntegrationTests()
    {
        var mockedRepository = new Mock<ILatestCalculationVersionRepository>();
        mockedRepository
            .Setup(repository => repository.GetLatestCalculationVersionAsync())
            .ReturnsAsync(1);

        var mockedGenerator = new Mock<ISettlementReportFileGenerator>();
        mockedGenerator
            .Setup(generator => generator.CountChunksAsync(
                It.IsAny<SettlementReportRequestFilterDto>(),
                It.IsAny<SettlementReportRequestedByActor>(),
                1))
            .ReturnsAsync(1);

        mockedGenerator
            .Setup(generator => generator.CountChunksAsync(
                It.IsAny<SettlementReportRequestFilterDto>(),
                It.IsAny<SettlementReportRequestedByActor>(),
                long.MaxValue))
            .ReturnsAsync(1);

        var mockedFactory = new Mock<ISettlementReportFileGeneratorFactory>();
        mockedFactory
            .Setup(factory => factory.Create(It.IsAny<SettlementReportFileContent>()))
            .Returns(mockedGenerator.Object);

        Fixture.Inject(mockedFactory.Object);
        Fixture.Inject(mockedRepository.Object);
    }

    [Fact]
    public async Task RequestReportInChunksAsync_OneFileNoOffset_WithMultipleChunks_ReturnsPartialFiles()
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
        var actorInfo = new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null);
        var settlementReportFileRequests = new List<SettlementReportFileRequestDto>
        {
            new(
                requestId,
                SettlementReportFileContent.EnergyResult,
                new SettlementReportPartialFileInfo("RESULTENERGY", false),
                filter,
                1),
        };

        var mockedRepository = new Mock<ILatestCalculationVersionRepository>();
        mockedRepository
            .Setup(repository => repository.GetLatestCalculationVersionAsync())
            .ReturnsAsync(1);

        var mockedGenerator = new Mock<ISettlementReportFileGenerator>();
        mockedGenerator
            .Setup(generator => generator.CountChunksAsync(
                It.IsAny<SettlementReportRequestFilterDto>(),
                It.IsAny<SettlementReportRequestedByActor>(),
                1))
            .ReturnsAsync(2);

        var mockedFactory = new Mock<ISettlementReportFileGeneratorFactory>();
        mockedFactory
            .Setup(factory => factory.Create(It.IsAny<SettlementReportFileContent>()))
            .Returns(mockedGenerator.Object);

        var sut = new SettlementReportInChunksRequestHandler(mockedFactory.Object);

        // Act
        var actual = (await sut.RequestReportInChunksAsync(settlementReportFileRequests, actorInfo)).ToList();

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
    public async Task RequestReportInChunksAsync_OneFile_NoOffset_WithOneChunks_ReturnsPartialFiles()
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
        var actorInfo = new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null);
        var settlementReportFileRequests = new List<SettlementReportFileRequestDto>
        {
            new(
                requestId,
                SettlementReportFileContent.EnergyResult,
                new SettlementReportPartialFileInfo("RESULTENERGY", false),
                filter,
                1),
        };

        var mockedRepository = new Mock<ILatestCalculationVersionRepository>();
        mockedRepository
            .Setup(repository => repository.GetLatestCalculationVersionAsync())
            .ReturnsAsync(1);

        var mockedGenerator = new Mock<ISettlementReportFileGenerator>();
        mockedGenerator
            .Setup(generator => generator.CountChunksAsync(
                It.IsAny<SettlementReportRequestFilterDto>(),
                It.IsAny<SettlementReportRequestedByActor>(),
                1))
            .ReturnsAsync(1);

        var mockedFactory = new Mock<ISettlementReportFileGeneratorFactory>();
        mockedFactory
            .Setup(factory => factory.Create(It.IsAny<SettlementReportFileContent>()))
            .Returns(mockedGenerator.Object);

        var sut = new SettlementReportInChunksRequestHandler(mockedFactory.Object);

        // Act
        var actual = (await sut.RequestReportInChunksAsync(settlementReportFileRequests, actorInfo)).ToList();

        // Assert
        var chunkA = actual[0];
        Assert.Equal(requestId, chunkA.RequestId);
        Assert.Equal(calculationFilter.Single(), chunkA.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTENERGY", chunkA.PartialFileInfo.FileName);
        Assert.Equal(0, chunkA.PartialFileInfo.FileOffset);
        Assert.Equal(0, chunkA.PartialFileInfo.ChunkOffset);
        Assert.Equal(SettlementReportFileContent.EnergyResult, chunkA.FileContent);
    }

    [Fact]
    public async Task RequestReportInChunksAsync_OneFile_WithOffset_WithOneChunks_ReturnsPartialFiles()
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
        var actorInfo = new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null);
        var settlementReportFileRequests = new List<SettlementReportFileRequestDto>
        {
            new(
                requestId,
                SettlementReportFileContent.EnergyResult,
                new SettlementReportPartialFileInfo("RESULTENERGY", false) { FileOffset = 1 },
                filter,
                1),
        };

        var mockedRepository = new Mock<ILatestCalculationVersionRepository>();
        mockedRepository
            .Setup(repository => repository.GetLatestCalculationVersionAsync())
            .ReturnsAsync(1);

        var mockedGenerator = new Mock<ISettlementReportFileGenerator>();
        mockedGenerator
            .Setup(generator => generator.CountChunksAsync(
                It.IsAny<SettlementReportRequestFilterDto>(),
                It.IsAny<SettlementReportRequestedByActor>(),
                1))
            .ReturnsAsync(1);

        var mockedFactory = new Mock<ISettlementReportFileGeneratorFactory>();
        mockedFactory
            .Setup(factory => factory.Create(It.IsAny<SettlementReportFileContent>()))
            .Returns(mockedGenerator.Object);

        var sut = new SettlementReportInChunksRequestHandler(mockedFactory.Object);

        // Act
        var actual = (await sut.RequestReportInChunksAsync(settlementReportFileRequests, actorInfo)).ToList();

        // Assert
        var chunkA = actual[0];
        Assert.Equal(requestId, chunkA.RequestId);
        Assert.Equal(calculationFilter.Single(), chunkA.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTENERGY", chunkA.PartialFileInfo.FileName);
        Assert.Equal(1, chunkA.PartialFileInfo.FileOffset);
        Assert.Equal(0, chunkA.PartialFileInfo.ChunkOffset);
        Assert.Equal(SettlementReportFileContent.EnergyResult, chunkA.FileContent);
    }

    [Fact]
    public async Task RequestReportInChunksAsync_OneFile_WithOffset_WithMultipleChunks_ReturnsPartialFiles()
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
        var actorInfo = new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null);
        var settlementReportFileRequests = new List<SettlementReportFileRequestDto>
        {
            new(
                requestId,
                SettlementReportFileContent.EnergyResult,
                new SettlementReportPartialFileInfo("RESULTENERGY", false) { FileOffset = 1 },
                filter,
                1),
        };

        var mockedRepository = new Mock<ILatestCalculationVersionRepository>();
        mockedRepository
            .Setup(repository => repository.GetLatestCalculationVersionAsync())
            .ReturnsAsync(1);

        var mockedGenerator = new Mock<ISettlementReportFileGenerator>();
        mockedGenerator
            .Setup(generator => generator.CountChunksAsync(
                It.IsAny<SettlementReportRequestFilterDto>(),
                It.IsAny<SettlementReportRequestedByActor>(),
                1))
            .ReturnsAsync(2);

        var mockedFactory = new Mock<ISettlementReportFileGeneratorFactory>();
        mockedFactory
            .Setup(factory => factory.Create(It.IsAny<SettlementReportFileContent>()))
            .Returns(mockedGenerator.Object);

        var sut = new SettlementReportInChunksRequestHandler(mockedFactory.Object);

        // Act
        var actual = (await sut.RequestReportInChunksAsync(settlementReportFileRequests, actorInfo)).ToList();

        // Assert
        var chunkA = actual[0];
        Assert.Equal(requestId, chunkA.RequestId);
        Assert.Equal(calculationFilter.Single(), chunkA.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTENERGY", chunkA.PartialFileInfo.FileName);
        Assert.Equal(1, chunkA.PartialFileInfo.FileOffset);
        Assert.Equal(0, chunkA.PartialFileInfo.ChunkOffset);
        Assert.Equal(SettlementReportFileContent.EnergyResult, chunkA.FileContent);

        var chunkB = actual[1];
        Assert.Equal(requestId, chunkB.RequestId);
        Assert.Equal(calculationFilter.Single(), chunkB.RequestFilter.GridAreas.Single());
        Assert.Equal("RESULTENERGY", chunkB.PartialFileInfo.FileName);
        Assert.Equal(1, chunkB.PartialFileInfo.FileOffset);
        Assert.Equal(1, chunkB.PartialFileInfo.ChunkOffset);
        Assert.Equal(SettlementReportFileContent.EnergyResult, chunkB.FileContent);
    }
}
