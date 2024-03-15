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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class WholesaleServicesQueries(
    DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor,
    IOptions<DeltaTableOptions> deltaTableOptions)
    : PackageQueriesBase<WholesaleServices, WholesaleTimeSeriesPoint>(databricksSqlWarehouseQueryExecutor), IWholesaleServicesQueries
{
    private readonly DatabricksSqlWarehouseQueryExecutor _databricksSqlWarehouseQueryExecutor = databricksSqlWarehouseQueryExecutor;

    public async IAsyncEnumerable<WholesaleServices> GetAsync(WholesaleServicesQueryParameters queryParameters)
    {
        if (queryParameters.Calculations.Count == 0)
            yield break;

        var sqlStatement = new WholesaleServicesQueryStatement(
            WholesaleServicesQueryStatement.StatementType.Select,
            queryParameters,
            deltaTableOptions.Value);

        var resultStream = GetDataAsync(sqlStatement, queryParameters.Calculations);

        await foreach (var wholesaleServices in resultStream.ConfigureAwait(false))
            yield return wholesaleServices;
    }

    public Task<bool> AnyAsync(WholesaleServicesQueryParameters queryParameters)
    {
        var sqlStatement = new WholesaleServicesQueryStatement(
            WholesaleServicesQueryStatement.StatementType.Exists,
            queryParameters,
            deltaTableOptions.Value);

        return _databricksSqlWarehouseQueryExecutor
            .ExecuteStatementAsync(sqlStatement)
            .AnyAsync()
            .AsTask();
    }

    protected override string CalculationIdColumnName => WholesaleResultColumnNames.CalculationId;

    protected override string TimeColumnName => WholesaleResultColumnNames.Time;

    protected override bool RowBelongsToNewPackage(RowData current, RowData previous)
    {
        var isInDifferentCalculationPeriod = current.CalculationPeriod != previous.CalculationPeriod;

        return HasDifferentColumnValues(current.Row, previous.Row) || isInDifferentCalculationPeriod;
    }

    protected override WholesaleServices CreatePackageFromRowData(RowData rowData, List<WholesaleTimeSeriesPoint> timeSeriesPoints)
    {
        return WholesaleServicesFactory.Create(
            rowData.Row,
            rowData.CalculationPeriod.Period,
            timeSeriesPoints,
            rowData.CalculationPeriod.CalculationVersion);
    }

    protected override WholesaleTimeSeriesPoint CreateTimeSeriesPoint(DatabricksSqlRow row)
    {
        return WholesaleTimeSeriesPointFactory.Create(row);
    }

    private static bool HasDifferentColumnValues(DatabricksSqlRow row1, DatabricksSqlRow row2)
    {
        return WholesaleServicesQueryStatement.ColumnsToGroupBy.Any(column => row1[column] != row2[column]);
    }
}
