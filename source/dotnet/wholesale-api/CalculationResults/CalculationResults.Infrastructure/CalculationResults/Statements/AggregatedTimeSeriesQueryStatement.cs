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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public class AggregatedTimeSeriesQueryStatement(
    AggregatedTimeSeriesQueryParameters parameters,
    IReadOnlyCollection<CalculationTypeForGridArea> calculationTypePerGridAreas,
    AggregatedTimeSeriesQuerySnippetProvider whereClauseProvider,
    DeltaTableOptions deltaTableOptions)
    : DatabricksStatement
{
    private readonly AggregatedTimeSeriesQueryParameters _parameters = parameters;
    private readonly IReadOnlyCollection<CalculationTypeForGridArea> _calculationTypePerGridAreas = calculationTypePerGridAreas;
    private readonly AggregatedTimeSeriesQuerySnippetProvider _whereClauseProvider = whereClauseProvider;
    private readonly DeltaTableOptions _deltaTableOptions = deltaTableOptions;

    protected override string GetSqlStatement()
    {
        var sql = $"""
                   SELECT {string.Join(", ", SqlColumnNames.Select(scn => $"`erv`.`{scn}`"))}, `erv`.`{BasisDataCalculationsColumnNames.Version}`
                   FROM (SELECT {string.Join(", ", SqlColumnNames.Select(scn => $"`er`.`{scn}`"))}, `er`.`{EnergyResultColumnNames.AggregationLevel}`, `cs`.`{BasisDataCalculationsColumnNames.Version}`
                   FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME} er
                   INNER JOIN {_deltaTableOptions.BasisDataSchemaName}.{_deltaTableOptions.CALCULATIONS_TABLE_NAME} cs
                   ON er.{EnergyResultColumnNames.CalculationId} = cs.{BasisDataCalculationsColumnNames.CalculationId}
                   WHERE {_whereClauseProvider.GenerateLatestOrFixedCalculationTypeWhereClause(_parameters, _calculationTypePerGridAreas)}) erv
                   INNER JOIN (SELECT max({BasisDataCalculationsColumnNames.Version}) AS max_version, {EnergyResultColumnNames.Time} AS max_time, {string.Join(", ", ColumnsToGroupBy.Select(ctgb => $"{ctgb} AS max_{ctgb}"))}
                   FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME} er
                   INNER JOIN {_deltaTableOptions.BasisDataSchemaName}.{_deltaTableOptions.CALCULATIONS_TABLE_NAME} cs
                   ON er.{EnergyResultColumnNames.CalculationId} = cs.{BasisDataCalculationsColumnNames.CalculationId}
                   {_whereClauseProvider.GetWhereClauseSqlExpression(_parameters, "er")} AND {_whereClauseProvider.GenerateLatestOrFixedCalculationTypeWhereClause(_parameters, _calculationTypePerGridAreas)}
                   GROUP BY {EnergyResultColumnNames.Time}, {string.Join(", ", ColumnsToGroupBy)}) maxver
                   ON erv.{EnergyResultColumnNames.Time} = maxver.max_time AND erv.{BasisDataCalculationsColumnNames.Version} = maxver.max_version AND {string.Join(" AND ", ColumnsToGroupBy.Select(ctgb => $"coalesce(erv.{ctgb}, 'is_null_value') = coalesce(maxver.max_{ctgb}, 'is_null_value')"))}
                   {_whereClauseProvider.GetWhereClauseSqlExpression(_parameters, "erv")}
                   ORDER BY {string.Join(", ", ColumnsToGroupBy)}, {EnergyResultColumnNames.Time};
                   """;

        return sql;
    }

    /// <summary>
    /// Since results are streamed and packages are created on-the-fly, the data need to be ordered so that
    ///     all rows belonging to one package are ordered directly after one another.
    /// </summary>
    public static string[] ColumnsToGroupBy =>
    [
        EnergyResultColumnNames.GridArea,
        EnergyResultColumnNames.TimeSeriesType,
    ];

    private static string[] SqlColumnNames { get; } =
    [
        EnergyResultColumnNames.CalculationId,
        EnergyResultColumnNames.GridArea,
        EnergyResultColumnNames.NeighborGridArea,
        EnergyResultColumnNames.TimeSeriesType,
        EnergyResultColumnNames.EnergySupplierId,
        EnergyResultColumnNames.BalanceResponsibleId,
        EnergyResultColumnNames.Time,
        EnergyResultColumnNames.Quantity,
        EnergyResultColumnNames.QuantityQualities,
        EnergyResultColumnNames.CalculationResultId,
        EnergyResultColumnNames.CalculationType,
        EnergyResultColumnNames.Resolution,
    ];
}
