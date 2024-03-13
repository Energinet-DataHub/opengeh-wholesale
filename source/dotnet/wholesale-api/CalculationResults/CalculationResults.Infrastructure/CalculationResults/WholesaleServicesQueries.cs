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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class WholesaleServicesQueries : IWholesaleServicesQueries
{
    private readonly DatabricksSqlWarehouseQueryExecutor _databricksSqlWarehouseQueryExecutor;
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions;
    private readonly ILogger<WholesaleServicesQueries> _logger;

    public WholesaleServicesQueries(
        DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor,
        IOptions<DeltaTableOptions> deltaTableOptions,
        ILogger<WholesaleServicesQueries> logger)
    {
        _databricksSqlWarehouseQueryExecutor = databricksSqlWarehouseQueryExecutor;
        _deltaTableOptions = deltaTableOptions;
        _logger = logger;
    }

    public async IAsyncEnumerable<WholesaleServices> GetAsync(WholesaleServicesQueryParameters queryParameters)
    {
        if (!queryParameters.Calculations.Any())
        {
            yield break;
        }

        var sqlStatement = new WholesaleServicesQueryStatement(WholesaleServicesQueryStatement.StatementType.Select, queryParameters, _deltaTableOptions.Value);
        await foreach (var wholesaleServices in GetInternalAsync(sqlStatement, queryParameters.Calculations).ConfigureAwait(false))
            yield return wholesaleServices;
    }

    public Task<bool> AnyAsync(WholesaleServicesQueryParameters queryParameters)
    {
        var sqlStatement = new WholesaleServicesQueryStatement(WholesaleServicesQueryStatement.StatementType.Exists, queryParameters, _deltaTableOptions.Value);

        return _databricksSqlWarehouseQueryExecutor
            .ExecuteStatementAsync(sqlStatement)
            .AnyAsync()
            .AsTask();
    }

    private async IAsyncEnumerable<WholesaleServices> GetInternalAsync(DatabricksStatement statement, IReadOnlyCollection<CalculationForPeriod> calculations)
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
                yield return WholesaleServicesFactory.Create(currentRow, timeSeriesPoints);
                resultCount++;
                timeSeriesPoints = [];
            }

            timeSeriesPoints.Add(timeSeriesPoint);
            currentRow = convertedNextRow;
        }

        if (currentRow != null)
        {
            yield return WholesaleServicesFactory.Create(currentRow, timeSeriesPoints);
            resultCount++;
        }

        _logger.LogDebug("Fetched {result_count} wholesale services results", resultCount);
    }

    private bool BelongsToDifferentResults(DatabricksSqlRow row, DatabricksSqlRow otherSqlRow)
    {
        return !row[WholesaleResultColumnNames.CalculationResultId]!.Equals(otherSqlRow[WholesaleResultColumnNames.CalculationResultId]);
    }
}
