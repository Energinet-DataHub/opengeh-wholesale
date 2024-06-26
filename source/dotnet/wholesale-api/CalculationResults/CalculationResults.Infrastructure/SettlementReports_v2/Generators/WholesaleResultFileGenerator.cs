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

using CsvHelper.Configuration;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Resolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Generators;

public sealed class WholesaleResultFileGenerator : CsvFileGeneratorBase<SettlementReportWholesaleResultRow, WholesaleResultFileGenerator.SettlementReportWholesaleResultRowMap>
{
    private readonly ISettlementReportWholesaleRepository _dataSource;

    public WholesaleResultFileGenerator(ISettlementReportWholesaleRepository dataSource)
         : base(1_350) // Up to 31 * 24 rows in each chunk for a month, 1.004.400 rows per chunk in total.
    {
        _dataSource = dataSource;
    }

    protected override Task<int> CountAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestInputActorInfo actorInfo, long maximumCalculationVersion)
    {
        return _dataSource.CountAsync(filter, actorInfo);
    }

    protected override IAsyncEnumerable<SettlementReportWholesaleResultRow> GetAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestInputActorInfo actorInfo, long maximumCalculationVersion, int skipChunks, int takeChunks)
    {
        return _dataSource.GetAsync(filter, actorInfo, skipChunks, takeChunks);
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
                .Index(2)
                .Convert(row => row.Value.GridArea?.PadLeft(3, '0'));

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
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.Resolution)),
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

            Map(r => r.QuantityUnit)
                .Name("MEASUREUNIT")
                .Index(8)
                .Convert(row => row.Value.QuantityUnit switch
                {
                    QuantityUnit.Kwh => "KWH",
                    QuantityUnit.Pieces => "PCS",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.QuantityUnit)),
                });

            Map(r => r.Currency)
                .Name("ENERGYCURRENCY")
                .Index(9)
                .Convert(row => row.Value.Currency switch
                {
                    Currency.DKK => "DKK",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.Currency)),
                });

            Map(r => r.Quantity)
                .Name("ENERGYQUANTITY")
                .Index(10)
                .Data.TypeConverterOptions.Formats = ["0.000"];

            Map(r => r.Price)
                .Name("PRICE")
                .Index(11)
                .Data.TypeConverterOptions.Formats = ["0.000000"];

            Map(r => r.Amount)
                .Name("AMOUNT")
                .Index(12)
                .Data.TypeConverterOptions.Formats = ["0.000000"];

            Map(r => r.ChargeType)
                .Name("CHARGETYPE")
                .Index(13)
                .Convert(row => row.Value.ChargeType switch
                {
                    ChargeType.Tariff => "D03",
                    ChargeType.Fee => "D02",
                    ChargeType.Subscription => "D01",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.ChargeType)),
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
