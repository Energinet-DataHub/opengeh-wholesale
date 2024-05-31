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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Statements;

public sealed class SettlementReportWholesaleResultCountQueryStatement : DatabricksStatement
{
    private readonly SettlementReportWholesaleResultQueryFilter _filter;

    public SettlementReportWholesaleResultCountQueryStatement(SettlementReportWholesaleResultQueryFilter filter)
    {
        _filter = filter;
    }

    protected override string GetSqlStatement()
    {
        return $"""
                    SELECT COUNT {SettlementReportWholesaleViewColumns.CalculationId} AS {Columns.Count}
                    FROM
                        settlement_report.wholesale_results_v1
                    WHERE 
                        {SettlementReportWholesaleViewColumns.GridArea} = '{_filter.GridAreaCode}' AND
                        {SettlementReportWholesaleViewColumns.CalculationType} = '{CalculationTypeMapper.ToDeltaTableValue(_filter.CalculationType)}' AND
                        {SettlementReportWholesaleViewColumns.Time} >= '{_filter.PeriodStart}' AND
                        {SettlementReportWholesaleViewColumns.Time} < '{_filter.PeriodEnd}' AND
                        {SettlementReportWholesaleViewColumns.CalculationId} = '{_filter.CalculationId}'
                """;
    }

    public static class Columns
    {
        public const string Count = "count";
    }
}
