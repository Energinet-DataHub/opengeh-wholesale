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
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class AggregatedTimeSeriesQueries(
    DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor,
    AggregatedTimeSeriesQueryStatementWhereClauseProvider whereClauseProvider,
    IOptions<DeltaTableOptions> deltaTableOptions)
    : IAggregatedTimeSeriesQueries
{
    private readonly DatabricksSqlWarehouseQueryExecutor _databricksSqlWarehouseQueryExecutor = databricksSqlWarehouseQueryExecutor;
    private readonly AggregatedTimeSeriesQueryStatementWhereClauseProvider _whereClauseProvider = whereClauseProvider;
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions = deltaTableOptions;

    public async IAsyncEnumerable<AggregatedTimeSeries> GetAsync(AggregatedTimeSeriesQueryParameters parameters)
    {
        var calculationTypePerGridAreas =
            await GetCalculationTypeForGridAreasAsync(parameters).ConfigureAwait(false);

        var sqlStatement = new AggregatedTimeSeriesQueryStatement(
            parameters,
            calculationTypePerGridAreas,
            _whereClauseProvider,
            _deltaTableOptions.Value);

        var timeSeriesPoints = new List<EnergyTimeSeriesPoint>();
        DatabricksSqlRow? previous = null;

        await foreach (var databricksCurrentRow in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(sqlStatement, Format.JsonArray).ConfigureAwait(false))
        {
            var current = new DatabricksSqlRow(databricksCurrentRow);

            // Yield a package created from previous data, if the current row belongs to a new package
            if (previous != null
                && (AggregatedTimeSeriesQueryStatement.ColumnsToGroupBy.Any(column => current[column] != previous[column])
                    || current[EnergyResultColumnNames.CalculationId] != previous[EnergyResultColumnNames.CalculationId]))
            {
                yield return AggregatedTimeSeriesFactory.Create(previous, timeSeriesPoints);
                timeSeriesPoints = [];
            }

            timeSeriesPoints.Add(EnergyTimeSeriesPointFactory.CreateTimeSeriesPoint(current));
            previous = current;
        }

        // Yield the last package
        if (previous != null)
            yield return AggregatedTimeSeriesFactory.Create(previous, timeSeriesPoints);
    }

    private async Task<List<CalculationTypeForGridArea>> GetCalculationTypeForGridAreasAsync(
        AggregatedTimeSeriesQueryParameters queryParameters)
    {
        var calculationTypeForGridAreaStatement =
            new CalculationTypeForGridAreasStatement(_deltaTableOptions.Value, _whereClauseProvider, queryParameters);

        var calculationTypeForGridAreas = await _databricksSqlWarehouseQueryExecutor
            .ExecuteStatementAsync(calculationTypeForGridAreaStatement, Format.JsonArray)
            .Select(d =>
            {
                var databricksSqlRow = new DatabricksSqlRow(d);
                return new CalculationTypeForGridArea(
                    databricksSqlRow[EnergyResultColumnNames.GridArea]!,
                    databricksSqlRow[EnergyResultColumnNames.CalculationType]!);
            })
            .ToListAsync()
            .ConfigureAwait(false);

        return calculationTypeForGridAreas
            .GroupBy(ctpga => ctpga.GridArea)
            .Select(g =>
            {
                if (queryParameters.CalculationType is not null)
                {
                    return new CalculationTypeForGridArea(
                        g.Key,
                        CalculationTypeMapper.ToDeltaTableValue(queryParameters.CalculationType.Value));
                }

                var calculationTypes = g.Select(ctpga => ctpga.CalculationType).ToList();

                if (calculationTypes.Contains(DeltaTableCalculationType.ThirdCorrectionSettlement))
                {
                    return new CalculationTypeForGridArea(g.Key, DeltaTableCalculationType.ThirdCorrectionSettlement);
                }

                if (calculationTypes.Contains(DeltaTableCalculationType.SecondCorrectionSettlement))
                {
                    return new CalculationTypeForGridArea(g.Key, DeltaTableCalculationType.SecondCorrectionSettlement);
                }

                if (calculationTypes.Contains(DeltaTableCalculationType.FirstCorrectionSettlement))
                {
                    return new CalculationTypeForGridArea(g.Key, DeltaTableCalculationType.FirstCorrectionSettlement);
                }

                return new CalculationTypeForGridArea(g.Key, DeltaTableCalculationType.BalanceFixing);
            })
            .Where(ctpga => queryParameters.CalculationType is not null
                            || ctpga.CalculationType != DeltaTableCalculationType.BalanceFixing)
            .Distinct()
            .ToList();
    }

    private class CalculationTypeForGridAreasStatement(
        DeltaTableOptions deltaTableOptions,
        AggregatedTimeSeriesQueryStatementWhereClauseProvider whereClauseProvider,
        AggregatedTimeSeriesQueryParameters queryParameters)
        : DatabricksStatement
    {
        private readonly DeltaTableOptions _deltaTableOptions = deltaTableOptions;
        private readonly AggregatedTimeSeriesQueryStatementWhereClauseProvider _whereClauseProvider = whereClauseProvider;
        private readonly AggregatedTimeSeriesQueryParameters _queryParameters = queryParameters;

        protected override string GetSqlStatement()
        {
            var sql = $"""
                       SELECT {EnergyResultColumnNames.GridArea}, {EnergyResultColumnNames.CalculationType}
                       FROM (SELECT wr.*
                             FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME} wr
                             INNER JOIN {_deltaTableOptions.BasisDataSchemaName}.{_deltaTableOptions.CALCULATIONS_TABLE_NAME} cs
                             ON wr.{EnergyResultColumnNames.CalculationId} = cs.{BasisDataCalculationsColumnNames.CalculationId}) wrv
                       """;

            sql = _whereClauseProvider.AddWhereClauseToSqlExpression(sql, _queryParameters, "wrv");

            sql += $"""
                    {"\n"}
                    GROUP BY {EnergyResultColumnNames.GridArea}, {EnergyResultColumnNames.CalculationType}
                    """;

            return sql;
        }
    }
}
