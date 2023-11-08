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
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.RequestCalculationResult;

public class AggregatedTimeSeriesQueries : IAggregatedTimeSeriesQueries
{
    private readonly IDatabricksSqlStatementClient _sqlStatementClient;
    private readonly DeltaTableOptions _deltaTableOptions;

    public AggregatedTimeSeriesQueries(
        IDatabricksSqlStatementClient sqlStatementClient,
        IOptions<DeltaTableOptions> deltaTableOptions)
    {
        _sqlStatementClient = sqlStatementClient;
        _deltaTableOptions = deltaTableOptions.Value;
    }

    public async Task<EnergyResult?> GetAsync(EnergyResultQueryParameters parameters)
    {
        var sqlStatement = CreateRequestSql(parameters);

        var timeSeriesPoints = new List<EnergyTimeSeriesPoint>();
        SqlResultRow? firstRow = null;
        await foreach (var currentRow in _sqlStatementClient.ExecuteAsync(sqlStatement, sqlStatementParameters: null).ConfigureAwait(false))
        {
            if (firstRow is null)
                firstRow = currentRow;

            var timeSeriesPoint = EnergyTimeSeriesPointFactory.CreateTimeSeriesPoint(currentRow);

            timeSeriesPoints.Add(timeSeriesPoint);
        }

        if (firstRow is null)
            return null;

        return EnergyResultFactory.CreateEnergyResult(firstRow, timeSeriesPoints, parameters.StartOfPeriod, parameters.EndOfPeriod);
    }

    public async Task<EnergyResult?> GetLatestCorrectionAsync(EnergyResultQueryParameters parameters)
    {
        if (parameters.ProcessType != null)
        {
            throw new ArgumentException("ProcessType must be null, since it will be overwritten", nameof(parameters));
        }

        var processType = await GetProcessTypeOfLatestCorrectionAsync(parameters).ConfigureAwait(false);

        var queryWithLatestCorrection = new EnergyResultQueryParameters(parameters, processType);

        return await GetAsync(queryWithLatestCorrection).ConfigureAwait(false);
    }

    public async Task<ProcessType> GetProcessTypeOfLatestCorrectionAsync(EnergyResultQueryParameters parameters)
    {
        var thirdCorrectionExists = await PerformCorrectionVersionExistsQueryAsync(
            new EnergyResultQueryParameters(
                parameters,
                ProcessType.ThirdCorrectionSettlement)).ConfigureAwait(false);
        if (thirdCorrectionExists)
            return ProcessType.ThirdCorrectionSettlement;

        var secondCorrectionExists = await PerformCorrectionVersionExistsQueryAsync(
            new EnergyResultQueryParameters(
                parameters,
                ProcessType.SecondCorrectionSettlement)).ConfigureAwait(false);

        if (secondCorrectionExists)
            return ProcessType.SecondCorrectionSettlement;

        return ProcessType.FirstCorrectionSettlement;
    }

    private Task<bool> PerformCorrectionVersionExistsQueryAsync(EnergyResultQueryParameters parameters)
    {
        var sql = CreateSelectCorrectionVersionSql(parameters);

        var tableRows = _sqlStatementClient.ExecuteAsync(sql, sqlStatementParameters: null);

        return tableRows.AnyAsync().AsTask();
    }

    private string CreateSelectCorrectionVersionSql(EnergyResultQueryParameters parameters)
    {
        var processType = parameters.ProcessType;

        if (processType != ProcessType.FirstCorrectionSettlement
            && processType != ProcessType.SecondCorrectionSettlement
            && processType != ProcessType.ThirdCorrectionSettlement)
            throw new ArgumentOutOfRangeException(nameof(processType), processType, "ProcessType must be a correction settlement type");

        var sql = $@"
            SELECT 1
            FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME} t1
            WHERE {CreateSqlQueryFilters(parameters)}";

        return sql;
    }

    private string CreateRequestSql(EnergyResultQueryParameters parameters)
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
                AND {CreateSqlQueryFilters(parameters)}";

        sql += $@"ORDER BY t1.time";
        return sql;
    }

    private string CreateSqlQueryFilters(EnergyResultQueryParameters parameters)
    {
        var whereClausesSql = $@"t1.{EnergyResultColumnNames.GridArea} IN ({parameters.GridArea})
            AND t1.{EnergyResultColumnNames.TimeSeriesType} IN ('{TimeSeriesTypeMapper.ToDeltaTableValue(parameters.TimeSeriesType)}')
            AND t1.{EnergyResultColumnNames.Time} >= '{parameters.StartOfPeriod.ToString()}'
            AND t1.{EnergyResultColumnNames.Time} < '{parameters.EndOfPeriod.ToString()}'
            AND t1.{EnergyResultColumnNames.AggregationLevel} = '{AggregationLevelMapper.ToDeltaTableValue(parameters.TimeSeriesType, parameters.EnergySupplierId, parameters.BalanceResponsibleId)}'
            ";
        if (parameters.ProcessType != null)
        {
            whereClausesSql +=
                $@"AND t1.{EnergyResultColumnNames.BatchProcessType} = '{ProcessTypeMapper.ToDeltaTableValue((ProcessType)parameters.ProcessType)}'";
        }

        if (parameters.EnergySupplierId != null)
        {
            whereClausesSql += $@"AND t1.{EnergyResultColumnNames.EnergySupplierId} = '{parameters.EnergySupplierId}'";
        }

        if (parameters.BalanceResponsibleId != null)
        {
            whereClausesSql += $@"AND t1.{EnergyResultColumnNames.BalanceResponsibleId} = '{parameters.BalanceResponsibleId}'";
        }

        return whereClausesSql;
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
