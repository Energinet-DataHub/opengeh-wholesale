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
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Statements;

public sealed class SettlementReportLatestEnergyResultCountQueryStatement : DatabricksStatement
{
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions;
    private readonly SettlementReportLatestEnergyResultQueryFilter _filter;

    public SettlementReportLatestEnergyResultCountQueryStatement(IOptions<DeltaTableOptions> deltaTableOptions, SettlementReportLatestEnergyResultQueryFilter filter)
    {
        _deltaTableOptions = deltaTableOptions;
        _filter = filter;
    }

    protected override string GetSqlStatement()
    {
        return $"""
                    SELECT COUNT(DISTINCT(TO_UTC_TIMESTAMP(
                      DATE_TRUNC(
                        'day',
                        FROM_UTC_TIMESTAMP({SettlementReportEnergyResultViewColumns.Time}, 'Europe/Copenhagen')
                      ),
                      'Europe/Copenhagen'
                    ))) AS {SettlementReportEnergyResultCountColumns.Count}
                    FROM
                        {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME}
                    WHERE
                        {SettlementReportEnergyResultViewColumns.GridArea} = '{SqlStringSanitizer.Sanitize(_filter.GridAreaCode)}' AND
                        {SettlementReportEnergyResultViewColumns.Time} >= '{_filter.PeriodStart}' AND
                        {SettlementReportEnergyResultViewColumns.Time} < '{_filter.PeriodEnd}' AND
                        {SettlementReportEnergyResultViewColumns.CalculationVersion} <= '{_filter.MaximumCalculationVersion}' AND
                        {SettlementReportEnergyResultViewColumns.CalculationType} = '{CalculationTypeMapper.ToDeltaTableValue(CalculationType.BalanceFixing)}'
                """;
    }
}
