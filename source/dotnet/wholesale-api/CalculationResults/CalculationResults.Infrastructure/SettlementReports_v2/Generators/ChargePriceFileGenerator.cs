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
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Resolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Generators;

public sealed class ChargePriceFileGenerator : ISettlementReportFileGenerator
{
    private const int ChunkSize = 1000;

    private readonly ISettlementReportChargePriceRepository _dataSource;

    public ChargePriceFileGenerator(ISettlementReportChargePriceRepository dataSource)
    {
        _dataSource = dataSource;
    }

    public string FileExtension => ".csv";

    public async Task<int> CountChunksAsync(SettlementReportRequestFilterDto filter)
    {
        var count = await _dataSource.CountAsync(filter).ConfigureAwait(false);
        return (int)Math.Ceiling(count / (double)ChunkSize);
    }

    public async Task WriteAsync(SettlementReportRequestFilterDto filter, SettlementReportPartialFileInfo fileInfo, StreamWriter destination)
    {
        var csvHelper = new CsvWriter(destination, new CultureInfo(filter.CsvFormatLocale ?? "en-US"));
        csvHelper.Context.RegisterClassMap<SettlementReportChargePriceRowMap>();

        await using (csvHelper.ConfigureAwait(false))
        {
            csvHelper.Context.TypeConverterOptionsCache.AddOptions<decimal>(
                new TypeConverterOptions
                {
                    Formats = ["0.000"],
                });

            if (fileInfo is { FileOffset: 0, ChunkOffset: 0 })
            {
                csvHelper.WriteHeader<SettlementReportChargePriceRow>();
                const int energyPriceFieldCount = 24;
                for (var i = 0; i < energyPriceFieldCount; ++i)
                {
                    csvHelper.WriteField($"ENERGYPRICE{i + 1}");
                }

                await csvHelper.NextRecordAsync().ConfigureAwait(false);
            }

            await foreach (var record in _dataSource.GetAsync(filter, fileInfo.ChunkOffset * ChunkSize, ChunkSize).ConfigureAwait(false))
            {
                csvHelper.WriteRecord(record);
                await csvHelper.NextRecordAsync().ConfigureAwait(false);
            }
        }
    }

    public sealed class SettlementReportChargePriceRowMap : ClassMap<SettlementReportChargePriceRow>
    {
        public SettlementReportChargePriceRowMap()
        {
            Map(r => r.ChargeType)
                .Name("CHARGETYPE")
                .Convert(row => row.Value.ChargeType switch
                {
                    ChargeType.Tariff => "D03",
                    ChargeType.Fee => "D02",
                    ChargeType.Subscription => "D01",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.ChargeType)),
                });

            Map(r => r.ChargeCode)
                .Name("CHARGETYPEID");

            Map(r => r.ChargeOwnerId)
                .Name("CHARGETYPEOWNER");

            Map(r => r.Resolution)
                .Name("RESOLUTIONDURATION")
                .Convert(row => row.Value.Resolution switch
                {
                    Resolution.Hour => "PT1H",
                    Resolution.Day => "P1D",
                    Resolution.Month => "P1M",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.Resolution)),
                });

            Map(r => r.TaxIndicator)
                .Name("TAXINDICATOR");

            Map(r => r.StartDateTime)
                .Name("STARTDATETIME");

            Map(r => r.EnergyPrices)
                .Name("ENERGYPRICE1")
                .TypeConverter<IEnumerableConverter>();
        }
    }
}
