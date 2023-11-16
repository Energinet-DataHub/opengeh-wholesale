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

    public async IAsyncEnumerable<AggregatedTimeSeries> GetLatestCorrectionAsync(AggregatedTimeSeriesQueryParameters parameters)
    {
        if (parameters.ProcessType != null)
        {
            throw new ArgumentException($"{nameof(parameters.ProcessType)} must be null, it will be overwritten.", nameof(parameters));
        }

        var processType = await GetProcessTypeOfLatestCorrectionAsync(parameters).ConfigureAwait(false);

        await foreach (var aggregatedTimeSeries in GetAsync(parameters with { ProcessType = processType }))
            yield return aggregatedTimeSeries;
    }

    public async IAsyncEnumerable<AggregatedTimeSeries> GetAsync(AggregatedTimeSeriesQueryParameters parameters)
    {
        var sqlStatement = new QueryAggregatedTimeSeriesStatement(parameters, _deltaTableOptions);
        await foreach (var aggregatedTimeSeries in GetInternalAsync(sqlStatement))
            yield return aggregatedTimeSeries;
    }

    public async Task<ProcessType> GetProcessTypeOfLatestCorrectionAsync(AggregatedTimeSeriesQueryParameters parameters)
    {
        var thirdCorrectionExists = await PerformCorrectionVersionExistsQueryAsync(
            parameters with { ProcessType = ProcessType.ThirdCorrectionSettlement }).ConfigureAwait(false);
        if (thirdCorrectionExists)
            return ProcessType.ThirdCorrectionSettlement;

        var secondCorrectionExists = await PerformCorrectionVersionExistsQueryAsync(
            parameters with { ProcessType = ProcessType.SecondCorrectionSettlement }).ConfigureAwait(false);

        if (secondCorrectionExists)
            return ProcessType.SecondCorrectionSettlement;

        return ProcessType.FirstCorrectionSettlement;
    }

    private async IAsyncEnumerable<AggregatedTimeSeries> GetInternalAsync(QueryAggregatedTimeSeriesStatement sqlStatement)
    {
        var timeSeriesPoints = new List<EnergyTimeSeriesPoint>();
        DatabricksSqlRow? previousRow = null;
        await foreach (var databricksCurrentRow in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(sqlStatement, Format.JsonArray).ConfigureAwait(false))
        {
            var currentRow = new DatabricksSqlRow(databricksCurrentRow);
            var timeSeriesPoint = EnergyTimeSeriesPointFactory.CreateTimeSeriesPoint(currentRow);

            if (previousRow != null && BelongsToDifferentGridArea(currentRow, previousRow))
            {
                yield return AggregatedTimeSeriesFactory.Create(previousRow, timeSeriesPoints);
                timeSeriesPoints = new List<EnergyTimeSeriesPoint>();
            }

            timeSeriesPoints.Add(timeSeriesPoint);
            previousRow = currentRow;
        }

        if (previousRow != null)
            yield return AggregatedTimeSeriesFactory.Create(previousRow, timeSeriesPoints);
    }

    private async Task<bool> PerformCorrectionVersionExistsQueryAsync(AggregatedTimeSeriesQueryParameters parameters)
    {
        return await _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(
            new QuerySingleAggregatedTimeSeriesStatement(parameters, _deltaTableOptions)).AnyAsync().ConfigureAwait(false);
    }

    private static bool BelongsToDifferentGridArea(DatabricksSqlRow row, DatabricksSqlRow otherRow)
    {
        return row[EnergyResultColumnNames.GridArea] != otherRow[EnergyResultColumnNames.GridArea];
    }
}
