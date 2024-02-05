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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class AggregatedTimeSeriesQueries : IAggregatedTimeSeriesQueries
{
    private readonly DatabricksSqlWarehouseQueryExecutor _databricksSqlWarehouseQueryExecutor;
    private readonly DeltaTableOptions _deltaTableOptions;

    public AggregatedTimeSeriesQueries(
        DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor,
        IOptions<DeltaTableOptions> deltaTableOptions)
    {
        _databricksSqlWarehouseQueryExecutor = databricksSqlWarehouseQueryExecutor;
        _deltaTableOptions = deltaTableOptions.Value;
    }

    public async IAsyncEnumerable<AggregatedTimeSeries> GetLatestCorrectionForGridAreaAsync(AggregatedTimeSeriesQueryParameters parameters)
    {
        if (parameters.ProcessType != null)
        {
            throw new ArgumentException($"{nameof(parameters.ProcessType)} must be null, it will be overwritten.", nameof(parameters));
        }

        if (parameters.GridArea == null)
        {
            throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters.GridArea)} may not be null.");
        }

        await foreach (var aggregatedTimeSeries in GetAggregatedTimeSeriesForLatestCorrectionAsync(parameters).ConfigureAwait(false))
            yield return aggregatedTimeSeries;
    }

    public async IAsyncEnumerable<AggregatedTimeSeries> GetAsync(AggregatedTimeSeriesQueryParameters parameters)
    {
        if (!parameters.LatestCalculationForPeriod.Any())
        {
            yield break;
        }

        var sqlStatement = new AggregatedTimeSeriesQueryStatement(parameters, _deltaTableOptions);
        await foreach (var aggregatedTimeSeries in GetInternalAsync(sqlStatement, parameters.LatestCalculationForPeriod).ConfigureAwait(false))
            yield return aggregatedTimeSeries;
    }

    private async IAsyncEnumerable<AggregatedTimeSeries> GetAggregatedTimeSeriesForLatestCorrectionAsync(AggregatedTimeSeriesQueryParameters parameters)
    {
        await foreach (var aggregatedTimeSeries in GetAsync(parameters with { ProcessType = ProcessType.ThirdCorrectionSettlement }).ConfigureAwait(false))
            yield return aggregatedTimeSeries;

        await foreach (var aggregatedTimeSeries in GetAsync(parameters with { ProcessType = ProcessType.SecondCorrectionSettlement }).ConfigureAwait(false))
            yield return aggregatedTimeSeries;

        await foreach (var aggregatedTimeSeries in GetAsync(parameters with { ProcessType = ProcessType.FirstCorrectionSettlement }).ConfigureAwait(false))
            yield return aggregatedTimeSeries;
    }

    private async IAsyncEnumerable<AggregatedTimeSeries> GetInternalAsync(
        AggregatedTimeSeriesQueryStatement sqlStatement,
        IReadOnlyCollection<CalculationForPeriod> latestCalculationForPeriod)
    {
        var timeSeriesPoints = new List<EnergyTimeSeriesPoint>();
        DatabricksSqlRow? previousRow = null;
        await foreach (var databricksCurrentRow in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(sqlStatement, Format.JsonArray).ConfigureAwait(false))
        {
            var currentRow = new DatabricksSqlRow(databricksCurrentRow);
            var timeSeriesPoint = EnergyTimeSeriesPointFactory.CreateTimeSeriesPoint(currentRow);

            if (previousRow != null && (BelongsToDifferentGridArea(currentRow, previousRow)
                                        || DifferentCalculationId(currentRow, previousRow)))
            {
                var version = GetVersion(previousRow, latestCalculationForPeriod);
                yield return AggregatedTimeSeriesFactory.Create(previousRow, timeSeriesPoints, version);
                timeSeriesPoints = new List<EnergyTimeSeriesPoint>();
            }

            timeSeriesPoints.Add(timeSeriesPoint);
            previousRow = currentRow;
        }

        if (previousRow != null)
        {
            var version = GetVersion(previousRow, latestCalculationForPeriod);
            yield return AggregatedTimeSeriesFactory.Create(previousRow, timeSeriesPoints, version);
        }
    }

    private long GetVersion(DatabricksSqlRow row, IReadOnlyCollection<CalculationForPeriod> latestCalculationForPeriod)
    {
        return latestCalculationForPeriod
            .First(x => x.BatchId == Guid.Parse(row[EnergyResultColumnNames.BatchId]!))
            .CalculationVersion;
    }

    private bool DifferentCalculationId(DatabricksSqlRow row, DatabricksSqlRow otherRow)
    {
        return row[EnergyResultColumnNames.BatchId] != otherRow[EnergyResultColumnNames.BatchId];
    }

    private static bool BelongsToDifferentGridArea(DatabricksSqlRow row, DatabricksSqlRow otherRow)
    {
        return row[EnergyResultColumnNames.GridArea] != otherRow[EnergyResultColumnNames.GridArea];
    }
}
