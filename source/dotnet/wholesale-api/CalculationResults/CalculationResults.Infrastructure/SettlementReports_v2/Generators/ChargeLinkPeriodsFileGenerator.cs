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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Generators;

public sealed class ChargeLinkPeriodsFileGenerator : ISettlementReportFileGenerator
{
    private const int ChunkSize = 1000;
    private readonly ISettlementReportChargeLinkPeriodsRepository _dataSource;

    public ChargeLinkPeriodsFileGenerator(ISettlementReportChargeLinkPeriodsRepository dataSource)
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
        csvHelper.Context.RegisterClassMap<SettlementReportChargeLinkPeriodsResultRowMap>();

        await using (csvHelper.ConfigureAwait(false))
        {
            if (fileInfo is { FileOffset: 0, ChunkOffset: 0 })
            {
                csvHelper.WriteHeader<SettlementReportChargeLinkPeriodsResultRow>();
                await csvHelper.NextRecordAsync().ConfigureAwait(false);
            }

            await foreach (var record in _dataSource.GetAsync(filter, fileInfo.ChunkOffset * ChunkSize, ChunkSize).ConfigureAwait(false))
            {
                csvHelper.WriteRecord(record);
                await csvHelper.NextRecordAsync().ConfigureAwait(false);
            }
        }
    }

    public sealed class SettlementReportChargeLinkPeriodsResultRowMap : ClassMap<SettlementReportChargeLinkPeriodsResultRow>
    {
        public SettlementReportChargeLinkPeriodsResultRowMap()
        {
            Map(r => r.MeteringPointId)
                .Name("METERINGPOINTID")
                .Index(0);

            Map(r => r.MeteringPointType)
                .Name("TYPEOFMP")
                .Index(1)
                .Convert(row => row.Value.MeteringPointType switch
                {
                    null => string.Empty,
                    MeteringPointType.Consumption => "E17",
                    MeteringPointType.Production => "E18",
                    MeteringPointType.Exchange => "E20",
                    MeteringPointType.VeProduction => "D01",
                    MeteringPointType.NetProduction => "D05",
                    MeteringPointType.SupplyToGrid => "D06",
                    MeteringPointType.ConsumptionFromGrid => "D07",
                    MeteringPointType.WholesaleServicesInformation => "D08",
                    MeteringPointType.OwnProduction => "D09",
                    MeteringPointType.NetFromGrid => "D10",
                    MeteringPointType.NetToGrid => "D11",
                    MeteringPointType.TotalConsumption => "D12",
                    MeteringPointType.ElectricalHeating => "D14",
                    MeteringPointType.NetConsumption => "D15",
                    MeteringPointType.EffectSettlement => "D19",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.MeteringPointType)),
                });

            Map(r => r.ChargeType)
                .Name("CHARGETYPE")
                .Index(2)
                .Convert(row => row.Value.ChargeType switch
                {
                    ChargeType.Tariff => "D03",
                    ChargeType.Fee => "D02",
                    ChargeType.Subscription => "D01",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.ChargeType)),
                });

            Map(r => r.ChargeOwnerId)
                .Name("CHARGETYPEOWNERID")
                .Index(3);

            Map(r => r.ChargeCode)
                .Name("CHARGETYPEID")
                .Index(4);

            Map(r => r.Quantity)
                .Name("CHARGEOCCURRENCES")
                .Index(5);

            Map(r => r.PeriodStart)
                .Name("PERIODSTART")
                .Index(6);

            Map(r => r.PeriodEnd)
                .Name("PERIODEND")
                .Index(7);
        }
    }
}
