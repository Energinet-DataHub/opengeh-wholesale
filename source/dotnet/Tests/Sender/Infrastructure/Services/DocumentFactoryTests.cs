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
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
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
    private const string ExpectedWhenQualityMeasured = @"<?xml version=""1.0"" encoding=""utf-8""?>
<NotifyAggregatedMeasureData_MarketDocument xmlns=""urn:ediel.org:measure:notifyaggregatedmeasuredata:0:1"">
    <mRID>f0de3417-71ce-426e-9001-12600da9102a</mRID>
    <type>E31</type>
    <process.processType>D04</process.processType>
    <businessSector.type>23</businessSector.type>
    <sender_MarketParticipant.mRID codingScheme=""A10"">5790001330552</sender_MarketParticipant.mRID>
    <sender_MarketParticipant.marketRole.type>DGL</sender_MarketParticipant.marketRole.type>
    <receiver_MarketParticipant.mRID codingScheme=""A10"">8200000007739</receiver_MarketParticipant.mRID>
    <receiver_MarketParticipant.marketRole.type>MDR</receiver_MarketParticipant.marketRole.type>
    <createdDateTime>2022-07-04T08:05:30Z</createdDateTime>
    <Series>
        <mRID>1B8E673E-DBBD-4611-87A9-C7154937786A</mRID>
        <version>1</version>
        <marketEvaluationPoint.type>E18</marketEvaluationPoint.type>
        <meteringGridArea_Domain.mRID codingScheme=""NDK"">805</meteringGridArea_Domain.mRID>
        <product>8716867000030</product>
        <quantity_Measure_Unit.name>KWH</quantity_Measure_Unit.name>
        <Period>
            <resolution>PT15M</resolution>
            <timeInterval>
                <start>2022-05-31T22:00:00Z</start>
                <end>2022-06-01T22:00:00Z</end>
            </timeInterval>
            <Point>
                <position>1</position>
                <quantity>2.000</quantity>
            </Point>
        </Period>
    </Series>
</NotifyAggregatedMeasureData_MarketDocument>";

    private const string ExpectedWhenQualityNotMeasured = @"<?xml version=""1.0"" encoding=""utf-8""?>
<NotifyAggregatedMeasureData_MarketDocument xmlns=""urn:ediel.org:measure:notifyaggregatedmeasuredata:0:1"">
    <mRID>f0de3417-71ce-426e-9001-12600da9102a</mRID>
    <type>E31</type>
    <process.processType>D04</process.processType>
    <businessSector.type>23</businessSector.type>
    <sender_MarketParticipant.mRID codingScheme=""A10"">5790001330552</sender_MarketParticipant.mRID>
    <sender_MarketParticipant.marketRole.type>DGL</sender_MarketParticipant.marketRole.type>
    <receiver_MarketParticipant.mRID codingScheme=""A10"">8200000007739</receiver_MarketParticipant.mRID>
    <receiver_MarketParticipant.marketRole.type>MDR</receiver_MarketParticipant.marketRole.type>
    <createdDateTime>2022-07-04T08:05:30Z</createdDateTime>
    <Series>
        <mRID>1B8E673E-DBBD-4611-87A9-C7154937786A</mRID>
        <version>1</version>
        <marketEvaluationPoint.type>E18</marketEvaluationPoint.type>
        <meteringGridArea_Domain.mRID codingScheme=""NDK"">805</meteringGridArea_Domain.mRID>
        <product>8716867000030</product>
        <quantity_Measure_Unit.name>KWH</quantity_Measure_Unit.name>
        <Period>
            <resolution>PT15M</resolution>
            <timeInterval>
                <start>2022-05-31T22:00:00Z</start>
                <end>2022-06-01T22:00:00Z</end>
            </timeInterval>
            <Point>
                <position>1</position>
                <quantity>2.000</quantity>
                <quality>A05</quality>
            </Point>
        </Period>
    </Series>
</NotifyAggregatedMeasureData_MarketDocument>";

    /// <summary>
    /// Point quality element must be omitted when quality is measured.
    /// </summary>
    [Theory]
    [InlineAutoMoqData(Quality.Incomplete, ExpectedWhenQualityNotMeasured)]
    [InlineAutoMoqData(Quality.Measured, ExpectedWhenQualityMeasured)]
    public async Task CreateAsync_ReturnsRsm014(
        Quality quality,
        string expected,
        DataBundleRequestDto request,
        Guid anyNotificationId,
        [Frozen] Mock<ICalculatedResultReader> calculatedResultReaderMock,
        [Frozen] Mock<IDocumentIdGenerator> documentIdGeneratorMock,
        [Frozen] Mock<ISeriesIdGenerator> seriesIdGeneratorMock,
        [Frozen] Mock<IProcessRepository> processRepositoryMock,
        [Frozen] Mock<IStorageHandler> storageHandlerMock,
        [Frozen] Mock<IClock> clockMock,
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        DocumentFactory sut)
    {
        // Arrange
        MockServices(
            request,
            anyNotificationId,
            quality,
            calculatedResultReaderMock,
            documentIdGeneratorMock,
            seriesIdGeneratorMock,
            processRepositoryMock,
            storageHandlerMock,
            clockMock,
            batchRepositoryMock);

        await using var outStream = new MemoryStream();

        // Act
        await sut.CreateAsync(request, outStream);
        outStream.Position = 0;

        // Assert
        using var stringReader = new StreamReader(outStream);
        var actual = await stringReader.ReadToEndAsync();

        actual = actual.Replace("\r", string.Empty); // Fix x-platform/line-ending problems
        expected = expected.Replace("\r", string.Empty); // Fix x-platform/line-ending problems

        actual.Should().Be(expected);
    }

    /// <summary>
    /// Verifies the document parses schema validation.
    /// Tested with both measured and a non-measured quality as it changes whether the point quality
    /// element is included in the message.
    /// </summary>
    [Theory]
    [InlineAutoMoqData(Quality.Measured)]
    [InlineAutoMoqData(Quality.Estimated)]
    public async Task CreateAsync_ReturnsSchemaCompliantRsm014(
        Quality quality,
        DataBundleRequestDto request,
        Guid anyNotificationId,
        [Frozen] Mock<ICalculatedResultReader> calculatedResultReaderMock,
        [Frozen] Mock<IDocumentIdGenerator> documentIdGeneratorMock,
        [Frozen] Mock<ISeriesIdGenerator> seriesIdGeneratorMock,
        [Frozen] Mock<IProcessRepository> processRepositoryMock,
        [Frozen] Mock<IStorageHandler> storageHandlerMock,
        [Frozen] Mock<IClock> clockMock,
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        DocumentFactory sut)
    {
        // Arrange
        MockServices(
            request,
            anyNotificationId,
            quality,
            calculatedResultReaderMock,
            documentIdGeneratorMock,
            seriesIdGeneratorMock,
            processRepositoryMock,
            storageHandlerMock,
            clockMock,
            batchRepositoryMock);

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
        Quality quality,
        Mock<ICalculatedResultReader> calculatedResultReaderMock,
        Mock<IDocumentIdGenerator> documentIdGeneratorMock,
        Mock<ISeriesIdGenerator> seriesIdGeneratorMock,
        Mock<IProcessRepository> processRepositoryMock,
        Mock<IStorageHandler> storageHandlerMock,
        Mock<IClock> clockMock,
        Mock<IBatchRepository> batchRepositoryMock)
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

        var point = new PointDto(1, "2.000", quality);

        calculatedResultReaderMock
            .Setup(x => x.ReadResultAsync(It.IsAny<Process>()))
            .ReturnsAsync(new BalanceFixingResultDto(new[] { point }));

        storageHandlerMock
            .Setup(handler => handler.GetDataAvailableNotificationIdsAsync(request))
            .ReturnsAsync(new[] { anyNotificationId });

        const string expectedIsoString = "2022-07-04T08:05:30Z";
        var instant = InstantPattern.General.Parse(expectedIsoString).Value;
        clockMock
            .Setup(clock => clock.GetCurrentInstant())
            .Returns(instant);
        var periodStart = Instant.FromUtc(2022, 05, 31, 22, 0);
        var periodEnd = Instant.FromUtc(2022, 06, 01, 22, 0);
        var batch = new Batch(
            ProcessType.BalanceFixing,
            new List<GridAreaCode> { new("805") },
            new Interval(periodStart, periodEnd));
        batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Guid>())).ReturnsAsync(batch);
    }
}
