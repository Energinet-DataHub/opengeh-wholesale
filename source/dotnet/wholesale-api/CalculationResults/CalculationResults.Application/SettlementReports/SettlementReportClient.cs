﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports;

public class SettlementReportClient : ISettlementReportClient
{
    private readonly ISettlementReportResultsCsvWriter _settlementReportResultsCsvWriter;
    private readonly ISettlementReportResultQueries _settlementReportResultQueries;

    public SettlementReportClient(
        ISettlementReportResultsCsvWriter settlementReportResultsCsvWriter,
        ISettlementReportResultQueries settlementReportResultQueries)
    {
        _settlementReportResultQueries = settlementReportResultQueries;
        _settlementReportResultsCsvWriter = settlementReportResultsCsvWriter;
    }

    public async Task CreateCompressedSettlementReportAsync(
        Func<Stream> openDestinationStream,
        string[] gridAreaCodes,
        CalculationType calculationType,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        string? energySupplier,
        string? csvFormatLocale)
    {
        if (calculationType == CalculationType.Aggregation)
            throw new BusinessValidationException($"{CalculationType.Aggregation} is not a valid calculation type for settlement reports.");

        var resultRows = await _settlementReportResultQueries.GetRowsAsync(
                gridAreaCodes,
                calculationType,
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
}
