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
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Statements;

public sealed class SettlementReportMonthlyAmountCountQueryStatement : DatabricksStatement
{
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions;
    private readonly SettlementReportMonthlyAmountQueryFilter _filter;

    public SettlementReportMonthlyAmountCountQueryStatement(IOptions<DeltaTableOptions> deltaTableOptions, SettlementReportMonthlyAmountQueryFilter filter)
    {
        _deltaTableOptions = deltaTableOptions;
        _filter = filter;
    }

    protected override string GetSqlStatement()
    {
        return $"""
                    SELECT COUNT(DISTINCT({SettlementReportMonthlyAmountViewColumns.ResultId})) AS {Columns.Count}
                    FROM
                        {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.MONTHLY_AMOUNTS_V1_VIEW_NAME}
                    WHERE
                        {SettlementReportMonthlyAmountViewColumns.GridAreaCode} = '{SqlStringSanitizer.Sanitize(_filter.GridAreaCode)}' AND
                        {SettlementReportMonthlyAmountViewColumns.CalculationType} = '{CalculationTypeMapper.ToDeltaTableValue(_filter.CalculationType)}' AND
                        {SettlementReportMonthlyAmountViewColumns.Time} >= '{_filter.PeriodStart}' AND
                        {SettlementReportMonthlyAmountViewColumns.Time} < '{_filter.PeriodEnd}' AND
                        {(_filter.EnergySupplier is null ? string.Empty : SettlementReportMonthlyAmountViewColumns.EnergySupplierId + " = '" + SqlStringSanitizer.Sanitize(_filter.EnergySupplier) + "' AND")} 
                        {SettlementReportMonthlyAmountViewColumns.CalculationId} = '{_filter.CalculationId}'
                """;
    }

    public static class Columns
    {
        public const string Count = "count";
    }
}
