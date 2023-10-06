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
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Models;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

    public async Task<EnergyResult?> GetAsync(EnergyResultQuery query)
    {
        var sqlStatement = CreateRequestSql(query);
        var timeSeriesPoints = new List<EnergyTimeSeriesPoint>();
        SqlResultRow? firstRow = null;
        var resultCount = 0;
        await foreach (var currentRow in _sqlStatementClient.ExecuteAsync(sqlStatement, sqlStatementParameters: null).ConfigureAwait(false))
        {
            if (firstRow is null)
                firstRow = currentRow;

            var timeSeriesPoint = TimeSeriesPointFactory.CreateTimeSeriesPoint(currentRow);

            timeSeriesPoints.Add(timeSeriesPoint);
            resultCount++;
        }

        _logger.LogDebug("Fetched {ResultCount} calculation results", resultCount);
        if (firstRow is null)
            return null;

        return EnergyResultFactory.CreateEnergyResult(firstRow, timeSeriesPoints, query.StartOfPeriod, query.EndOfPeriod);
    }

    private string CreateRequestSql(EnergyResultQuery query)
    {
        var sql = $@"
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
                AND t1.{EnergyResultColumnNames.AggregationLevel} = '{AggregationLevelMapper.ToDeltaTableValue(query.TimeSeriesType, query.EnergySupplierId, query.BalanceResponsibleId)}'
            ";
        if (query.EnergySupplierId != null)
        {
            sql += $@"AND t1.{EnergyResultColumnNames.EnergySupplierId} = '{query.EnergySupplierId}'";
        }

        if (query.BalanceResponsibleId != null)
        {
            sql += $@"AND t1.{EnergyResultColumnNames.BalanceResponsibleId} = '{query.BalanceResponsibleId}'";
        }

        sql += $@"ORDER BY t1.time";
        return sql;
    }

    private static string[] SqlColumnNames { get; } =
    {
        EnergyResultColumnNames.BatchId, EnergyResultColumnNames.GridArea, EnergyResultColumnNames.FromGridArea,
        EnergyResultColumnNames.TimeSeriesType, EnergyResultColumnNames.EnergySupplierId,
        EnergyResultColumnNames.BalanceResponsibleId, EnergyResultColumnNames.Time,
        EnergyResultColumnNames.Quantity, EnergyResultColumnNames.QuantityQuality,
        EnergyResultColumnNames.CalculationResultId, EnergyResultColumnNames.BatchProcessType,
    };
}
