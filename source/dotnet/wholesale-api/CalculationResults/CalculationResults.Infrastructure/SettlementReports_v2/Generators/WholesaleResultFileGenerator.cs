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
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Resolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Generators;

public sealed class WholesaleResultFileGenerator : ISettlementReportFileGenerator
{
    private const int ChunkSize = 100;

    private readonly ISettlementReportWholesaleRepository _dataSource;
    private readonly CalculationType _calculationType;

    public WholesaleResultFileGenerator(ISettlementReportWholesaleRepository dataSource, CalculationType calculationType)
    {
        _dataSource = dataSource;
        _calculationType = calculationType;
    }

    public string FileExtension => ".csv";

    public async Task<int> CountChunksAsync(SettlementReportRequestFilterDto filter)
    {
        var count = await _dataSource.CountAsync(_calculationType, filter).ConfigureAwait(false);
        return (int)Math.Ceiling(count / (double)ChunkSize);
    }

    public async Task WriteAsync(SettlementReportRequestFilterDto filter, int chunkOffset, StreamWriter destination)
    {
        var csvHelper = new CsvWriter(destination, new CultureInfo(filter.CsvFormatLocale ?? "en-US"));
        csvHelper.Context.RegisterClassMap<SettlementReportWholesaleResultRowMap>();

        await using (csvHelper.ConfigureAwait(false))
        {
            csvHelper.WriteHeader<SettlementReportWholesaleResultRowMap>();
            await csvHelper.NextRecordAsync().ConfigureAwait(false);

            await foreach (var record in _dataSource.GetAsync(_calculationType, filter, chunkOffset * ChunkSize, ChunkSize).ConfigureAwait(false))
            {
                csvHelper.WriteRecord(record);
                await csvHelper.NextRecordAsync().ConfigureAwait(false);
            }
        }
    }

    public sealed class SettlementReportWholesaleResultRowMap : ClassMap<SettlementReportWholesaleResultRow>
    {
        public SettlementReportWholesaleResultRowMap()
        {
            Map(r => r.EnergyBusinessProcess)
                .Name("ENERGYBUSINESSPROCESS")
                .Index(0);

            Map(r => r.ProcessVariant)
                .Name("PROCESSVARIANT")
                .Index(1);

            Map(r => r.GridArea)
                .Name("METERINGGRIDAREAID")
                .Index(2);

            Map(r => r.EnergySupplierId)
                .Name("ENERGYSUPPLIERID")
                .Index(3);

            Map(r => r.StartDateTime)
                .Name("STARTDATETIME")
                .Index(4);

            Map(r => r.Resolution)
                .Name("RESOLUTIONDURATION")
                .Index(5)
                .Convert(row => row.Value.Resolution switch
                {
                    Resolution.Hour => "PT1H",
                    Resolution.Day => "P1D",
                    _ => throw new ArgumentOutOfRangeException(nameof(row)),
                });

            Map(r => r.MeteringPointType)
                .Name("TYPEOFMP")
                .Index(6)
                .Convert(row => row.Value.MeteringPointType switch
                {
                    null => string.Empty,
                    MeteringPointType.Consumption => "E17",
                    MeteringPointType.Production => "E18",
                    MeteringPointType.Exchange => "E20",
                    _ => throw new ArgumentOutOfRangeException(nameof(row)),
                });

            Map(r => r.SettlementMethod)
                .Name("SETTLEMENTMETHOD")
                .Index(7)
                .Convert(row => row.Value.SettlementMethod switch
                {
                    null => string.Empty,
                    SettlementMethod.NonProfiled => "E02",
                    SettlementMethod.Flex => "D01",
                    _ => throw new ArgumentOutOfRangeException(nameof(row)),
                });

            Map(r => r.QuantityUnit)
                .Name("MEASUREUNIT")
                .Index(8)
                .Convert(row => row.Value.QuantityUnit switch
                {
                    QuantityUnit.Kwh => "KWH",
                    QuantityUnit.Pieces => "PCS",
                    _ => throw new ArgumentOutOfRangeException(nameof(row)),
                });

            Map(r => r.Currency)
                .Name("ENERGYCURRENCY")
                .Index(9)
                .Convert(row => row.Value.Currency switch
                {
                    Currency.DKK => "DKK",
                    _ => throw new ArgumentOutOfRangeException(nameof(row)),
                });

            Map(r => r.Quantity)
                .Name("ENERGYCURRENCY")
                .Index(10)
                .Data.TypeConverterOptions.Formats = ["#,##0"];

            Map(r => r.Price)
                .Name("PRICE")
                .Index(11)
                .Data.TypeConverterOptions.Formats = ["0.000"];

            Map(r => r.Amount)
                .Name("AMOUNT")
                .Index(12)
                .Data.TypeConverterOptions.Formats = ["#,##0"];

            Map(r => r.ChargeType)
                .Name("CHARGETYPE")
                .Index(13)
                .Convert(row => row.Value.ChargeType switch
                {
                    ChargeType.Tariff => "D03",
                    ChargeType.Fee => "D02",
                    ChargeType.Subscription => "D01",
                    _ => throw new ArgumentOutOfRangeException(nameof(row)),
                });

            Map(r => r.ChargeCode)
                .Name("CHARGETYPEID")
                .Index(14);

            Map(r => r.ChargeOwnerId)
                .Name("CHARGETYPEOWNERID")
                .Index(15);
        }
    }
}
