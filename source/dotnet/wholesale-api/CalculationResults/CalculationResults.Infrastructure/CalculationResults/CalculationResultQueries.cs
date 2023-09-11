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
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Models;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Energinet.DataHub.Wholesale.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
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
        var batch = await _batchesClient.GetAsync(batchId).ConfigureAwait(false);
        var sql = CreateBatchResultsSql(batchId);
        var timeSeriesPoints = new List<TimeSeriesPoint>();
        SqlResultRow? currentRow = null;
        var resultCount = 0;

        await foreach (var nextRow in _sqlStatementClient.ExecuteAsync(sql).ConfigureAwait(false))
        {
            var timeSeriesPoint = CreateTimeSeriesPoint(nextRow);

            if (currentRow != null && BelongsToDifferentResults(currentRow, nextRow))
            {
                yield return CreateCalculationResult(batch, currentRow, timeSeriesPoints);
                resultCount++;
                timeSeriesPoints = new List<TimeSeriesPoint>();
            }

            timeSeriesPoints.Add(timeSeriesPoint);
            currentRow = nextRow;
        }

        if (currentRow != null)
        {
            yield return CreateCalculationResult(batch, currentRow, timeSeriesPoints);
            resultCount++;
        }

        _logger.LogDebug("Fetched all {ResultCount} results for batch {BatchId}", resultCount, batchId);
    }

    public async IAsyncEnumerable<CalculationResult> GetAsync(CalculationResultQuery query)
    {
        var sql = CreateRequestSql(query);
        var timeSeriesPoints = new List<TimeSeriesPoint>();
        SqlResultRow? currentRow = null;
        var resultCount = 0;

        await foreach (var nextRow in _sqlStatementClient.ExecuteAsync(sql).ConfigureAwait(false))
        {
            var timeSeriesPoint = CreateTimeSeriesPoint(nextRow);

            if (currentRow != null && BelongsToDifferentResults(currentRow, nextRow))
            {
                yield return CreateCalculationResult(query, currentRow, timeSeriesPoints);
                resultCount++;
                timeSeriesPoints = new List<TimeSeriesPoint>();
            }

            timeSeriesPoints.Add(timeSeriesPoint);
            currentRow = nextRow;
        }

        if (currentRow != null)
        {
            yield return CreateCalculationResult(query, currentRow, timeSeriesPoints);
            resultCount++;
        }

        _logger.LogDebug("Fetched all {ResultCount} results", resultCount);
    }

    private string CreateRequestSql(CalculationResultQuery query)
    {
        var i = query.StartOfPeriod.ToDateTimeUtc();
        return $@"
    SELECT {string.Join(", ", SqlColumnNames)}
    FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME}
    WHERE {EnergyResultColumnNames.TimeSeriesType} = '{query.TimeSeriesType.ToLower()}'
    AND {EnergyResultColumnNames.AggregationLevel} = '{MapAggregationLevel(query.AggregationLevel)}'
    AND {EnergyResultColumnNames.GridArea} = '{query.GridArea}'
    AND {EnergyResultColumnNames.Time} >= '{query.StartOfPeriod.ToString()}'
    AND {EnergyResultColumnNames.Time} <= '{query.EndOfPeriod.ToString()}'
    ORDER BY {EnergyResultColumnNames.CalculationResultId}, {EnergyResultColumnNames.Time}
    ";
    }

    private string MapAggregationLevel(AggregationLevel queryAggregationLevel)
    {
        if (queryAggregationLevel == AggregationLevel.GridArea)
            return DeltaTableAggregationLevel.GridArea;
        throw new InvalidOperationException($"Unsupported aggregation level: {queryAggregationLevel}");
    }

    private string CreateBatchResultsSql(Guid batchId)
    {
        return $@"
SELECT {string.Join(", ", SqlColumnNames)}
FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME}
WHERE {EnergyResultColumnNames.BatchId} = '{batchId}'
ORDER BY {EnergyResultColumnNames.CalculationResultId}, {EnergyResultColumnNames.Time}
";
    }

    public static string[] SqlColumnNames { get; } =
    {
        EnergyResultColumnNames.BatchId,
        EnergyResultColumnNames.GridArea,
        EnergyResultColumnNames.FromGridArea,
        EnergyResultColumnNames.TimeSeriesType,
        EnergyResultColumnNames.EnergySupplierId,
        EnergyResultColumnNames.BalanceResponsibleId,
        EnergyResultColumnNames.Time,
        EnergyResultColumnNames.Quantity,
        EnergyResultColumnNames.QuantityQuality,
        EnergyResultColumnNames.CalculationResultId,
    };

    public static bool BelongsToDifferentResults(SqlResultRow row, SqlResultRow otherRow)
    {
        return row[EnergyResultColumnNames.CalculationResultId] != otherRow[EnergyResultColumnNames.CalculationResultId];
    }

    private static TimeSeriesPoint CreateTimeSeriesPoint(SqlResultRow row)
    {
        var time = SqlResultValueConverters.ToDateTimeOffset(row[EnergyResultColumnNames.Time])!.Value;
        var quantity = SqlResultValueConverters.ToDecimal(row[EnergyResultColumnNames.Quantity])!.Value;
        var quality = SqlResultValueConverters.ToQuantityQuality(row[EnergyResultColumnNames.QuantityQuality]);
        return new TimeSeriesPoint(time, quantity, quality);
    }

    private static CalculationResult CreateCalculationResult(
        BatchDto batch,
        SqlResultRow sqlResultRow,
        List<TimeSeriesPoint> timeSeriesPoints)
    {
        var id = SqlResultValueConverters.ToGuid(sqlResultRow[EnergyResultColumnNames.CalculationResultId]);
        var timeSeriesType = SqlResultValueConverters.ToTimeSeriesType(sqlResultRow[EnergyResultColumnNames.TimeSeriesType]);
        var energySupplierId = sqlResultRow[EnergyResultColumnNames.EnergySupplierId];
        var balanceResponsibleId = sqlResultRow[EnergyResultColumnNames.BalanceResponsibleId];
        var gridArea = sqlResultRow[EnergyResultColumnNames.GridArea];
        var fromGridArea = sqlResultRow[EnergyResultColumnNames.FromGridArea];
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

    private static CalculationResult CreateCalculationResult(
        object request,
        SqlResultRow sqlResultRow,
        List<TimeSeriesPoint> timeSeriesPoints)
    {
        var id = SqlResultValueConverters.ToGuid(sqlResultRow[EnergyResultColumnNames.CalculationResultId]);
        var timeSeriesType = SqlResultValueConverters.ToTimeSeriesType(sqlResultRow[EnergyResultColumnNames.TimeSeriesType]);
        var energySupplierId = sqlResultRow[EnergyResultColumnNames.EnergySupplierId];
        var balanceResponsibleId = sqlResultRow[EnergyResultColumnNames.BalanceResponsibleId];
        var gridArea = sqlResultRow[EnergyResultColumnNames.GridArea];
        var fromGridArea = sqlResultRow[EnergyResultColumnNames.FromGridArea];
        var batchId = sqlResultRow[EnergyResultColumnNames.BatchId];
        return new CalculationResult(
            id,
            Guid.Parse(batchId),
            gridArea,
            timeSeriesType,
            energySupplierId,
            balanceResponsibleId,
            timeSeriesPoints.ToArray(),
            ProcessType.BalanceFixing,
            Instant.FromUtc(2000, 1, 1, 1, 1),
            Instant.FromUtc(2000, 1, 10, 1, 1),
            fromGridArea);
    }
}
