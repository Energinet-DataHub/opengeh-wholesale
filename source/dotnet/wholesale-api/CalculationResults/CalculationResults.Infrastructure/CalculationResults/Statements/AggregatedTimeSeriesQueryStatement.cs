﻿// Copyright 2020 Energinet DataHub A/S
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

public class AggregatedTimeSeriesQueryStatement : DatabricksStatement
{
    private readonly AggregatedTimeSeriesQueryParameters _parameters;
    private readonly DeltaTableOptions _deltaTableOptions;

    public AggregatedTimeSeriesQueryStatement(AggregatedTimeSeriesQueryParameters parameters, DeltaTableOptions deltaTableOptions)
    {
        _parameters = parameters;
        _deltaTableOptions = deltaTableOptions;
    }

    protected override string GetSqlStatement()
    {
        var sql = $@"
            SELECT {string.Join(", ", SqlColumnNames.Select(columnName => $"t1.{columnName}"))}
            FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME} t1
            WHERE ({CreateSqlQueryFilters(_parameters)})";

        // The order is important for combining the rows into packages, since the sql rows are streamed and
        //      packages are created on-the-fly each time a new row is received.
        sql += $"""
                ORDER BY
                    {string.Join(", ", ColumnsToGroupBy.Select(columnName => $"t1.{columnName}"))},
                    t1.{EnergyResultColumnNames.Time}
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
                t1.{EnergyResultColumnNames.TimeSeriesType} IN ('{TimeSeriesTypeMapper.ToDeltaTableValue(timeSeriesType)}')
            AND t1.{EnergyResultColumnNames.AggregationLevel} = '{AggregationLevelMapper.ToDeltaTableValue(timeSeriesType, parameters.EnergySupplierId, parameters.BalanceResponsibleId)}'";

        var calculationPeriodFilter = parameters.LatestCalculationForPeriod
            .Select(calculationForPeriod => $@"
                (t1.{EnergyResultColumnNames.CalculationId} = '{calculationForPeriod.CalculationId}'  
                AND t1.{EnergyResultColumnNames.Time} >= '{calculationForPeriod.Period.Start}'
                AND t1.{EnergyResultColumnNames.Time} < '{calculationForPeriod.Period.End}')")
            .ToList();

        whereClausesSql += $" AND ({string.Join(" OR ", calculationPeriodFilter)})";

        if (parameters.GridAreaCodes.Count > 0)
        {
            whereClausesSql += $"AND t1.{EnergyResultColumnNames.GridArea} IN ({string.Join(",", parameters.GridAreaCodes.Select(gridAreaCode => $"'{gridAreaCode}'"))})";
        }

        if (!string.IsNullOrWhiteSpace(parameters.EnergySupplierId))
        {
            whereClausesSql += $"AND t1.{EnergyResultColumnNames.EnergySupplierId} = '{parameters.EnergySupplierId}'";
        }

        if (!string.IsNullOrWhiteSpace(parameters.BalanceResponsibleId))
        {
            whereClausesSql += $"AND t1.{EnergyResultColumnNames.BalanceResponsibleId} = '{parameters.BalanceResponsibleId}'";
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
        EnergyResultColumnNames.CalculationId,
    ];

    private static string[] SqlColumnNames { get; } =
    {
        EnergyResultColumnNames.CalculationId,
        EnergyResultColumnNames.GridArea,
        EnergyResultColumnNames.FromGridArea,
        EnergyResultColumnNames.TimeSeriesType,
        EnergyResultColumnNames.EnergySupplierId,
        EnergyResultColumnNames.BalanceResponsibleId,
        EnergyResultColumnNames.Time,
        EnergyResultColumnNames.Quantity,
        EnergyResultColumnNames.QuantityQualities,
        EnergyResultColumnNames.CalculationResultId,
        EnergyResultColumnNames.CalculationType,
    };
}
