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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class WholesaleResultQueries : IWholesaleResultQueries
{
    private readonly DatabricksSqlWarehouseQueryExecutor _databricksSqlWarehouseQueryExecutor;
    private readonly ICalculationsClient _calculationsClient;
    private readonly DeltaTableOptions _deltaTableOptions;
    private readonly ILogger<WholesaleResultQueries> _logger;

    public WholesaleResultQueries(DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor, ICalculationsClient calculationsClient, IOptions<DeltaTableOptions> deltaTableOptions, ILogger<WholesaleResultQueries> logger)
    {
        _databricksSqlWarehouseQueryExecutor = databricksSqlWarehouseQueryExecutor;
        _calculationsClient = calculationsClient;
        _deltaTableOptions = deltaTableOptions.Value;
        _logger = logger;
    }

    public async IAsyncEnumerable<WholesaleResult> GetAsync(Guid calculationId)
    {
        var calculation = await _calculationsClient.GetAsync(calculationId).ConfigureAwait(false);
        var statement = new WholesaleResultQueryStatement(calculationId, _deltaTableOptions);
        await foreach (var calculationResult in GetInternalAsync(statement, calculation.PeriodStart.ToInstant(), calculation.PeriodEnd.ToInstant(), calculation.Version).ConfigureAwait(false))
            yield return calculationResult;
        _logger.LogDebug("Fetched all wholesale calculation results for calculation {calculation_id}", calculationId);
    }

    public static bool BelongsToDifferentResults(DatabricksSqlRow row, DatabricksSqlRow otherSqlRow)
    {
        return !row[WholesaleResultColumnNames.CalculationResultId]!.Equals(otherSqlRow[WholesaleResultColumnNames.CalculationResultId]);
    }

    private async IAsyncEnumerable<WholesaleResult> GetInternalAsync(DatabricksStatement statement, Instant periodStart, Instant periodEnd, long version)
    {
        var timeSeriesPoints = new List<WholesaleTimeSeriesPoint>();
        DatabricksSqlRow? currentRow = null;
        var resultCount = 0;

        await foreach (var nextRow in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement, Format.JsonArray).ConfigureAwait(false))
        {
            var convertedNextRow = new DatabricksSqlRow(nextRow);
            var timeSeriesPoint = WholesaleTimeSeriesPointFactory.Create(convertedNextRow);

            if (currentRow != null && BelongsToDifferentResults(currentRow, convertedNextRow))
            {
                yield return WholesaleResultFactory.CreateWholesaleResult(currentRow, timeSeriesPoints, periodStart, periodEnd, version);
                resultCount++;
#pragma warning disable SA1010 // Opening square brackets should be spaced correctly
                timeSeriesPoints = [];
#pragma warning restore SA1010 // Opening square brackets should be spaced correctly
            }

            timeSeriesPoints.Add(timeSeriesPoint);
            currentRow = convertedNextRow;
        }

        if (currentRow != null)
        {
            yield return WholesaleResultFactory.CreateWholesaleResult(currentRow, timeSeriesPoints, periodStart, periodEnd, version);
            resultCount++;
        }

        _logger.LogDebug("Fetched {result_count} calculation results", resultCount);
    }
}
