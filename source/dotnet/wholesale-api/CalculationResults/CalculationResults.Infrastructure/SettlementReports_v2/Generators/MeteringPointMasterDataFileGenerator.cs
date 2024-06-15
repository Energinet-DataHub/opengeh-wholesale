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

public sealed class MeteringPointMasterDataFileGenerator : ISettlementReportFileGenerator
{
    private const int ChunkSize = 1000;
    private readonly ISettlementReportMeteringPointMasterDataRepository _dataSource;

    public MeteringPointMasterDataFileGenerator(ISettlementReportMeteringPointMasterDataRepository dataSource)
    {
        _dataSource = dataSource;
    }

    public string FileExtension => ".csv";

    public async Task<int> CountChunksAsync(SettlementReportRequestFilterDto filter)
    {
        var count = await _dataSource.CountAsync(filter).ConfigureAwait(false);
        return (int)Math.Ceiling(count / (double)ChunkSize);
    }

    public async Task WriteAsync(SettlementReportRequestFilterDto filter, int chunkOffset, StreamWriter destination)
    {
        var csvHelper = new CsvWriter(destination, new CultureInfo(filter.CsvFormatLocale ?? "en-US"));
        csvHelper.Context.RegisterClassMap<SettlementReportMeteringPointMasterDataRowMap>();

        await using (csvHelper.ConfigureAwait(false))
        {
            csvHelper.WriteHeader<SettlementReportMeteringPointMasterDataRow>();
            await csvHelper.NextRecordAsync().ConfigureAwait(false);

            await foreach (var record in _dataSource.GetAsync(filter, chunkOffset * ChunkSize, ChunkSize).ConfigureAwait(false))
            {
                csvHelper.WriteRecord(record);
                await csvHelper.NextRecordAsync().ConfigureAwait(false);
            }
        }
    }

    public sealed class SettlementReportMeteringPointMasterDataRowMap : ClassMap<SettlementReportMeteringPointMasterDataRow>
    {
        public SettlementReportMeteringPointMasterDataRowMap()
        {
            Map(r => r.MeteringPointId)
                .Name("METERINGPOINTID")
                .Index(0);

            Map(r => r.PeriodStart)
                .Name("VALIDFROM")
                .Index(1);

            Map(r => r.PeriodEnd)
                .Name("VALIDTO")
                .Index(2);

            Map(r => r.GridAreaId)
                .Name("GRIDAREAID")
                .Index(3);

            Map(r => r.GridAreaToId)
                .Name("TOGRIDAREAID")
                .Index(4);

            Map(r => r.GridAreaFromId)
                .Name("FROMGRIDAREAID")
                .Index(5);

            Map(r => r.MeteringPointType)
                .Name("TYPEOFMP")
                .Index(6)
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

            Map(r => r.SettlementMethod)
                .Name("SETTLEMENTMETHOD")
                .Index(7)
                .Convert(row => row.Value.SettlementMethod switch
                {
                    null => string.Empty,
                    SettlementMethod.NonProfiled => "E02",
                    SettlementMethod.Flex => "D01",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.SettlementMethod)),
                });

            Map(r => r.EnergySupplierId)
                .Name("ENERGYSUPPLIERID")
                .Index(8);
        }
    }
}
