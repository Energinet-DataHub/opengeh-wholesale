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

using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class CalculationResultQueries : ICalculationResultQueries
{
    private readonly ISqlStatementClient _sqlStatementClient;
    private readonly IBatchesClient _batchesClient;
    private readonly DeltaTableOptions _deltaTableOptions;
    private readonly ILogger<CalculationResultQueries> _logger;

    public CalculationResultQueries(ISqlStatementClient sqlStatementClient, IBatchesClient batchesClient, IOptions<DeltaTableOptions> deltaTableOptions, ILogger<CalculationResultQueries> logger)
    {
        _sqlStatementClient = sqlStatementClient;
        _batchesClient = batchesClient;
        _deltaTableOptions = deltaTableOptions.Value;
        _logger = logger;
    }

    public async IAsyncEnumerable<CalculationResult> GetAsync(Guid batchId)
    {
        _logger.LogError("Entering GetAsync method with batchId: {batchId}", batchId);
        var batch = await _batchesClient.GetAsync(batchId).ConfigureAwait(false);
        _logger.LogError("Retrieved batch with id: {batchId}", batchId);
        var sql = CreateBatchResultsSql(batchId);
        _logger.LogError("Created SQL statement: {sql}", sql);
        var timeSeriesPoints = new List<TimeSeriesPoint>();
        SqlResultRow? currentRow = null;

        await foreach (var nextRow in _sqlStatementClient.ExecuteAsync(sql).ConfigureAwait(false))
        {
            _logger.LogError("Processing row: {row}", nextRow);
            var timeSeriesPoint = CreateTimeSeriesPoint(nextRow);
            _logger.LogError("Created time series point: {timeSeriesPoint}", timeSeriesPoint);

            if (currentRow != null && BelongsToDifferentResults(currentRow, nextRow))
            {
                _logger.LogError("Current row belongs to different results, creating calculation result");
                yield return CreateCalculationResult(batch, currentRow, timeSeriesPoints);
                timeSeriesPoints = new List<TimeSeriesPoint>();
            }

            timeSeriesPoints.Add(timeSeriesPoint);
            currentRow = nextRow;
        }

        _logger.LogError("Reached end of rows, creating final calculation result");
        if (currentRow != null)
        {
            _logger.LogError("Reached end of rows, creating final calculation result");
            yield return CreateCalculationResult(batch, currentRow, timeSeriesPoints);
        }
    }

    private string CreateBatchResultsSql(Guid batchId)
    {
        return $@"
SELECT {string.Join(", ", SqlColumnNames)}
FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.RESULT_TABLE_NAME}
WHERE {ResultColumnNames.BatchId} = '{batchId}'
ORDER BY {ResultColumnNames.CalculationResultId}, {ResultColumnNames.Time}
";
    }

    public static string[] SqlColumnNames { get; } =
    {
        ResultColumnNames.BatchId,
        ResultColumnNames.GridArea,
        ResultColumnNames.FromGridArea,
        ResultColumnNames.TimeSeriesType,
        ResultColumnNames.EnergySupplierId,
        ResultColumnNames.BalanceResponsibleId,
        ResultColumnNames.Time,
        ResultColumnNames.Quantity,
        ResultColumnNames.QuantityQuality,
        ResultColumnNames.CalculationResultId,
    };

    public static bool BelongsToDifferentResults(SqlResultRow row, SqlResultRow otherRow)
    {
        return row[ResultColumnNames.CalculationResultId] != otherRow[ResultColumnNames.CalculationResultId];
    }

    private static TimeSeriesPoint CreateTimeSeriesPoint(SqlResultRow row)
    {
        var time = SqlResultValueConverters.ToDateTimeOffset(row[ResultColumnNames.Time])!.Value;
        var quantity = SqlResultValueConverters.ToDecimal(row[ResultColumnNames.Quantity])!.Value;
        var quality = SqlResultValueConverters.ToQuantityQuality(row[ResultColumnNames.QuantityQuality]);
        return new TimeSeriesPoint(time, quantity, quality);
    }

    private static CalculationResult CreateCalculationResult(
        BatchDto batch,
        SqlResultRow sqlResultRow,
        List<TimeSeriesPoint> timeSeriesPoints)
    {
        var id = SqlResultValueConverters.ToGuid(sqlResultRow[ResultColumnNames.CalculationResultId]);
        var timeSeriesType = SqlResultValueConverters.ToTimeSeriesType(sqlResultRow[ResultColumnNames.TimeSeriesType]);
        var energySupplierId = sqlResultRow[ResultColumnNames.EnergySupplierId];
        var balanceResponsibleId = sqlResultRow[ResultColumnNames.BalanceResponsibleId];
        var gridArea = sqlResultRow[ResultColumnNames.GridArea];
        var fromGridArea = sqlResultRow[ResultColumnNames.FromGridArea];
        return new CalculationResult(
            id,
            batch.BatchId,
            gridArea,
            timeSeriesType,
            energySupplierId,
            balanceResponsibleId,
            timeSeriesPoints.ToArray(),
            batch.ProcessType,
            batch.PeriodStart.ToInstant(),
            batch.PeriodEnd.ToInstant(),
            fromGridArea);
    }
}
