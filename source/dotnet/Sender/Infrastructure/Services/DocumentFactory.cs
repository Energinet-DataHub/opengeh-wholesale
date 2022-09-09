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
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Persistence.Processes;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Sender.Infrastructure.Services;

public class DocumentFactory : IDocumentFactory
{
    // <product> is fixed to 8716867000030 (Active Energy) for current document type.
    // <quantity_Measure_Unit> is fixed to kWh for current document type.
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
        <cim:mRID>{seriesId}</cim:mRID>
        <cim:version>1</cim:version>
        <cim:marketEvaluationPoint.type>E18</cim:marketEvaluationPoint.type>
        <cim:meteringGridArea_Domain.mRID codingScheme=""NDK"">{gridArea}</cim:meteringGridArea_Domain.mRID>
        <cim:product>8716867000030</cim:product>
        <cim:quantity_Measure_Unit.name>KWH</cim:quantity_Measure_Unit.name>
            <cim:Period>
                <cim:resolution>PT15M</cim:resolution>
                <cim:timeInterval>
                    <cim:start>{timeIntervalFrom}</cim:start>
                    <cim:end>{timeIntervalTo}</cim:end>
                </cim:timeInterval>{points}
            </cim:Period>
    </cim:Series>
</cim:NotifyAggregatedMeasureData_MarketDocument>";

    private const string PointTemplate = @"
<cim:Point>
    <cim:position>{position}</cim:position>
    <cim:quantity>{quantity}</cim:quantity>
</cim:Point>";

    private readonly IProcessRepository _processRepository;
    private readonly IStorageHandler _storageHandler;
    private readonly IClock _clock;
    private readonly IDocumentIdGenerator _documentIdGenerator;
    private readonly ISeriesIdGenerator _seriesIdGenerator;
    private readonly IBatchRepository _batchRepository;
    private readonly ICalculatedResultReader _resultReader;

    public DocumentFactory(
        ICalculatedResultReader resultReader,
        IProcessRepository processRepository,
        IStorageHandler storageHandler,
        IClock clock,
        IDocumentIdGenerator documentIdGenerator,
        ISeriesIdGenerator seriesIdGenerator,
        IBatchRepository batchRepository)
    {
        _resultReader = resultReader;
        _processRepository = processRepository;
        _storageHandler = storageHandler;
        _clock = clock;
        _documentIdGenerator = documentIdGenerator;
        _seriesIdGenerator = seriesIdGenerator;
        _batchRepository = batchRepository;
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
        var batchId = process.BatchId;
        var batch = await _batchRepository.GetAsync(batchId).ConfigureAwait(false);

        var result = await _resultReader.ReadResultAsync(process).ConfigureAwait(false);

        var document = CimTemplate
            .Replace("{documentId}", _documentIdGenerator.Create())
            .Replace("{seriesId}", _seriesIdGenerator.Create())
            .Replace("{recipientGln}", GetMdrGlnForGridArea(process.GridAreaCode))
            .Replace("{createdDateTime}", _clock.GetCurrentInstant().ToString())
            .Replace("{timeIntervalFrom}", batch.PeriodStart.ToString())
            .Replace("{timeIntervalTo}", batch.PeriodEnd.ToString())
            .Replace("{points}", CreatePoints(result))
            .Replace("{gridArea}", process.GridAreaCode);

        await WriteToStreamAsync(document, outputStream).ConfigureAwait(false);
    }

    private static string CreatePoints(BalanceFixingResultDto result)
    {
        var sb = new StringBuilder();
        foreach (var point in result.Points)
        {
            sb.Append(PointTemplate
                .Replace("{position}", point.position.ToString())
                .Replace("{quantity}", point.quantity));
        }

        return sb.ToString();
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
        var writer = new StreamWriter(outputStream, leaveOpen: true);
        await using (writer.ConfigureAwait(false))
        {
            await writer.WriteAsync(s).ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
        }
    }
}
