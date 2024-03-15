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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class AggregatedTimeSeriesQueries(
    DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor,
    IOptions<DeltaTableOptions> deltaTableOptions)
    : PackageQueriesBase<AggregatedTimeSeries, EnergyTimeSeriesPoint>(databricksSqlWarehouseQueryExecutor), IAggregatedTimeSeriesQueries
{
    public async IAsyncEnumerable<AggregatedTimeSeries> GetAsync(AggregatedTimeSeriesQueryParameters parameters)
    {
        if (parameters.LatestCalculationForPeriod.Count == 0)
            yield break;

        var sqlStatement = new AggregatedTimeSeriesQueryStatement(parameters, deltaTableOptions.Value);
        await foreach (var aggregatedTimeSeries in GetDataAsync(sqlStatement, parameters.LatestCalculationForPeriod).ConfigureAwait(false))
            yield return aggregatedTimeSeries;
    }

    protected override string CalculationIdColumnName => EnergyResultColumnNames.CalculationId;

    protected override string TimeColumnName => EnergyResultColumnNames.Time;

    protected override AggregatedTimeSeries CreatePackageFromRowData(RowData rowData, List<EnergyTimeSeriesPoint> timeSeriesPoints)
    {
        return AggregatedTimeSeriesFactory.Create(
            rowData.Row,
            rowData.CalculationPeriod.Period.Start,
            rowData.CalculationPeriod.Period.End,
            timeSeriesPoints,
            rowData.CalculationPeriod.CalculationVersion);
    }

    protected override EnergyTimeSeriesPoint CreateTimeSeriesPoint(DatabricksSqlRow row)
    {
        return EnergyTimeSeriesPointFactory.CreateTimeSeriesPoint(row);
    }

    protected override bool RowBelongsToNewPackage(RowData current, RowData previous)
    {
        var isInDifferentCalculationPeriod = current.CalculationPeriod != previous.CalculationPeriod;
        return HasDifferentColumnValues(current.Row, previous.Row) || isInDifferentCalculationPeriod;
    }

    private bool HasDifferentColumnValues(DatabricksSqlRow row1, DatabricksSqlRow row2)
    {
        return AggregatedTimeSeriesQueryStatement.ColumnsToGroupBy.Any(column => row1[column] != row2[column]);
    }
}
