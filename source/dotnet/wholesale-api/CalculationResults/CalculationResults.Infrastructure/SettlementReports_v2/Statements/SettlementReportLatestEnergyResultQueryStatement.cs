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

public sealed class SettlementReportLatestEnergyResultQueryStatement : DatabricksStatement
{
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions;
    private readonly SettlementReportLatestEnergyResultQueryFilter _filter;
    private readonly int _skip;
    private readonly int _take;

    public SettlementReportLatestEnergyResultQueryStatement(IOptions<DeltaTableOptions> deltaTableOptions, SettlementReportLatestEnergyResultQueryFilter filter, int skip, int take)
    {
        _deltaTableOptions = deltaTableOptions;
        _filter = filter;
        _skip = skip;
        _take = take;
    }

    protected override string GetSqlStatement()
    {
        var latestVersionPrDay =
            $"""
                     SELECT MAX({SettlementReportEnergyResultViewColumns.CalculationVersion}) AS max_calc_version, TO_UTC_TIMESTAMP(
                           DATE_TRUNC(
                             'day',
                             FROM_UTC_TIMESTAMP({SettlementReportEnergyResultViewColumns.Time}, 'Europe/Copenhagen')
                           ),
                           'Europe/Copenhagen'
                         ) AS start_of_day
                     FROM
                         {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME}
                     WHERE
                         {SettlementReportEnergyResultViewColumns.GridArea} = '{SqlStringSanitizer.Sanitize(_filter.GridAreaCode)}' AND
                         {SettlementReportEnergyResultViewColumns.Time} >= '{_filter.PeriodStart}' AND
                         {SettlementReportEnergyResultViewColumns.Time} < '{_filter.PeriodEnd}' AND
                         {SettlementReportEnergyResultViewColumns.CalculationVersion} <= '{_filter.MaximumCalculationVersion}' AND
                         {SettlementReportEnergyResultViewColumns.CalculationType} = '{CalculationTypeMapper.ToDeltaTableValue(CalculationType.BalanceFixing)}'
                     GROUP BY 
                        start_of_day LIMIT {_take} OFFSET {_skip}
                 """.Replace(Environment.NewLine, " ");

        var sqlStatement = $"""
                                SELECT {string.Join(", ", [
                                    SettlementReportEnergyResultViewColumns.CalculationId,
                                    SettlementReportEnergyResultViewColumns.CalculationType,
                                    SettlementReportEnergyResultViewColumns.ResultId,
                                    SettlementReportEnergyResultViewColumns.GridArea,
                                    SettlementReportEnergyResultViewColumns.Time,
                                    SettlementReportEnergyResultViewColumns.Resolution,
                                    SettlementReportEnergyResultViewColumns.Quantity,
                                    SettlementReportEnergyResultViewColumns.MeteringPointType,
                                    SettlementReportEnergyResultViewColumns.SettlementMethod,
                                ])}
                                FROM
                                    {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME}
                                JOIN 
                                    ({latestVersionPrDay}) AS latest ON {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME}.{SettlementReportEnergyResultViewColumns.CalculationVersion} = latest.max_calc_version AND
                                      TO_UTC_TIMESTAMP(
                                          DATE_TRUNC(
                                            'day',
                                            FROM_UTC_TIMESTAMP({_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME}.{SettlementReportEnergyResultViewColumns.Time}, 'Europe/Copenhagen')
                                          ),
                                          'Europe/Copenhagen'
                                      ) = latest.start_of_day
                                WHERE
                                    {SettlementReportEnergyResultViewColumns.GridArea} = '{SqlStringSanitizer.Sanitize(_filter.GridAreaCode)}' AND
                                    {SettlementReportEnergyResultViewColumns.Time} >= '{_filter.PeriodStart}' AND
                                    {SettlementReportEnergyResultViewColumns.Time} < '{_filter.PeriodEnd}' AND
                                    {SettlementReportEnergyResultViewColumns.CalculationType} = '{CalculationTypeMapper.ToDeltaTableValue(CalculationType.BalanceFixing)}'
                            """;
        return sqlStatement;
    }
}
