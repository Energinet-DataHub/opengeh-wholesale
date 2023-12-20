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
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class EnergyResultQueries : IEnergyResultQueries
{
    private readonly DatabricksSqlWarehouseQueryExecutor _databricksSqlWarehouseQueryExecutor;
    private readonly ICalculationsClient _calculationsClient;
    private readonly DeltaTableOptions _deltaTableOptions;
    private readonly ILogger<EnergyResultQueries> _logger;

    public EnergyResultQueries(DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor, ICalculationsClient calculationsClient, IOptions<DeltaTableOptions> deltaTableOptions, ILogger<EnergyResultQueries> logger)
    {
        _databricksSqlWarehouseQueryExecutor = databricksSqlWarehouseQueryExecutor;
        _calculationsClient = calculationsClient;
        _deltaTableOptions = deltaTableOptions.Value;
        _logger = logger;
    }

    public async IAsyncEnumerable<EnergyResult> GetAsync(Guid batchId)
    {
        var batch = await _calculationsClient.GetAsync(batchId).ConfigureAwait(false);
        var statement = new EnergyResultQueryStatement(batchId, _deltaTableOptions);
        await foreach (var calculationResult in GetInternalAsync(statement, batch.PeriodStart.ToInstant(), batch.PeriodEnd.ToInstant()))
            yield return calculationResult;
        _logger.LogDebug("Fetched all energy results for calculation {calculation_id}", batchId);
    }

    public static bool BelongsToDifferentResults(DatabricksSqlRow row, DatabricksSqlRow otherRow)
    {
        return !row[EnergyResultColumnNames.CalculationResultId]!.Equals(otherRow[EnergyResultColumnNames.CalculationResultId]);
    }

    private async IAsyncEnumerable<EnergyResult> GetInternalAsync(EnergyResultQueryStatement statement, Instant periodStart, Instant periodEnd)
    {
        var timeSeriesPoints = new List<EnergyTimeSeriesPoint>();
        DatabricksSqlRow? currentRow = null;
        var resultCount = 0;

        await foreach (var nextRow in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement, Format.JsonArray).ConfigureAwait(false))
        {
            var databricksSqlNextRow = new DatabricksSqlRow(nextRow);
            var timeSeriesPoint = EnergyTimeSeriesPointFactory.CreateTimeSeriesPoint(databricksSqlNextRow);

            if (currentRow != null && BelongsToDifferentResults(currentRow, databricksSqlNextRow))
            {
                yield return EnergyResultFactory.CreateEnergyResult(currentRow!, timeSeriesPoints, periodStart, periodEnd);
                resultCount++;
                timeSeriesPoints = new List<EnergyTimeSeriesPoint>();
            }

            timeSeriesPoints.Add(timeSeriesPoint);
            currentRow = databricksSqlNextRow;
        }

        if (currentRow != null)
        {
            yield return EnergyResultFactory.CreateEnergyResult(currentRow, timeSeriesPoints, periodStart, periodEnd);
            resultCount++;
        }

        _logger.LogDebug("Fetched {ResultCount} calculation results", resultCount);
    }
}
