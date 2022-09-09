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
using Energinet.DataHub.Core.Schemas;
using Energinet.DataHub.Core.SchemaValidation;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.Wholesale.Sender.Infrastructure;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Persistence.Processes;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Services;
using FluentAssertions;
using Moq;
using NodaTime;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Sender.Infrastructure.Services;

[UnitTest]
public class DocumentFactoryTests
{
    private const string Expected = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<cim:NotifyAggregatedMeasureData_MarketDocument xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:cim=""urn:ediel.org:measure:notifyaggregatedmeasuredata:0:1"" xsi:schemaLocation=""urn:ediel.org:measure:notifyaggregatedmeasuredata:0:1 urn-ediel-org-measure-notifyaggregatedmeasuredata-0-1.xsd"">
    <cim:mRID>f0de3417-71ce-426e-9001-12600da9102a</cim:mRID>
    <cim:type>E31</cim:type>
    <cim:process.processType>D04</cim:process.processType>
    <cim:businessSector.type>23</cim:businessSector.type>
    <cim:sender_MarketParticipant.mRID codingScheme=""A10"">5790001330552</cim:sender_MarketParticipant.mRID>
    <cim:sender_MarketParticipant.marketRole.type>DGL</cim:sender_MarketParticipant.marketRole.type>
    <cim:receiver_MarketParticipant.mRID codingScheme=""A10"">8200000007739</cim:receiver_MarketParticipant.mRID>
    <cim:receiver_MarketParticipant.marketRole.type>MDR</cim:receiver_MarketParticipant.marketRole.type>
    <cim:createdDateTime>2022-07-04T08:05:30Z</cim:createdDateTime>
    <cim:Series>
        <cim:mRID>1B8E673E-DBBD-4611-87A9-C7154937786A</cim:mRID>
        <cim:version>1</cim:version>
        <cim:marketEvaluationPoint.type>E18</cim:marketEvaluationPoint.type>
        <cim:meteringGridArea_Domain.mRID codingScheme=""NDK"">805</cim:meteringGridArea_Domain.mRID>
        <cim:product>8716867000030</cim:product>
        <cim:quantity_Measure_Unit.name>KWH</cim:quantity_Measure_Unit.name>
            <cim:Period>
                <cim:resolution>PT15M</cim:resolution>
                <cim:timeInterval>
                    <cim:start>2022-06-30T22:00:00Z</cim:start>
                    <cim:end>2022-07-01T22:00:00Z</cim:end>
                </cim:timeInterval>
                 <cim:Point>
                     <cim:position>1</cim:position>
                     <cim:quantity>2</cim:quantity>
                 </cim:Point>
            </cim:Period>
    </cim:Series>
</cim:NotifyAggregatedMeasureData_MarketDocument>";

    /// <summary>
    /// Verifies the current completeness state of the document creation.
    /// </summary>
    [Theory]
    [InlineAutoMoqData]
    public async Task CreateAsync_ReturnsRsm014(
        DataBundleRequestDto request,
        Guid anyNotificationId,
        [Frozen] Mock<ICalculatedResultReader> calculatedResultReaderMock,
        [Frozen] Mock<IDocumentIdGenerator> documentIdGeneratorMock,
        [Frozen] Mock<ISeriesIdGenerator> seriesIdGeneratorMock,
        [Frozen] Mock<IProcessRepository> processRepositoryMock,
        [Frozen] Mock<IStorageHandler> storageHandlerMock,
        [Frozen] Mock<IClock> clockMock,
        DocumentFactory sut)
    {
        // Arrange
        MockServices(
            request,
            anyNotificationId,
            calculatedResultReaderMock,
            documentIdGeneratorMock,
            seriesIdGeneratorMock,
            processRepositoryMock,
            storageHandlerMock);

        const string expectedIsoString = "2022-07-04T08:05:30Z";
        var instant = InstantPattern.General.Parse(expectedIsoString).Value;
        clockMock
            .Setup(clock => clock.GetCurrentInstant())
            .Returns(instant);

        await using var outStream = new MemoryStream();

        // Act
        await sut.CreateAsync(request, outStream);
        outStream.Position = 0;

        // Assert
        using var stringReader = new StreamReader(outStream);

        var actual = await stringReader.ReadToEndAsync();

        actual.Should().Be(Expected);
        var reader = new SchemaValidatingReader(outStream, Schemas.CimXml.MeasureNotifyAggregatedMeasureData);
        Assert.False(reader.HasErrors);
    }

    /// <summary>
    /// Verifies the document parses schema validation.
    /// </summary>
    [Theory]
    [InlineAutoMoqData]
    public async Task CreateAsync_ReturnsSchemaCompliantRsm014(
        DataBundleRequestDto request,
        Guid anyNotificationId,
        [Frozen] Mock<ICalculatedResultReader> calculatedResultReaderMock,
        [Frozen] Mock<IDocumentIdGenerator> documentIdGeneratorMock,
        [Frozen] Mock<ISeriesIdGenerator> seriesIdGeneratorMock,
        [Frozen] Mock<IProcessRepository> processRepositoryMock,
        [Frozen] Mock<IStorageHandler> storageHandlerMock,
        [Frozen] Mock<IClock> clockMock,
        DocumentFactory sut)
    {
        // Arrange
        MockServices(
            request,
            anyNotificationId,
            calculatedResultReaderMock,
            documentIdGeneratorMock,
            seriesIdGeneratorMock,
            processRepositoryMock,
            storageHandlerMock);

        const string expectedIsoString = "2022-07-04T08:05:30Z";
        var instant = InstantPattern.General.Parse(expectedIsoString).Value;
        clockMock
            .Setup(clock => clock.GetCurrentInstant())
            .Returns(instant);

        await using var outStream = new MemoryStream();

        // Act
        await sut.CreateAsync(request, outStream);
        outStream.Position = 0;

        // Assert
        var reader = new SchemaValidatingReader(outStream, Schemas.CimXml.MeasureNotifyAggregatedMeasureData);
        Assert.False(reader.HasErrors);
    }

    private static void MockServices(
        DataBundleRequestDto request,
        Guid anyNotificationId,
        Mock<ICalculatedResultReader> calculatedResultReaderMock,
        Mock<IDocumentIdGenerator> documentIdGeneratorMock,
        Mock<ISeriesIdGenerator> seriesIdGeneratorMock,
        Mock<IProcessRepository> processRepositoryMock,
        Mock<IStorageHandler> storageHandlerMock)
    {
        documentIdGeneratorMock
            .Setup(generator => generator.Create())
            .Returns("f0de3417-71ce-426e-9001-12600da9102a");

        seriesIdGeneratorMock
            .Setup(generator => generator.Create())
            .Returns("1B8E673E-DBBD-4611-87A9-C7154937786A");

        var anyGridAreaCode = "805";
        processRepositoryMock
            .Setup(repository => repository.GetAsync(It.IsAny<MessageHubReference>()))
            .ReturnsAsync(
                (MessageHubReference messageHubRef) => new Process(messageHubRef, anyGridAreaCode, Guid.NewGuid()));

        var point = new PointDto(1, "2");

        calculatedResultReaderMock
            .Setup(x => x.ReadResultAsync(It.IsAny<Process>()))
            .ReturnsAsync(new BalanceFixingResultDto(new[] { point }));

        storageHandlerMock
            .Setup(handler => handler.GetDataAvailableNotificationIdsAsync(request))
            .ReturnsAsync(new[] { anyNotificationId });
    }
}
