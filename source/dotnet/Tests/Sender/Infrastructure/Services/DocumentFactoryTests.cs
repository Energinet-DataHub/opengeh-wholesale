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
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Persistence.Processes;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Services;
using FluentAssertions;
using Moq;
using NodaTime.Text;
using Xunit;

namespace Energinet.DataHub.Wholesale.Tests.Sender.Infrastructure.Services;

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
        <!-- content will be added in future releases -->
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
        [Frozen] Mock<IDocumentIdGenerator> documentIdGeneratorMock,
        [Frozen] Mock<IProcessRepository> processRepositoryMock,
        [Frozen] Mock<IStorageHandler> storageHandlerMock,
        [Frozen] Mock<NodaTime.IClock> clockMock,
        DocumentFactory sut)
    {
        // Arrange
        documentIdGeneratorMock
            .Setup(generator => generator.Create())
            .Returns("f0de3417-71ce-426e-9001-12600da9102a");

        var anyGridAreaCode = "805";
        processRepositoryMock
            .Setup(repository => repository.GetAsync(It.IsAny<MessageHubReference>()))
            .ReturnsAsync((MessageHubReference messageHubRef) => new Process(messageHubRef, anyGridAreaCode));

        storageHandlerMock
            .Setup(handler => handler.GetDataAvailableNotificationIdsAsync(request))
            .ReturnsAsync(new[] { anyNotificationId });

        const string expectedIsoString = "2022-07-04T08:05:30Z";
        var instant = InstantPattern.General.Parse(expectedIsoString).Value;
        clockMock.Setup(clock => clock.GetCurrentInstant())
            .Returns(instant);

        using var outStream = new MemoryStream();

        // Act
        await sut.CreateAsync(request, outStream);

        // Assert
        using var stringReader = new StreamReader(outStream);
        var actual = await stringReader.ReadToEndAsync();

        actual.Should().Be(Expected);
    }
}
