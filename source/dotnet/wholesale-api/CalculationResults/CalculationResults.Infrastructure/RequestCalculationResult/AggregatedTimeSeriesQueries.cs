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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.RequestCalculationResult.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.RequestCalculationResult;

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

    public async Task<AggregatedTimeSeriesResult?> GetAsync(AggregatedTimeSeriesQueryParameters parameters)
    {
        var timeSeriesPoints = new List<EnergyTimeSeriesPoint>();
        DatabricksSqlRow? firstRow = null;
        await foreach (var currentRow in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(new QueryAggregatedTimeSeriesStatement(parameters, _deltaTableOptions), Format.JsonArray).ConfigureAwait(false))
        {
            var databricksCurrentRow = new DatabricksSqlRow(currentRow);
            if (firstRow is null)
                firstRow = databricksCurrentRow;

            var timeSeriesPoint = EnergyTimeSeriesPointFactory.CreateTimeSeriesPoint(databricksCurrentRow);

            timeSeriesPoints.Add(timeSeriesPoint);
        }

        if (firstRow is null)
            return null;

        return AggregatedTimeSeriesResultFactory.CreateEnergyResult(firstRow, timeSeriesPoints);
    }

    public async Task<AggregatedTimeSeriesResult?> GetLatestCorrectionAsync(AggregatedTimeSeriesQueryParameters parameters)
    {
        if (parameters.ProcessType != null)
        {
            throw new ArgumentException($"{nameof(parameters.ProcessType)} must be null, it will be overwritten.", nameof(parameters));
        }

        var processType = await GetProcessTypeOfLatestCorrectionAsync(parameters).ConfigureAwait(false);

        return await GetAsync(
            parameters with { ProcessType = processType }).ConfigureAwait(false);
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

    private async Task<bool> PerformCorrectionVersionExistsQueryAsync(AggregatedTimeSeriesQueryParameters parameters)
    {
        return await _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(
            new QuerySingleAggregatedTimeSeriesStatement(parameters, _deltaTableOptions)).AnyAsync().ConfigureAwait(false);
    }
}
