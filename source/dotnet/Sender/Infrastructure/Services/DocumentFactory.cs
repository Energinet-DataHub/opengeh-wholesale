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

using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Persistence.Processes;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Sender.Infrastructure.Services;

public class DocumentFactory : IDocumentFactory
{
    private const string CimTemplate = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<cim:NotifyAggregatedMeasureData_MarketDocument xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:cim=""urn:ediel.org:measure:notifyaggregatedmeasuredata:0:1"" xsi:schemaLocation=""urn:ediel.org:measure:notifyaggregatedmeasuredata:0:1 urn-ediel-org-measure-notifyaggregatedmeasuredata-0-1.xsd"">
    <cim:mRID>{documentId}</cim:mRID>
    <cim:type>E31</cim:type>
    <cim:process.processType>D04</cim:process.processType>
    <cim:businessSector.type>23</cim:businessSector.type>
    <cim:sender_MarketParticipant.mRID codingScheme=""A10"">5790001330552</cim:sender_MarketParticipant.mRID>
    <cim:sender_MarketParticipant.marketRole.type>DGL</cim:sender_MarketParticipant.marketRole.type>
    <cim:receiver_MarketParticipant.mRID codingScheme=""A10"">{recipientGln}</cim:receiver_MarketParticipant.mRID>
    <cim:receiver_MarketParticipant.marketRole.type>MDR</cim:receiver_MarketParticipant.marketRole.type>
    <cim:createdDateTime>{createdDateTime}</cim:createdDateTime>
    <cim:Series>
        <!-- content will be added in future releases -->
    </cim:Series>
</cim:NotifyAggregatedMeasureData_MarketDocument>";

    private readonly IProcessRepository _processRepository;
    private readonly IStorageHandler _storageHandler;
    private readonly IClock _clock;
    private readonly IDocumentIdGenerator _documentIdGenerator;

    public DocumentFactory(IProcessRepository processRepository, IStorageHandler storageHandler, IClock clock, IDocumentIdGenerator documentIdGenerator)
    {
        _processRepository = processRepository;
        _storageHandler = storageHandler;
        _clock = clock;
        _documentIdGenerator = documentIdGenerator;
    }

    public async Task CreateAsync(DataBundleRequestDto request, Stream outputStream)
    {
        var notificationIds = await _storageHandler
            .GetDataAvailableNotificationIdsAsync(request)
            .ConfigureAwait(false);

        // Currently bundling is not supported
        var notificationId = notificationIds.Single();

        var messageHubReference = new MessageHubReference(notificationId);
        var process = await _processRepository.GetAsync(messageHubReference).ConfigureAwait(false);

        var document = CimTemplate
            .Replace("{documentId}", _documentIdGenerator.Create())
            .Replace("{recipientGln}", GetMdrGlnForGridArea(process.GridAreaCode))
            .Replace("{createdDateTime}", _clock.GetCurrentInstant().ToString());

        await WriteToStreamAsync(document, outputStream).ConfigureAwait(false);
    }

    private static string GetMdrGlnForGridArea(string gridAreaCode)
    {
        var gln = gridAreaCode switch
        {
            "805" => "8200000007739",
            "806" => "8200000007746",
            _ => throw new NotImplementedException("Only test grid areas 805 and 806 are supported."),
        };
        return gln;
    }

    private static async Task WriteToStreamAsync(string s, Stream outputStream)
    {
        var writer = new StreamWriter(outputStream);
        await using (writer.ConfigureAwait(false))
        {
            await writer.WriteAsync(s).ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
        }
    }
}
