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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

/// <summary>
/// Provides base logic for performing a query that retrieves sql rows based on a list of calculations
///     and groups the rows into packages, which each contains metadata and a list of time series points.
/// Used to retrieve and create WholesaleServices and AggregatedTimeSeriesData packages, which are
///     results from databricks that can span multiple calculations
/// </summary>
public abstract class PackageQueriesBase<TPackageResult, TTimeSeriesPoint>(DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor)
{
    protected abstract bool RowBelongsToNewPackage(DatabricksSqlRow current, DatabricksSqlRow previous);

    protected abstract TPackageResult CreatePackageFromRowData(
        DatabricksSqlRow row,
        List<TTimeSeriesPoint> timeSeriesPoints);

    protected abstract TTimeSeriesPoint CreateTimeSeriesPoint(DatabricksSqlRow row);

    /// <summary>
    /// Retrieves a stream of sql rows from the Databricks SQL Warehouse and groups the sql rows into packages
    /// which are streamed back as they are finished.
    /// Used to create WholesaleServices and AggregatedTimeSeriesData packages, which are
    /// results that can span multiple calculations
    /// </summary>
    protected async IAsyncEnumerable<TPackageResult> GetDataAsync(DatabricksStatement sqlStatement)
    {
        var timeSeriesPoints = new List<TTimeSeriesPoint>();
        DatabricksSqlRow? previous = null;

        await foreach (var databricksCurrentRow in databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(sqlStatement, Format.JsonArray).ConfigureAwait(false))
        {
            var current = new DatabricksSqlRow(databricksCurrentRow);

            // Yield a package created from previous data, if the current row belongs to a new package
            if (previous != null && RowBelongsToNewPackage(current, previous))
            {
                yield return CreatePackageFromRowData(previous, timeSeriesPoints);
                timeSeriesPoints = [];
            }

            timeSeriesPoints.Add(CreateTimeSeriesPoint(current));
            previous = current;
        }

        // Yield the last package
        if (previous != null)
            yield return CreatePackageFromRowData(previous, timeSeriesPoints);
    }
}
