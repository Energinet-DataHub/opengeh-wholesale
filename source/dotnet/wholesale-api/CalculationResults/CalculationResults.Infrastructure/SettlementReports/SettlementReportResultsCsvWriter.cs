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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;

public sealed class SettlementReportResultsCsvWriter : ISettlementReportResultsCsvWriter
{
    public async Task WriteAsync(
        Stream destination,
        IEnumerable<SettlementReportResultRow> rows,
        CultureInfo csvFormatLocale)
    {
        var writer = new StreamWriter(destination, leaveOpen: true);

        await using (writer.ConfigureAwait(false))
        {
            var csvHelper = new CsvWriter(writer, csvFormatLocale);
            csvHelper.Context.RegisterClassMap<SettlementReportResultRowMap>();

            await using (csvHelper.ConfigureAwait(false))
            {
                await csvHelper.WriteRecordsAsync(rows).ConfigureAwait(false);
            }
        }
    }

    public sealed class SettlementReportResultRowMap : ClassMap<SettlementReportResultRow>
    {
        public SettlementReportResultRowMap()
        {
            Map(r => r.GridArea)
                .Name("METERINGGRIDAREAID");

            Map(r => r.ProcessType)
                .Name("ENERGYBUSINESSPROCESS")
                .Convert(ConvertProcessTypeToCsvField);

            Map(r => r.Time)
                .Name("STARTDATETIME");

            Map(r => r.Resolution)
                .Name("RESOLUTIONDURATION");

            Map(r => r.MeteringPointType)
                .Name("TYPEOFMP")
                .Convert(ConvertMeteringPointTypeToCsvField);

            Map(r => r.SettlementMethod)
                .Name("SETTLEMENTMETHOD")
                .Convert(ConvertSettlementMethodToCsvField);

            Map(r => r.Quantity)
                .Name("ENERGYQUANTITY")
                .Data.TypeConverterOptions.Formats = new[] { "0.000" };
        }

        private static string ConvertProcessTypeToCsvField(ConvertToStringArgs<SettlementReportResultRow> row)
        {
            return row.Value.ProcessType switch
            {
                ProcessType.BalanceFixing => "D04",
                ProcessType.Aggregation => throw new NotSupportedException(),
                _ => throw new ArgumentOutOfRangeException(nameof(row)),
            };
        }

        private static string ConvertMeteringPointTypeToCsvField(ConvertToStringArgs<SettlementReportResultRow> row)
        {
            return row.Value.MeteringPointType switch
            {
                MeteringPointType.Consumption => "E17",
                MeteringPointType.Production => "E18",
                MeteringPointType.Exchange => "E20",
                _ => throw new ArgumentOutOfRangeException(nameof(row)),
            };
        }

        private static string ConvertSettlementMethodToCsvField(ConvertToStringArgs<SettlementReportResultRow> row)
        {
            return row.Value.SettlementMethod switch
            {
                null => string.Empty,
                SettlementMethod.NonProfiled => "E02",
                SettlementMethod.Flex => "D01",
                _ => throw new ArgumentOutOfRangeException(nameof(row)),
            };
        }
    }
}
