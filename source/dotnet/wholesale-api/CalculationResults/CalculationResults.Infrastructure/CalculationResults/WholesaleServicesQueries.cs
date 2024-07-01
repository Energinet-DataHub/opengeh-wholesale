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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class WholesaleServicesQueries(
    DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor,
    WholesaleServicesQueryStatementWhereClauseProvider whereClauseProvider,
    IOptions<DeltaTableOptions> deltaTableOptions)
    : PackageQueriesBase<WholesaleServices, WholesaleTimeSeriesPoint>(databricksSqlWarehouseQueryExecutor), IWholesaleServicesQueries
{
    private readonly DatabricksSqlWarehouseQueryExecutor _databricksSqlWarehouseQueryExecutor = databricksSqlWarehouseQueryExecutor;
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions = deltaTableOptions;
    private readonly WholesaleServicesQueryStatementWhereClauseProvider _whereClauseProvider = whereClauseProvider;

    public async IAsyncEnumerable<WholesaleServices> GetAsync(WholesaleServicesQueryParameters queryParameters)
    {
        var calculationTypePerGridAreas =
            await GetCalculationTypePerGridAreasAsync(queryParameters).ConfigureAwait(false);

        var sqlStatement = new WholesaleServicesQueryStatement(
            WholesaleServicesQueryStatement.StatementType.Select,
            queryParameters,
            calculationTypePerGridAreas,
            _whereClauseProvider,
            _deltaTableOptions.Value);

        var resultStream = GetDataAsync(sqlStatement);

        await foreach (var wholesaleServices in resultStream.ConfigureAwait(false))
            yield return wholesaleServices;
    }

    public async Task<bool> AnyAsync(WholesaleServicesQueryParameters queryParameters)
    {
        var calculationTypePerGridAreas =
            await GetCalculationTypePerGridAreasAsync(queryParameters).ConfigureAwait(false);

        var sqlStatement = new WholesaleServicesQueryStatement(
            WholesaleServicesQueryStatement.StatementType.Exists,
            queryParameters,
            calculationTypePerGridAreas,
            _whereClauseProvider,
            _deltaTableOptions.Value);

        return await _databricksSqlWarehouseQueryExecutor
            .ExecuteStatementAsync(sqlStatement)
            .AnyAsync()
            .ConfigureAwait(false);
    }

    protected override bool RowBelongsToNewPackage(DatabricksSqlRow current, DatabricksSqlRow previous)
    {
        return WholesaleServicesQueryStatement.ColumnsToGroupBy.Any(column => current[column] != previous[column])
               || current[WholesaleResultColumnNames.CalculationId] != previous[WholesaleResultColumnNames.CalculationId];
    }

    protected override WholesaleServices CreatePackageFromRowData(
        DatabricksSqlRow rowData,
        List<WholesaleTimeSeriesPoint> timeSeriesPoints)
    {
        return WholesaleServicesFactory.Create(rowData, timeSeriesPoints);
    }

    protected override WholesaleTimeSeriesPoint CreateTimeSeriesPoint(DatabricksSqlRow row)
    {
        return WholesaleTimeSeriesPointFactory.Create(row);
    }

    private async Task<List<CalculationTypePerGridArea>> GetCalculationTypePerGridAreasAsync(
        WholesaleServicesQueryParameters queryParameters)
    {
        var calculationTypePerGridAreaStatement =
            new CalculationTypePerGridAreaStatement(_deltaTableOptions.Value, _whereClauseProvider, queryParameters);

        var calculationTypePerGridAreas = await _databricksSqlWarehouseQueryExecutor
            .ExecuteStatementAsync(calculationTypePerGridAreaStatement, Format.JsonArray)
            .Select(d =>
            {
                var databricksSqlRow = new DatabricksSqlRow(d);
                return new CalculationTypePerGridArea(
                    databricksSqlRow[WholesaleResultColumnNames.GridArea]!,
                    databricksSqlRow[WholesaleResultColumnNames.CalculationType]!);
            })
            .ToListAsync()
            .ConfigureAwait(false);

        var foo = calculationTypePerGridAreas
            .GroupBy(ctpga => ctpga.GridArea)
            .Select(g =>
            {
                if (queryParameters.CalculationType is not null)
                {
                    return new CalculationTypePerGridArea(g.Key, CalculationTypeMapper.ToDeltaTableValue(queryParameters.CalculationType.Value));
                }

                if (g.Select(ctpga => ctpga.CalculationType)
                    .Contains(DeltaTableCalculationType.ThirdCorrectionSettlement))
                {
                    return new CalculationTypePerGridArea(g.Key, DeltaTableCalculationType.ThirdCorrectionSettlement);
                }

                if (g.Select(ctpga => ctpga.CalculationType)
                    .Contains(DeltaTableCalculationType.SecondCorrectionSettlement))
                {
                    return new CalculationTypePerGridArea(g.Key, DeltaTableCalculationType.SecondCorrectionSettlement);
                }

                if (g.Select(ctpga => ctpga.CalculationType)
                    .Contains(DeltaTableCalculationType.FirstCorrectionSettlement))
                {
                    return new CalculationTypePerGridArea(g.Key, DeltaTableCalculationType.FirstCorrectionSettlement);
                }

                return new CalculationTypePerGridArea(g.Key, DeltaTableCalculationType.WholesaleFixing);
            })
            .Where(ctpga => queryParameters.CalculationType is not null || ctpga.CalculationType != DeltaTableCalculationType.WholesaleFixing)
            .Distinct()
            .ToList();
        return foo;
    }

    private class CalculationTypePerGridAreaStatement(
        DeltaTableOptions deltaTableOptions,
        WholesaleServicesQueryStatementWhereClauseProvider whereClauseProvider,
        WholesaleServicesQueryParameters queryParameters)
        : DatabricksStatement
    {
        private readonly DeltaTableOptions _deltaTableOptions = deltaTableOptions;
        private readonly WholesaleServicesQueryStatementWhereClauseProvider _whereClauseProvider = whereClauseProvider;
        private readonly WholesaleServicesQueryParameters _queryParameters = queryParameters;

        protected override string GetSqlStatement()
        {
            var sql = $"""
                       SELECT {WholesaleResultColumnNames.GridArea}, {WholesaleResultColumnNames.CalculationType}
                       FROM (SELECT wr.*
                             FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.WHOLESALE_RESULTS_TABLE_NAME} wr
                             INNER JOIN {_deltaTableOptions.BasisDataSchemaName}.{_deltaTableOptions.CALCULATIONS_TABLE_NAME} cs
                             ON wr.{WholesaleResultColumnNames.CalculationId} = cs.{BasisDataCalculationsColumnNames.CalculationId}) wrv
                       """;

            sql = _whereClauseProvider.AddWhereClauseToSqlExpression(sql, _queryParameters);

            sql += $"""
                    {"\n"}
                    GROUP BY {WholesaleResultColumnNames.GridArea}, {WholesaleResultColumnNames.CalculationType}
                    """;

            return sql;
        }
    }
}
