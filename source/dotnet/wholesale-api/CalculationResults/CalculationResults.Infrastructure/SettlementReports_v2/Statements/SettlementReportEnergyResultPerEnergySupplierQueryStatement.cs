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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Statements;

public sealed class SettlementReportEnergyResultPerEnergySupplierQueryStatement : DatabricksStatement
{
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions;
    private readonly SettlementReportEnergyResultPerEnergySupplierQueryFilter _filter;
    private readonly int _skip;
    private readonly int _take;

    public SettlementReportEnergyResultPerEnergySupplierQueryStatement(IOptions<DeltaTableOptions> deltaTableOptions, SettlementReportEnergyResultPerEnergySupplierQueryFilter filter, int skip, int take)
    {
        _deltaTableOptions = deltaTableOptions;
        _filter = filter;
        _skip = skip;
        _take = take;
    }

    protected override string GetSqlStatement()
    {
        var calculationResult =
            $"""
                     SELECT DISTINCT({SettlementReportEnergyResultPerEnergySupplierViewColumns.ResultId})
                     FROM
                         {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_ES_GA_V1_VIEW_NAME}
                     WHERE 
                         {SettlementReportEnergyResultPerEnergySupplierViewColumns.GridArea} = '{_filter.GridAreaCode}' AND
                         {SettlementReportEnergyResultPerEnergySupplierViewColumns.Time} >= '{_filter.PeriodStart}' AND
                         {SettlementReportEnergyResultPerEnergySupplierViewColumns.Time} < '{_filter.PeriodEnd}' AND
                         {SettlementReportEnergyResultPerEnergySupplierViewColumns.CalculationId} = '{_filter.CalculationId}' AND
                         {SettlementReportEnergyResultPerEnergySupplierViewColumns.EnergySupplier} = '{_filter.EnergySupplier}'
                     ORDER BY 
                         {SettlementReportEnergyResultPerEnergySupplierViewColumns.ResultId} LIMIT {_take} OFFSET {_skip}
                 """.Replace(Environment.NewLine, " ");

        var sqlStatement = $"""
                                SELECT {string.Join(", ", [
                                    SettlementReportEnergyResultPerEnergySupplierViewColumns.CalculationId,
                                    SettlementReportEnergyResultPerEnergySupplierViewColumns.CalculationType,
                                    "cr." + SettlementReportEnergyResultPerEnergySupplierViewColumns.ResultId,
                                    SettlementReportEnergyResultPerEnergySupplierViewColumns.GridArea,
                                    SettlementReportEnergyResultPerEnergySupplierViewColumns.Time,
                                    SettlementReportEnergyResultPerEnergySupplierViewColumns.Resolution,
                                    SettlementReportEnergyResultPerEnergySupplierViewColumns.Quantity,
                                    SettlementReportEnergyResultPerEnergySupplierViewColumns.MeteringPointType,
                                    SettlementReportEnergyResultPerEnergySupplierViewColumns.SettlementMethod,
                                    SettlementReportEnergyResultPerEnergySupplierViewColumns.EnergySupplier,
                                ])}
                                FROM
                                    {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_ES_GA_V1_VIEW_NAME}
                                JOIN 
                                    ({calculationResult}) AS cr ON {_deltaTableOptions.Value.SettlementReportSchemaName}.{_deltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_ES_GA_V1_VIEW_NAME}.{SettlementReportEnergyResultPerEnergySupplierViewColumns.ResultId} = cr.{SettlementReportEnergyResultPerEnergySupplierViewColumns.ResultId}
                                WHERE 
                                    {SettlementReportEnergyResultPerEnergySupplierViewColumns.GridArea} = '{_filter.GridAreaCode}' AND
                                    {SettlementReportEnergyResultPerEnergySupplierViewColumns.Time} >= '{_filter.PeriodStart}' AND
                                    {SettlementReportEnergyResultPerEnergySupplierViewColumns.Time} < '{_filter.PeriodEnd}' AND
                                    {SettlementReportEnergyResultPerEnergySupplierViewColumns.CalculationId} = '{_filter.CalculationId}' AND 
                                    {SettlementReportEnergyResultPerEnergySupplierViewColumns.EnergySupplier} = '{_filter.EnergySupplier}'
                            """;
        return sqlStatement;
    }
}
