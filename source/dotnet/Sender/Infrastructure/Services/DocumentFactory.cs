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

using System.Xml;
using System.Xml.Serialization;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Persistence.Processes;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Services.NotifyAggregatedMeasureDataMarketDocument;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Sender.Infrastructure.Services;

public class DocumentFactory : IDocumentFactory
{
    // <product> is fixed to 8716867000030 (Active Energy) for current document type.
    // <quantity_Measure_Unit> is fixed to kWh for current document type.

    private readonly IProcessRepository _processRepository;
    private readonly IStorageHandler _storageHandler;
    private readonly IClock _clock;
    private readonly IDocumentIdGenerator _documentIdGenerator;
    private readonly ISeriesIdGenerator _seriesIdGenerator;
    private readonly ICalculatedResultReader _resultReader;

    public DocumentFactory(
        ICalculatedResultReader resultReader,
        IProcessRepository processRepository,
        IStorageHandler storageHandler,
        IClock clock,
        IDocumentIdGenerator documentIdGenerator,
        ISeriesIdGenerator seriesIdGenerator)
    {
        _resultReader = resultReader;
        _processRepository = processRepository;
        _storageHandler = storageHandler;
        _clock = clock;
        _documentIdGenerator = documentIdGenerator;
        _seriesIdGenerator = seriesIdGenerator;
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

        var result = await _resultReader.ReadResultAsync(process).ConfigureAwait(false);

        var points = result.Points.Select(p => new NotifyAggregatedMeasureDataMarketDocument.PointDto(p.position, p.quantity, p.quality)).ToList();

        var document = new NotifyAggregatedMeasureDataMarketDocumentDto(
            _documentIdGenerator.Create(),
            GetMdrGlnForGridArea(process.GridAreaCode),
            _clock.GetCurrentInstant(),
            new SeriesDto(
                _seriesIdGenerator.Create(),
                process.GridAreaCode,
                new PeriodDto(
                    new TimeIntervalDto(CalculateTimeInterval().Start, CalculateTimeInterval().End),
                    points)));

        using var xmlWriter = XmlWriter.Create(outputStream, new XmlWriterSettings() { Indent = true, Async = true, IndentChars = "    " });
        var serializer = new XmlSerializer(typeof(NotifyAggregatedMeasureDataMarketDocumentDto));
        serializer.Serialize(xmlWriter, document);
        await xmlWriter.FlushAsync().ConfigureAwait(false);
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

    private static Interval CalculateTimeInterval()
    {
        var localDate = new LocalDate(2022, 07, 01);

        // These values should be provided by the calculator once they have been computed.
        var from = localDate;
        var to = localDate.PlusDays(1);

        var targetTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!;

        var fromInstant = from.AtMidnight().InZoneStrictly(targetTimeZone).ToInstant();
        var toInstant = to.AtMidnight().InZoneStrictly(targetTimeZone).ToInstant();

        return new Interval(fromInstant, toInstant);
    }
}
