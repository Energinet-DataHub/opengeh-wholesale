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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.RequestCalculationResult;

public class RequestCalculationResultQueries : IRequestCalculationResultQueries
{
    private readonly ISqlStatementClient _sqlStatementClient;
    private readonly DeltaTableOptions _deltaTableOptions;
    private readonly ILogger<RequestCalculationResultQueries> _logger;

    public RequestCalculationResultQueries(
        ISqlStatementClient sqlStatementClient,
        IOptions<DeltaTableOptions> deltaTableOptions,
        ILogger<RequestCalculationResultQueries> logger)
    {
        _sqlStatementClient = sqlStatementClient;
        _deltaTableOptions = deltaTableOptions.Value;
        _logger = logger;
    }

    public async Task<EnergyResult?> GetAsync(CalculationResultQuery query)
    {
        var sqlStatement = CreateRequestSql(query);
        var timeSeriesPoints = new List<TimeSeriesPoint>();
        SqlResultRow? firstRow = null;
        var resultCount = 0;
        await foreach (var currentRow in _sqlStatementClient.ExecuteAsync(sqlStatement).ConfigureAwait(false))
        {
            if (firstRow is null)
                firstRow = currentRow;

            var timeSeriesPoint = CreateTimeSeriesPoint(currentRow);

            timeSeriesPoints.Add(timeSeriesPoint);
            resultCount++;
        }

        _logger.LogDebug("Fetched {ResultCount} calculation results", resultCount);
        if (firstRow is null) return null;

        return CreateEnergyResult(firstRow, timeSeriesPoints, query.StartOfPeriod,  query.EndOfPeriod);
    }

    private string CreateRequestSql(CalculationResultQuery query)
    {
        return $@"
            SELECT {string.Join(", ", SqlColumnNames.Select(columenName => $"t1.{columenName}"))}
            FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME} t1
            LEFT JOIN {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME} t2
                ON t1.{EnergyResultColumnNames.Time} = t2.{EnergyResultColumnNames.Time}
                    AND t1.{EnergyResultColumnNames.BatchExecutionTimeStart} < t2.{EnergyResultColumnNames.BatchExecutionTimeStart}
                    AND t1.{EnergyResultColumnNames.GridArea} = t2.{EnergyResultColumnNames.GridArea}
                    AND COALESCE(t1.{EnergyResultColumnNames.FromGridArea}, 'N/A') = COALESCE(t2.{EnergyResultColumnNames.FromGridArea}, 'N/A')
                    AND t1.{EnergyResultColumnNames.TimeSeriesType} = t2.{EnergyResultColumnNames.TimeSeriesType}
                    AND t1.{EnergyResultColumnNames.BatchProcessType} = t2.{EnergyResultColumnNames.BatchProcessType}
                    AND t1.{EnergyResultColumnNames.AggregationLevel} = t2.{EnergyResultColumnNames.AggregationLevel}
            WHERE t2.time IS NULL
                AND t1.{EnergyResultColumnNames.GridArea} IN ({query.GridArea})
                AND t1.{EnergyResultColumnNames.TimeSeriesType} IN ('{TimeSeriesTypeMapper.ToDeltaTableValue(query.TimeSeriesType)}')
                AND t1.{EnergyResultColumnNames.Time} BETWEEN '{query.StartOfPeriod.ToString()}' AND '{query.EndOfPeriod.ToString()}'
                AND t1.{EnergyResultColumnNames.AggregationLevel} = '{AggregationLevelMapper.ToDeltaTableValue(query.TimeSeriesType, null, null)}'
            ORDER BY t1.time
            ";
    }

    public static string[] SqlColumnNames { get; } =
    {
        EnergyResultColumnNames.BatchId, EnergyResultColumnNames.GridArea, EnergyResultColumnNames.FromGridArea,
        EnergyResultColumnNames.TimeSeriesType, EnergyResultColumnNames.EnergySupplierId,
        EnergyResultColumnNames.BalanceResponsibleId, EnergyResultColumnNames.Time,
        EnergyResultColumnNames.Quantity, EnergyResultColumnNames.QuantityQuality,
        EnergyResultColumnNames.CalculationResultId, EnergyResultColumnNames.BatchProcessType,
    };

    private static TimeSeriesPoint CreateTimeSeriesPoint(SqlResultRow row)
    {
        var time = SqlResultValueConverters.ToDateTimeOffset(row[EnergyResultColumnNames.Time])!.Value;
        var quantity = SqlResultValueConverters.ToDecimal(row[EnergyResultColumnNames.Quantity])!.Value;
        var quality = SqlResultValueConverters.ToQuantityQuality(row[EnergyResultColumnNames.QuantityQuality]);
        return new TimeSeriesPoint(time, quantity, quality);
    }

    private static EnergyResult CreateEnergyResult(
        SqlResultRow sqlResultRow,
        List<TimeSeriesPoint> timeSeriesPoints,
        Instant periodStart,
        Instant periodEnd)
    {
        var id = SqlResultValueConverters.ToGuid(sqlResultRow[EnergyResultColumnNames.CalculationResultId]);
        var timeSeriesType =
            SqlResultValueConverters.ToTimeSeriesType(sqlResultRow[EnergyResultColumnNames.TimeSeriesType]);
        var energySupplierId = sqlResultRow[EnergyResultColumnNames.EnergySupplierId];
        var balanceResponsibleId = sqlResultRow[EnergyResultColumnNames.BalanceResponsibleId];
        var gridArea = sqlResultRow[EnergyResultColumnNames.GridArea];
        var fromGridArea = sqlResultRow[EnergyResultColumnNames.FromGridArea];
        var batchId = sqlResultRow[EnergyResultColumnNames.BatchId];
        var processType = sqlResultRow[EnergyResultColumnNames.BatchProcessType];

        return new EnergyResult(
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
