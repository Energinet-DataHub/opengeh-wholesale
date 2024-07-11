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
using CsvHelper;
using CsvHelper.TypeConversion;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Resolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Generators;

public sealed class ChargePriceFileGenerator : ISettlementReportFileGenerator
{
    private const int ChunkSize = 1_750; // Up to 582 rows in each chunk for a month, 1.018.500 rows per chunk in total.

    private readonly ISettlementReportChargePriceRepository _dataSource;

    public ChargePriceFileGenerator(ISettlementReportChargePriceRepository dataSource)
    {
        _dataSource = dataSource;
    }

    public string FileExtension => ".csv";

    public async Task<int> CountChunksAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo, long maximumCalculationVersion)
    {
        var count = await _dataSource.CountAsync(filter).ConfigureAwait(false);
        return (int)Math.Ceiling(count / (double)ChunkSize);
    }

    public async Task WriteAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo, SettlementReportPartialFileInfo fileInfo, long maximumCalculationVersion, StreamWriter destination)
    {
        var csvHelper = new CsvWriter(destination, new CultureInfo(filter.CsvFormatLocale ?? "en-US"));

        await using (csvHelper.ConfigureAwait(false))
        {
            csvHelper.Context.TypeConverterOptionsCache.AddOptions<decimal>(
                new TypeConverterOptions
                {
                    Formats = ["0.000000"],
                });

            if (fileInfo is { FileOffset: 0, ChunkOffset: 0 })
            {
                await WriteHeaderAsync(csvHelper).ConfigureAwait(false);
            }

            await foreach (var record in _dataSource.GetAsync(filter, fileInfo.ChunkOffset * ChunkSize, ChunkSize).ConfigureAwait(false))
            {
                await WriteRecordAsync(csvHelper, record).ConfigureAwait(false);
            }
        }
    }

    private static async Task WriteHeaderAsync(CsvWriter csvHelper)
    {
        const int energyPriceFieldCount = 25;

        csvHelper.WriteField("CHARGETYPE");
        csvHelper.WriteField("CHARGETYPEID");
        csvHelper.WriteField("CHARGETYPEOWNER");
        csvHelper.WriteField("RESOLUTIONDURATION");
        csvHelper.WriteField("TAXINDICATOR");
        csvHelper.WriteField("STARTDATETIME");

        for (var i = 0; i < energyPriceFieldCount; ++i)
        {
            csvHelper.WriteField($"ENERGYPRICE{i + 1}");
        }

        await csvHelper.NextRecordAsync().ConfigureAwait(false);
    }

    private static async Task WriteRecordAsync(CsvWriter csvHelper, SettlementReportChargePriceRow record)
    {
        csvHelper.WriteField(record.ChargeType switch
        {
            ChargeType.Tariff => "D03",
            ChargeType.Fee => "D02",
            ChargeType.Subscription => "D01",
            _ => throw new ArgumentOutOfRangeException(nameof(record.ChargeType)),
        });

        csvHelper.WriteField(record.ChargeCode);
        csvHelper.WriteField(record.ChargeOwnerId, shouldQuote: true);

        csvHelper.WriteField(record.Resolution switch
        {
            Resolution.Hour => "PT1H",
            Resolution.Day => "P1D",
            Resolution.Month => "P1M",
            _ => throw new ArgumentOutOfRangeException(nameof(record.Resolution)),
        });

        csvHelper.WriteField(record.TaxIndicator ? "1" : "0");
        csvHelper.WriteField(record.StartDateTime);

        foreach (var energyPrice in record.EnergyPrices)
        {
            csvHelper.WriteField(energyPrice);
        }

        await csvHelper.NextRecordAsync().ConfigureAwait(false);
    }
}
