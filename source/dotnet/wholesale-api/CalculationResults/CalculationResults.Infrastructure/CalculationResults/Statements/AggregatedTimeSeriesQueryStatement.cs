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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.EnergyResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public class AggregatedTimeSeriesQueryStatement(
    AggregatedTimeSeriesQueryParameters parameters,
    DeltaTableOptions deltaTableOptions)
    : DatabricksStatement
{
    private readonly AggregatedTimeSeriesQueryParameters _parameters = parameters;
    private readonly DeltaTableOptions _deltaTableOptions = deltaTableOptions;

    protected override string GetSqlStatement()
    {
        var sql = $"""
                   SELECT {string.Join(", ", SqlColumnNames.Select(scn => $"`erv`.`{scn}`"))}, `erv`.`{BasisDataCalculationsColumnNames.Version}`
                   FROM (SELECT {string.Join(", ", SqlColumnNames.Select(scn => $"`er`.`{scn}`"))}, `er`.`{EnergyResultColumnNames.AggregationLevel}`, `cs`.`{BasisDataCalculationsColumnNames.Version}`
                   FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME} er
                   INNER JOIN {_deltaTableOptions.BasisDataSchemaName}.{_deltaTableOptions.CALCULATIONS_TABLE_NAME} cs
                   ON er.{EnergyResultColumnNames.CalculationId} = cs.{BasisDataCalculationsColumnNames.CalculationId}) erv
                   INNER JOIN (SELECT max({BasisDataCalculationsColumnNames.Version}) AS max_version, {EnergyResultColumnNames.Time} AS max_time
                   FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME} er
                   INNER JOIN {_deltaTableOptions.BasisDataSchemaName}.{_deltaTableOptions.CALCULATIONS_TABLE_NAME} cs
                   ON er.{EnergyResultColumnNames.CalculationId} = cs.{BasisDataCalculationsColumnNames.CalculationId}
                   GROUP BY {EnergyResultColumnNames.Time}) maxver
                   ON erv.{EnergyResultColumnNames.Time} = maxver.max_time AND erv.{BasisDataCalculationsColumnNames.Version} = maxver.max_version
                   WHERE ({CreateSqlQueryFilters(_parameters)})
                   """;

        // The order is important for combining the rows into packages, since the sql rows are streamed and packages
        // are created on-the-fly each time a new row is received.
        sql += $"""
                ORDER BY {string.Join(", ", ColumnsToGroupBy)}, {EnergyResultColumnNames.Time};
                """;

        return sql;
    }

    private static string CreateSqlQueryFilters(AggregatedTimeSeriesQueryParameters parameters)
    {
        return string.Join(
            " OR ",
            parameters.TimeSeriesTypes
                .Select(timeSeriesType => CreateSqlQueryFilter(parameters, timeSeriesType))
                .Select(s => $"({s})"));
    }

    private static string CreateSqlQueryFilter(
        AggregatedTimeSeriesQueryParameters parameters,
        TimeSeriesType timeSeriesType)
    {
        var whereClausesSql = $@"
                erv.{EnergyResultColumnNames.TimeSeriesType} IN ('{TimeSeriesTypeMapper.ToDeltaTableValue(timeSeriesType)}')
            AND erv.{EnergyResultColumnNames.AggregationLevel} = '{AggregationLevelMapper.ToDeltaTableValue(timeSeriesType, parameters.EnergySupplierId, parameters.BalanceResponsibleId)}'";

        whereClausesSql +=
            $"""
             AND (erv.{EnergyResultColumnNames.Time} BETWEEN '{parameters.Period.Start}' AND '{parameters.Period.End}')
             """;

        if (parameters.GridAreaCodes.Count > 0)
        {
            whereClausesSql +=
                $" AND erv.{EnergyResultColumnNames.GridArea} IN ({string.Join(",", parameters.GridAreaCodes.Select(gridAreaCode => $"'{gridAreaCode}'"))})";
        }

        if (parameters.EnergySupplierId is not null)
        {
            whereClausesSql += $" AND erv.{EnergyResultColumnNames.EnergySupplierId} = '{parameters.EnergySupplierId}'";
        }

        if (parameters.BalanceResponsibleId is not null)
        {
            whereClausesSql +=
                $" AND erv.{EnergyResultColumnNames.BalanceResponsibleId} = '{parameters.BalanceResponsibleId}'";
        }

        return whereClausesSql;
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
