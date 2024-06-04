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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Generators;

// TODO: Will be replaced by WholesaleResultFileGenerator from next PR.
public sealed class WholesaleFixingResultFileGenerator : ISettlementReportFileGenerator
{
    private readonly ISettlementReportDataRepository _dataSource;
    private readonly CalculationType _calculationType;

    public WholesaleFixingResultFileGenerator(ISettlementReportDataRepository dataSource, CalculationType calculationType)
    {
        _dataSource = dataSource;
        _calculationType = calculationType;
    }

    public string FileExtension => ".csv";

    public Task<int> CountChunksAsync(SettlementReportRequestFilterDto filter)
    {
        return Task.FromResult(1);
    }

    public async Task WriteAsync(SettlementReportRequestFilterDto filter, int chunkOffset, StreamWriter destination)
    {
        var csvHelper = new CsvWriter(destination, new CultureInfo(filter.CsvFormatLocale ?? "en-US"));
        csvHelper.Context.RegisterClassMap<SettlementReportResultRowMap>();

        await using (csvHelper.ConfigureAwait(false))
        {
            if (chunkOffset == 0)
            {
                csvHelper.WriteHeader<SettlementReportResultRow>();
            }

            await foreach (var record in _dataSource.TryReadBalanceFixingResultsAsync(filter).ConfigureAwait(false))
            {
                await csvHelper.NextRecordAsync().ConfigureAwait(false);
                csvHelper.WriteRecord(record);
            }
        }
    }

    public sealed class SettlementReportResultRowMap : ClassMap<SettlementReportResultRow>
    {
        public SettlementReportResultRowMap()
        {
            Map(r => r.GridAreaCode)
                .Name("METERINGGRIDAREAID")
                .Index(0)
                .Convert(row => row.Value.GridAreaCode);

            Map(r => r.CalculationType)
                .Name("ENERGYBUSINESSPROCESS")
                .Index(1)
                .Convert(row => row.Value.CalculationType switch
                {
                    CalculationType.BalanceFixing => "D04",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.Resolution)),
                });

            Map(r => r.Time)
                .Name("STARTDATETIME")
                .Index(2);

            Map(r => r.Resolution)
                .Name("RESOLUTIONDURATION")
                .Index(3)
                .Convert(row => row.Value.Resolution switch
                {
                    Resolution.Hour => "PT1H",
                    Resolution.QuarterHour => "PT15M",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.Resolution)),
                });

            Map(r => r.MeteringPointType)
                .Name("TYPEOFMP")
                .Index(4)
                .Convert(row => row.Value.MeteringPointType switch
                {
                    null => string.Empty,
                    MeteringPointType.Consumption => "E17",
                    MeteringPointType.Production => "E18",
                    MeteringPointType.Exchange => "E20",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.MeteringPointType)),
                });

            Map(r => r.SettlementMethod)
                .Name("SETTLEMENTMETHOD")
                .Index(5)
                .Convert(row => row.Value.SettlementMethod switch
                {
                    null => string.Empty,
                    SettlementMethod.NonProfiled => "E02",
                    SettlementMethod.Flex => "D01",
                    _ => throw new ArgumentOutOfRangeException(nameof(row)),
                });

            Map(r => r.Quantity)
                .Name("ENERGYQUANTITY")
                .Index(6)
                .Data.TypeConverterOptions.Formats = ["0.000"];
        }
    }
}
