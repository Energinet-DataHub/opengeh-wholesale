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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Abstractions;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.EnergyResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Energinet.DataHub.Wholesale.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.RequestCalculationResult;

public class RequestCalculationResultQueries : IRequestCalculationResultQueries
{
    private readonly IDatabricksSqlStatementClient _sqlStatementClient;
    private readonly DeltaTableOptions _deltaTableOptions;
    private readonly ILogger<RequestCalculationResultQueries> _logger;

    public RequestCalculationResultQueries(
        IDatabricksSqlStatementClient sqlStatementClient,
        IOptions<DeltaTableOptions> deltaTableOptions,
        ILogger<RequestCalculationResultQueries> logger)
    {
        _sqlStatementClient = sqlStatementClient;
        _deltaTableOptions = deltaTableOptions.Value;
        _logger = logger;
    }

    public async IAsyncEnumerable<EnergyResult> GetAsync(EnergyResultQuery query)
    {
        var sqlStatement = CreateRequestSql(query);

        await foreach (var calculationResult in GetInternalAsync(sqlStatement, query.StartOfPeriod, query.EndOfPeriod))
            yield return calculationResult;
    }

    public async Task<ProcessType> GetLatestCorrectionAsync(IEnergyResultFilter query)
    {
        var thirdCorrectionExists = await PerformCorrectionVersionExistsQueryAsync(query, ProcessType.ThirdCorrectionSettlement).ConfigureAwait(false);
        if (thirdCorrectionExists)
            return ProcessType.ThirdCorrectionSettlement;

        var secondCorrectionExists = await PerformCorrectionVersionExistsQueryAsync(query, ProcessType.SecondCorrectionSettlement).ConfigureAwait(false);
        if (secondCorrectionExists)
            return ProcessType.SecondCorrectionSettlement;

        return ProcessType.FirstCorrectionSettlement;
    }

    private async IAsyncEnumerable<EnergyResult> GetInternalAsync(string sql, Instant periodStart, Instant periodEnd)
    {
        var timeSeriesPoints = new List<EnergyTimeSeriesPoint>();
        SqlResultRow? currentRow = null;
        var resultCount = 0;
        await foreach (var nextRow in _sqlStatementClient.ExecuteAsync(sql, sqlStatementParameters: null)
                           .ConfigureAwait(false))
        {
            var timeSeriesPoint = EnergyTimeSeriesPointFactory.CreateTimeSeriesPoint(nextRow);
            if (currentRow != null && BelongsToDifferentGridArea(currentRow, nextRow))
            {
                yield return EnergyResultFactory.CreateEnergyResult(currentRow, timeSeriesPoints, periodStart, periodEnd);
                resultCount++;
                timeSeriesPoints = new List<EnergyTimeSeriesPoint>();
            }

            timeSeriesPoints.Add(timeSeriesPoint);
            currentRow = nextRow;
        }

        if (currentRow != null)
        {
            yield return EnergyResultFactory.CreateEnergyResult(currentRow, timeSeriesPoints, periodStart, periodEnd);
            resultCount++;
        }

        _logger.LogDebug("Fetched {ResultCount} calculation results", resultCount);
    }

    private Task<bool> PerformCorrectionVersionExistsQueryAsync(IEnergyResultFilter queryFilter, ProcessType processType)
    {
        var sql = CreateSelectCorrectionVersionSql(queryFilter, processType);

        var tableRows = _sqlStatementClient.ExecuteAsync(sql, sqlStatementParameters: null);

        return tableRows.AnyAsync().AsTask();
    }

    private string CreateSelectCorrectionVersionSql(IEnergyResultFilter query, ProcessType processType)
    {
        if (processType != ProcessType.FirstCorrectionSettlement
            && processType != ProcessType.SecondCorrectionSettlement
            && processType != ProcessType.ThirdCorrectionSettlement)
            throw new ArgumentOutOfRangeException(nameof(processType), processType, "ProcessType must be a correction settlement type");

        var sql = $@"
            SELECT 1
            FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME} t1
            WHERE {CreateSqlQueryFilters(query, processType)}";

        return sql;
    }

    private string CreateRequestSql(EnergyResultQuery query)
    {
        var sql = $@"
            SELECT {string.Join(", ", SqlColumnNames.Select(columnName => $"t1.{columnName}"))}
            FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME} t1
            LEFT JOIN {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME} t2
                ON t1.{EnergyResultColumnNames.Time} = t2.{EnergyResultColumnNames.Time}
                    AND t1.{EnergyResultColumnNames.BatchExecutionTimeStart} < t2.{EnergyResultColumnNames.BatchExecutionTimeStart}
                    AND t1.{EnergyResultColumnNames.GridArea} = t2.{EnergyResultColumnNames.GridArea}
                    AND COALESCE(t1.{EnergyResultColumnNames.FromGridArea}, 'N/A') = COALESCE(t2.{EnergyResultColumnNames.FromGridArea}, 'N/A')
                    AND t1.{EnergyResultColumnNames.TimeSeriesType} = t2.{EnergyResultColumnNames.TimeSeriesType}
                    AND t1.{EnergyResultColumnNames.BatchProcessType} = t2.{EnergyResultColumnNames.BatchProcessType}
                    AND t1.{EnergyResultColumnNames.AggregationLevel} = t2.{EnergyResultColumnNames.AggregationLevel}
            WHERE t2.{EnergyResultColumnNames.Time} IS NULL
                AND {CreateSqlQueryFilters(query, query.ProcessType)}";

        sql += $@"ORDER BY t1.{EnergyResultColumnNames.GridArea}, t1.time";
        return sql;
    }

    private string CreateSqlQueryFilters(IEnergyResultFilter query, ProcessType processType)
    {
        var whereClausesSql = $@"t1.{EnergyResultColumnNames.TimeSeriesType} IN ('{TimeSeriesTypeMapper.ToDeltaTableValue(query.TimeSeriesType)}')
            AND t1.{EnergyResultColumnNames.Time} >= '{query.StartOfPeriod.ToString()}'
            AND t1.{EnergyResultColumnNames.Time} < '{query.EndOfPeriod.ToString()}'
            AND t1.{EnergyResultColumnNames.AggregationLevel} = '{AggregationLevelMapper.ToDeltaTableValue(query.TimeSeriesType, query.EnergySupplierId, query.BalanceResponsibleId)}'
            AND t1.{EnergyResultColumnNames.BatchProcessType} = '{ProcessTypeMapper.ToDeltaTableValue(processType)}'
            ";

        if (query.EnergySupplierId != null)
        {
            whereClausesSql += $@"AND t1.{EnergyResultColumnNames.EnergySupplierId} = '{query.EnergySupplierId}'";
        }

        if (query.BalanceResponsibleId != null)
        {
            whereClausesSql += $@"AND t1.{EnergyResultColumnNames.BalanceResponsibleId} = '{query.BalanceResponsibleId}'";
        }

        if (query.GridArea != null)
        {
            whereClausesSql += $@"AND t1.{EnergyResultColumnNames.GridArea} = ({query.GridArea})";
        }

        return whereClausesSql;
    }

    private static bool BelongsToDifferentGridArea(SqlResultRow row, SqlResultRow otherRow)
    {
        return row[EnergyResultColumnNames.GridArea] != otherRow[EnergyResultColumnNames.GridArea];
    }

    private static string[] SqlColumnNames { get; } =
    {
        EnergyResultColumnNames.BatchId, EnergyResultColumnNames.GridArea, EnergyResultColumnNames.FromGridArea,
        EnergyResultColumnNames.TimeSeriesType, EnergyResultColumnNames.EnergySupplierId,
        EnergyResultColumnNames.BalanceResponsibleId, EnergyResultColumnNames.Time,
        EnergyResultColumnNames.Quantity, EnergyResultColumnNames.QuantityQualities,
        EnergyResultColumnNames.CalculationResultId, EnergyResultColumnNames.BatchProcessType,
    };
}
