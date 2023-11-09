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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;

public static class SettlementReportDataFactory
{
    public static IEnumerable<SettlementReportResultRow> Create(IEnumerable<DatabricksSqlRow> databricksSqlRows)
    {
        return databricksSqlRows.Select(Map);
    }

    private static SettlementReportResultRow Map(DatabricksSqlRow databricksSqlRow)
    {
        var gridArea = databricksSqlRow[EnergyResultColumnNames.GridArea];
        var processType = databricksSqlRow[EnergyResultColumnNames.BatchProcessType];
        var time = databricksSqlRow[EnergyResultColumnNames.Time];
        var timeSeriesType = databricksSqlRow[EnergyResultColumnNames.TimeSeriesType];
        var quantity = databricksSqlRow[EnergyResultColumnNames.Quantity];

        return new SettlementReportResultRow(
            gridArea!,
            ProcessTypeMapper.FromDeltaTableValue(processType!),
            SqlResultValueConverters.ToInstant(time)!.Value,
            "PT15M", // TODO (JMG): store resolution in delta table?
            MeteringPointTypeMapper.FromTimeSeriesTypeDeltaTableValue(timeSeriesType!),
            SettlementMethodMapper.FromTimeSeriesTypeDeltaTableValue(timeSeriesType!),
            SqlResultValueConverters.ToDecimal(quantity)!.Value);
    }
}
