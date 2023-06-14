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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;
using NodaTime.Text;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;

public static class SettlementReportDataFactory
{
    public static IEnumerable<SettlementReportResultRow> Create(TableChunk tableChunk)
    {
        return Enumerable.Range(0, tableChunk.RowCount)
            .Select(i => new SettlementReportResultRow(
                tableChunk[i, ResultColumnNames.GridArea],
                ProcessTypeMapper.FromDeltaTableValue(tableChunk[i, ResultColumnNames.BatchProcessType]),
                InstantPattern.ExtendedIso.Parse(tableChunk[i, ResultColumnNames.Time]).Value,
                "PT15M", // TODO (JMG): store resolution in delta table?
                MeteringPointTypeMapper.FromDeltaTableValue(tableChunk[i, ResultColumnNames.TimeSeriesType]),
                SettlementMethodMapper.FromDeltaTableValue(tableChunk[i, ResultColumnNames.TimeSeriesType]),
                decimal.Parse(tableChunk[i, ResultColumnNames.Quantity], CultureInfo.InvariantCulture)));
    }
}
