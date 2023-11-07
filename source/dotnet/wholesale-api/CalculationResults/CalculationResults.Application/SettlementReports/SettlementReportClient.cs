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

using System.Globalization;
using System.IO.Compression;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports;

public class SettlementReportClient : ISettlementReportClient
{
    private readonly IBatchesClient _batchesClient;
    private readonly ISettlementReportResultsCsvWriter _settlementReportResultsCsvWriter;
    private readonly ISettlementReportResultQueries _settlementReportResultQueries;
    private readonly ISettlementReportRepository _settlementReportRepository;

    public SettlementReportClient(
        IBatchesClient batchesClient,
        ISettlementReportResultsCsvWriter settlementReportResultsCsvWriter,
        ISettlementReportRepository settlementReportRepository,
        ISettlementReportResultQueries settlementReportResultQueries)
    {
        _batchesClient = batchesClient;
        _settlementReportResultQueries = settlementReportResultQueries;
        _settlementReportResultsCsvWriter = settlementReportResultsCsvWriter;
        _settlementReportRepository = settlementReportRepository;
    }

    public async Task<SettlementReportDto> GetSettlementReportAsync(Guid batchId)
    {
        var batch = await _batchesClient.GetAsync(batchId).ConfigureAwait(false);
        var report = await _settlementReportRepository.GetSettlementReportAsync(Map(batch)).ConfigureAwait(false);
        return new SettlementReportDto(report.Stream);
    }

    public async Task CreateCompressedSettlementReportAsync(
        Func<Stream> openDestinationStream,
        string[] gridAreaCodes,
        ProcessType processType,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        string? energySupplier,
        string? csvFormatLocale)
    {
        if (processType == ProcessType.Aggregation)
            throw new BusinessValidationException($"{ProcessType.Aggregation} is not a valid process type for settlement reports.");

        var resultRows = await _settlementReportResultQueries.GetRowsAsync(
                gridAreaCodes,
                processType,
                Instant.FromDateTimeOffset(periodStart),
                Instant.FromDateTimeOffset(periodEnd),
                energySupplier)
            .ConfigureAwait(false);

        var destination = openDestinationStream();

        await using (destination.ConfigureAwait(false))
        {
            using var archive = new ZipArchive(destination, ZipArchiveMode.Create, true);

            var zipArchiveEntry = archive.CreateEntry("Result.csv");
            var zipEntryStream = zipArchiveEntry.Open();
            var targetLocale = new CultureInfo(csvFormatLocale ?? "en-US");

            await using (zipEntryStream.ConfigureAwait(false))
            {
                await _settlementReportResultsCsvWriter
                    .WriteAsync(zipEntryStream, resultRows, targetLocale)
                    .ConfigureAwait(false);
            }
        }
    }

    public async Task GetSettlementReportAsync(Guid batchId, string gridAreaCode, Stream outputStream)
    {
        var batch = await _batchesClient.GetAsync(batchId).ConfigureAwait(false);
        await _settlementReportRepository
            .GetSettlementReportAsync(Map(batch), gridAreaCode, outputStream)
            .ConfigureAwait(false);
    }

    private BatchInfo Map(BatchDto batch)
    {
        return new BatchInfo
        {
            Id = batch.BatchId,
            PeriodStart = Instant.FromDateTimeOffset(batch.PeriodStart),
            PeriodEnd = Instant.FromDateTimeOffset(batch.PeriodEnd),
            GridAreaCodes = batch.GridAreaCodes.ToList(),
        };
    }
}
