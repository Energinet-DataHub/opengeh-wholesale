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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.RequestCalculationResult.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.RequestCalculationResult;

public class RequestCalculationResultQueries : IRequestCalculationResultQueries
{
    private readonly DatabricksSqlWarehouseQueryExecutor _databricksSqlWarehouseQueryExecutor;
    private readonly DeltaTableOptions _deltaTableOptions;
    private readonly ILogger<RequestCalculationResultQueries> _logger;

    public RequestCalculationResultQueries(
        DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor,
        IOptions<DeltaTableOptions> deltaTableOptions,
        ILogger<RequestCalculationResultQueries> logger)
    {
        _databricksSqlWarehouseQueryExecutor = databricksSqlWarehouseQueryExecutor;
        _deltaTableOptions = deltaTableOptions.Value;
        _logger = logger;
    }

    public async Task<EnergyResult?> GetAsync(EnergyResultQuery query)
    {
        var statement = new QueryCalculationResultsStatement2(_deltaTableOptions, query);
        var timeSeriesPoints = new List<EnergyTimeSeriesPoint>();
        SqlResultRow? firstRow = null;
        var resultCount = 0;
        await foreach (var currentRow in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement).ConfigureAwait(false))
        {
            if (firstRow is null)
                firstRow = currentRow;

            var timeSeriesPoint = EnergyTimeSeriesPointFactory.CreateTimeSeriesPoint(currentRow);

            timeSeriesPoints.Add(timeSeriesPoint);
            resultCount++;
        }

        _logger.LogDebug("Fetched {ResultCount} calculation results", resultCount);
        if (firstRow is null)
            return null;

        return EnergyResultFactory.CreateEnergyResult(firstRow, timeSeriesPoints, query.StartOfPeriod, query.EndOfPeriod);
    }
}
