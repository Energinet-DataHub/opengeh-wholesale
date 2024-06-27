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

using CsvHelper.Configuration;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Generators;

public sealed class MonthlyAmountFileGenerator : CsvFileGeneratorBase<SettlementReportMonthlyAmountRow, MonthlyAmountFileGenerator.SettlementReportMonthlyAmountRowMap>
{
    private readonly ISettlementReportMonthlyAmountRepository _dataSource;

    public MonthlyAmountFileGenerator(ISettlementReportMonthlyAmountRepository dataSource)
    : base(250)
    {
        _dataSource = dataSource;
    }

    protected override Task<int> CountAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo, long maximumCalculationVersion)
    {
        return _dataSource.CountAsync(filter, actorInfo);
    }

    protected override IAsyncEnumerable<SettlementReportMonthlyAmountRow> GetAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo, long maximumCalculationVersion, int skipChunks, int takeChunks)
    {
        return _dataSource.GetAsync(filter, actorInfo, skipChunks, takeChunks);
    }

    public sealed class SettlementReportMonthlyAmountRowMap : ClassMap<SettlementReportMonthlyAmountRow>
    {
        public SettlementReportMonthlyAmountRowMap()
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
                    Resolution.Month => "P1M",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.Resolution)),
                });

            Map(r => r.QuantityUnit)
                .Name("MEASUREUNIT")
                .Index(6)
                .Convert(row => row.Value.QuantityUnit switch
                {
                    QuantityUnit.Kwh => "KWH",
                    QuantityUnit.Pieces => "PCS",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.QuantityUnit)),
                });

            Map(r => r.Currency)
                .Name("ENERGYCURRENCY")
                .Index(7)
                .Convert(row => row.Value.Currency switch
                {
                    Currency.DKK => "DKK",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.Currency)),
                });

            Map(r => r.Amount)
                .Name("AMOUNT")
                .Index(8)
                .Data.TypeConverterOptions.Formats = ["0.000000"];

            Map(r => r.ChargeType)
                .Name("CHARGETYPE")
                .Index(9)
                .Convert(row => row.Value.ChargeType switch
                {
                    null => string.Empty,
                    ChargeType.Tariff => "D03",
                    ChargeType.Fee => "D02",
                    ChargeType.Subscription => "D01",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.ChargeType)),
                });

            Map(r => r.ChargeCode)
                .Name("CHARGETYPEID")
                .Index(10);

            Map(r => r.ChargeOwnerId)
                .Name("CHARGETYPEOWNERID")
                .Index(11);
        }
    }
}
