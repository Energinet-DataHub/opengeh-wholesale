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

using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Resolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Generators;

public sealed class ChargePriceFileGenerator : CsvFileGeneratorBase<SettlementReportChargePriceRow, ChargePriceFileGenerator.SettlementReportChargePriceRowMap>
{
    private readonly ISettlementReportChargePriceRepository _dataSource;

    public ChargePriceFileGenerator(ISettlementReportChargePriceRepository dataSource)
        : base(1000)
    {
        _dataSource = dataSource;
    }

    protected override Task<int> CountAsync(SettlementReportRequestFilterDto filter, long maximumCalculationVersion)
    {
        return _dataSource.CountAsync(filter);
    }

    protected override IAsyncEnumerable<SettlementReportChargePriceRow> GetAsync(SettlementReportRequestFilterDto filter, long maximumCalculationVersion, int skipChunks, int takeChunks)
    {
        return _dataSource.GetAsync(filter, skipChunks, takeChunks);
    }

    protected override void WriteHeader(CsvWriter csvHelper)
    {
        const int energyPriceFieldCount = 25;

        csvHelper.WriteField($"CHARGETYPE");
        csvHelper.WriteField($"CHARGETYPEID");
        csvHelper.WriteField($"CHARGETYPEOWNER");
        csvHelper.WriteField($"RESOLUTIONDURATION");
        csvHelper.WriteField($"TAXINDICATOR");
        csvHelper.WriteField($"STARTDATETIME");

        for (var i = 0; i < energyPriceFieldCount; ++i)
        {
            csvHelper.WriteField($"ENERGYPRICE{i + 1}");
        }
    }

    protected override void ConfigureCsv(CsvWriter csvHelper)
    {
        csvHelper.Context.TypeConverterOptionsCache.AddOptions<decimal>(
            new TypeConverterOptions
            {
                Formats = ["0.000000"],
            });
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
                .TypeConverter<IEnumerableConverter>();
        }
    }
}
