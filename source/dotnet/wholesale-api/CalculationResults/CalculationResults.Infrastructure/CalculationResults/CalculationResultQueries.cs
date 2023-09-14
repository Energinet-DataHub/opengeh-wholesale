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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
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
        await foreach (var p in GetInternalAsync(sql, batch.PeriodStart.ToInstant(), batch.PeriodEnd.ToInstant()))
            yield return p;
        _logger.LogDebug("Fetched all calculation results for batch {BatchId}", batchId);
    }

    public async IAsyncEnumerable<CalculationResult> GetAsync(CalculationResultQuery query)
    {
        var sqlStatement = CreateRequestSql(query);
        await foreach (var p in GetInternalAsync(sqlStatement, query.StartOfPeriod, query.EndOfPeriod))
            yield return p;
        _logger.LogDebug("Fetched all calculation results for sql statement {SqlStatement}", sqlStatement);
    }

    private async IAsyncEnumerable<CalculationResult> GetInternalAsync(string sql, Instant periodStart, Instant periodEnd)
    {
        var timeSeriesPoints = new List<TimeSeriesPoint>();
        SqlResultRow? currentRow = null;
        var resultCount = 0;

        await foreach (var nextRow in _sqlStatementClient.ExecuteAsync(sql).ConfigureAwait(false))
        {
            var timeSeriesPoint = CreateTimeSeriesPoint(nextRow);

            if (currentRow != null && BelongsToDifferentResults(currentRow, nextRow))
            {
                yield return CreateCalculationResult(currentRow, timeSeriesPoints, periodStart, periodEnd);
                resultCount++;
                timeSeriesPoints = new List<TimeSeriesPoint>();
            }

            timeSeriesPoints.Add(timeSeriesPoint);
            currentRow = nextRow;
        }

        if (currentRow != null)
        {
            yield return CreateCalculationResult(currentRow, timeSeriesPoints, periodStart, periodEnd);
            resultCount++;
        }

        _logger.LogDebug("Fetched {ResultCount} calculation results", resultCount);
    }

    private string CreateRequestSql(CalculationResultQuery query)
    {
        return $@"
    SELECT {string.Join(", ", SqlColumnNames)}
    FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME}
    WHERE {EnergyResultColumnNames.TimeSeriesType} = '{TimeSeriesTypeMapper.ToDeltaTableValue(query.TimeSeriesType)}'
    AND {EnergyResultColumnNames.AggregationLevel} = '{AggregationLevelMapper.ToDeltaTableValue(query.TimeSeriesType, query.EnergySupplierId, query.BalanceResponsibleId)}'
    AND {EnergyResultColumnNames.GridArea} = '{query.GridArea}'
    AND {EnergyResultColumnNames.Time} >= '{query.StartOfPeriod.ToString()}'
    AND {EnergyResultColumnNames.Time} <= '{query.EndOfPeriod.ToString()}'
    AND {query.EnergySupplierId == null || query.EnergySupplierId == EnergyResultColumnNames.EnergySupplierId}
    AND {query.BalanceResponsibleId == null || query.BalanceResponsibleId == EnergyResultColumnNames.BalanceResponsibleId}
    ORDER BY {EnergyResultColumnNames.CalculationResultId}, {EnergyResultColumnNames.Time}
    ";
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
        EnergyResultColumnNames.BatchProcessType,
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
        SqlResultRow sqlResultRow,
        List<TimeSeriesPoint> timeSeriesPoints,
        Instant periodStart,
        Instant periodEnd)
    {
        var id = SqlResultValueConverters.ToGuid(sqlResultRow[EnergyResultColumnNames.CalculationResultId]);
        var timeSeriesType = SqlResultValueConverters.ToTimeSeriesType(sqlResultRow[EnergyResultColumnNames.TimeSeriesType]);
        var energySupplierId = sqlResultRow[EnergyResultColumnNames.EnergySupplierId];
        var balanceResponsibleId = sqlResultRow[EnergyResultColumnNames.BalanceResponsibleId];
        var gridArea = sqlResultRow[EnergyResultColumnNames.GridArea];
        var fromGridArea = sqlResultRow[EnergyResultColumnNames.FromGridArea];
        var batchId = sqlResultRow[EnergyResultColumnNames.BatchId];
        var processType = sqlResultRow[EnergyResultColumnNames.BatchProcessType];

        return new CalculationResult(
            id,
            Guid.Parse(batchId),
            gridArea,
            timeSeriesType,
            energySupplierId,
            balanceResponsibleId,
            timeSeriesPoints.ToArray(),
            ProcessTypeMapper.FromDeltaTableValue(processType),
            periodStart,
            periodEnd,
            fromGridArea);
    }
}
