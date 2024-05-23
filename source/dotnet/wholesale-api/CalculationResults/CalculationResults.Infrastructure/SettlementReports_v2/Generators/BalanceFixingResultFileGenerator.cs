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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Generators;

public sealed class BalanceFixingResultFileGenerator : ISettlementReportFileGenerator
{
    private readonly ISettlementReportDataRepository _dataSource;

    public BalanceFixingResultFileGenerator(ISettlementReportDataRepository dataSource)
    {
        _dataSource = dataSource;
    }

    public string FileExtension => ".csv";

    public async Task WriteAsync(SettlementReportRequestFilterDto filter, StreamWriter destination)
    {
        var csvHelper = new CsvWriter(destination, new CultureInfo(filter.CsvFormatLocale ?? "en-US"));
        csvHelper.Context.RegisterClassMap<SettlementReportResultRowMap>();

        await using (csvHelper.ConfigureAwait(false))
        {
            await csvHelper
                .WriteRecordsAsync(_dataSource.TryReadBalanceFixingResultsAsync(filter))
                .ConfigureAwait(false);
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

            Map(r => r.Quantity)
                .Name("ENERGYBUSINESSPROCESS")
                .Index(1)
                .Convert(_ => "D04");

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

            Map(r => r.Quantity)
                .Name("ENERGYSUPPLIERID")
                .Index(7)
                .Convert(_ => string.Empty);
        }
    }
}
