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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using NodaTime.Text;
using ProcessType = Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient.Mappers.ProcessType;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;

public static class SettlementReportDataFactory
{
    public static IEnumerable<SettlementReportResultRow> Create(Table table)
    {
        return Enumerable.Range(0, table.Count)
            .Select(i => new SettlementReportResultRow(
                table[i, ResultColumnNames.GridArea],
                ProcessType.FromDeltaTable(table[i, ResultColumnNames.BatchProcessType]),
                InstantPattern.ExtendedIso.Parse(table[i, ResultColumnNames.Time]).Value,
                "PT15M", // TODO: store resolution in delta table?
                MeteringPointTypeMapper.FromDeltaTableValue(table[i, ResultColumnNames.TimeSeriesType]),
                SettlementMethodMapper.FromDeltaTableValue(table[i, ResultColumnNames.TimeSeriesType]),
                decimal.Parse(table[i, ResultColumnNames.Quantity])));
    }
}
