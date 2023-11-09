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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.RequestCalculationResult;

public class AggregatedTimeSeriesQueries : IAggregatedTimeSeriesQueries
{
    private readonly IDatabricksSqlStatementClient _sqlStatementClient;
    private readonly AggregatedTimeSeriesSqlGenerator _aggregatedTimeSeriesSqlGenerator;

    public AggregatedTimeSeriesQueries(
        IDatabricksSqlStatementClient sqlStatementClient,
        AggregatedTimeSeriesSqlGenerator aggregatedTimeSeriesSqlGenerator)
    {
        _sqlStatementClient = sqlStatementClient;
        _aggregatedTimeSeriesSqlGenerator = aggregatedTimeSeriesSqlGenerator;
    }

    public async Task<EnergyResult?> GetAsync(AggregatedTimeSeriesQueryParameters parameters)
    {
        var sqlStatement = _aggregatedTimeSeriesSqlGenerator.CreateRequestSql(parameters);

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

    public async Task<EnergyResult?> GetLatestCorrectionAsync(AggregatedTimeSeriesQueryParameters parameters)
    {
        if (parameters.ProcessType != null)
        {
            throw new ArgumentException("ProcessType must be null, since it will be overwritten", nameof(parameters));
        }

        var processType = await GetProcessTypeOfLatestCorrectionAsync(parameters).ConfigureAwait(false);

        var queryWithLatestCorrection = new AggregatedTimeSeriesQueryParameters(parameters, processType);

        return await GetAsync(queryWithLatestCorrection).ConfigureAwait(false);
    }

    public async Task<ProcessType> GetProcessTypeOfLatestCorrectionAsync(AggregatedTimeSeriesQueryParameters parameters)
    {
        var thirdCorrectionExists = await PerformCorrectionVersionExistsQueryAsync(
            new AggregatedTimeSeriesQueryParameters(
                parameters,
                ProcessType.ThirdCorrectionSettlement)).ConfigureAwait(false);
        if (thirdCorrectionExists)
            return ProcessType.ThirdCorrectionSettlement;

        var secondCorrectionExists = await PerformCorrectionVersionExistsQueryAsync(
            new AggregatedTimeSeriesQueryParameters(
                parameters,
                ProcessType.SecondCorrectionSettlement)).ConfigureAwait(false);

        if (secondCorrectionExists)
            return ProcessType.SecondCorrectionSettlement;

        return ProcessType.FirstCorrectionSettlement;
    }

    private Task<bool> PerformCorrectionVersionExistsQueryAsync(AggregatedTimeSeriesQueryParameters parameters)
    {
        var sql = CreateSelectCorrectionVersionSql(parameters);

        var tableRows = _sqlStatementClient.ExecuteAsync(sql, sqlStatementParameters: null);

        return tableRows.AnyAsync().AsTask();
    }

    private string CreateSelectCorrectionVersionSql(AggregatedTimeSeriesQueryParameters parameters)
    {
        var processType = parameters.ProcessType;

        if (processType != ProcessType.FirstCorrectionSettlement
            && processType != ProcessType.SecondCorrectionSettlement
            && processType != ProcessType.ThirdCorrectionSettlement)
            throw new ArgumentOutOfRangeException(nameof(processType), processType, "ProcessType must be a correction settlement type");

        var sql = _aggregatedTimeSeriesSqlGenerator.GetSingleRow(parameters);

        return sql;
    }
}
