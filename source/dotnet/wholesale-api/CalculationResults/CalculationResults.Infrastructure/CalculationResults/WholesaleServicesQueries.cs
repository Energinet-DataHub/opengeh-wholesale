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
    WholesaleServicesQueryStatementHelper helper,
    IOptions<DeltaTableOptions> deltaTableOptions)
    : PackageQueriesBase<WholesaleServices, WholesaleTimeSeriesPoint>(databricksSqlWarehouseQueryExecutor), IWholesaleServicesQueries
{
    private readonly DatabricksSqlWarehouseQueryExecutor _databricksSqlWarehouseQueryExecutor = databricksSqlWarehouseQueryExecutor;
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions = deltaTableOptions;
    private readonly WholesaleServicesQueryStatementHelper _helper = helper;

    public async IAsyncEnumerable<WholesaleServices> GetAsync(WholesaleServicesQueryParameters queryParameters)
    {
        var calculationTypePerGridAreas =
            await GetCalculationTypeForGridAreasAsync(queryParameters).ConfigureAwait(false);

        var sqlStatement = new WholesaleServicesQueryStatement(
            WholesaleServicesQueryStatement.StatementType.Select,
            queryParameters,
            calculationTypePerGridAreas,
            _helper,
            _deltaTableOptions.Value);

        var resultStream = GetDataAsync(sqlStatement);

        await foreach (var wholesaleServices in resultStream.ConfigureAwait(false))
            yield return wholesaleServices;
    }

    public async Task<bool> AnyAsync(WholesaleServicesQueryParameters queryParameters)
    {
        var calculationTypePerGridAreas =
            await GetCalculationTypeForGridAreasAsync(queryParameters).ConfigureAwait(false);

        var sqlStatement = new WholesaleServicesQueryStatement(
            WholesaleServicesQueryStatement.StatementType.Exists,
            queryParameters,
            calculationTypePerGridAreas,
            _helper,
            _deltaTableOptions.Value);

        return await _databricksSqlWarehouseQueryExecutor
            .ExecuteStatementAsync(sqlStatement)
            .AnyAsync()
            .ConfigureAwait(false);
    }

    protected override bool RowBelongsToNewPackage(DatabricksSqlRow current, DatabricksSqlRow previous)
    {
        var amountType = _helper.GuessAmountTypeFromRow(current);
        var calculationIdColumn = _helper.GetCalculationIdColumnName(amountType);

        return _helper
                   .GetColumnsToAggregateBy(amountType)
                   .Any(column => current[column] != previous[column])
               || current[calculationIdColumn] != previous[calculationIdColumn];
    }

    protected override WholesaleServices CreatePackageFromRowData(
        DatabricksSqlRow rowData,
        List<WholesaleTimeSeriesPoint> timeSeriesPoints)
    {
        var amountType = _helper.GuessAmountTypeFromRow(rowData);

        return WholesaleServicesFactory.Create(rowData, amountType, timeSeriesPoints);
    }

    protected override WholesaleTimeSeriesPoint CreateTimeSeriesPoint(DatabricksSqlRow row)
    {
        return WholesaleTimeSeriesPointFactory.Create(row);
    }

    private async Task<List<CalculationTypeForGridArea>> GetCalculationTypeForGridAreasAsync(
        WholesaleServicesQueryParameters queryParameters)
    {
        var calculationTypeForGridAreaStatement =
            new CalculationTypeForGridAreasStatement(_deltaTableOptions.Value, _helper, queryParameters);

        var calculationTypeForGridAreas = await _databricksSqlWarehouseQueryExecutor
            .ExecuteStatementAsync(calculationTypeForGridAreaStatement, Format.JsonArray)
            .Select(d =>
            {
                var databricksSqlRow = new DatabricksSqlRow(d);
                return new CalculationTypeForGridArea(
                    databricksSqlRow[_helper.GetGridAreaCodeColumnName(queryParameters.AmountType)]!,
                    databricksSqlRow[_helper.GetCalculationTypeColumnName(queryParameters.AmountType)]!);
            })
            .ToListAsync()
            .ConfigureAwait(false);

        return calculationTypeForGridAreas
            .GroupBy(ctpga => ctpga.GridArea)
            .Select(g =>
            {
                if (queryParameters.CalculationType is not null)
                {
                    return new CalculationTypeForGridArea(g.Key, CalculationTypeMapper.ToDeltaTableValue(queryParameters.CalculationType.Value));
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

                return new CalculationTypeForGridArea(g.Key, DeltaTableCalculationType.WholesaleFixing);
            })
            .Where(ctpga => queryParameters.CalculationType is not null || ctpga.CalculationType != DeltaTableCalculationType.WholesaleFixing)
            .Distinct()
            .ToList();
    }

    private class CalculationTypeForGridAreasStatement(
        DeltaTableOptions deltaTableOptions,
        WholesaleServicesQueryStatementHelper helper,
        WholesaleServicesQueryParameters queryParameters)
        : DatabricksStatement
    {
        private readonly DeltaTableOptions _deltaTableOptions = deltaTableOptions;
        private readonly WholesaleServicesQueryStatementHelper _helper = helper;
        private readonly WholesaleServicesQueryParameters _queryParameters = queryParameters;

        protected override string GetSqlStatement()
        {
            var sql = $"""
                       SELECT {_helper.GetGridAreaCodeColumnName(_queryParameters.AmountType)}, {_helper.GetCalculationTypeColumnName(_queryParameters.AmountType)}
                       FROM {_helper.GetSource(_queryParameters.AmountType, _deltaTableOptions)} wrv
                       """;

            sql = _helper.AddWhereClauseToSqlExpression(sql, _queryParameters);

            sql += $"""
                    {"\n"}
                    GROUP BY {_helper.GetGridAreaCodeColumnName(_queryParameters.AmountType)}, {_helper.GetCalculationTypeColumnName(_queryParameters.AmountType)}
                    """;

            return sql;
        }
    }
}
