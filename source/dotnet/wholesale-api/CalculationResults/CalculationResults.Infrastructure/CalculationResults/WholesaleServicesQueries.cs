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
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        await foreach (var wholesaleServices in GetInternalAsync(sqlStatement, queryParameters).ConfigureAwait(false))
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

    private async IAsyncEnumerable<WholesaleServices> GetInternalAsync(DatabricksStatement statement, WholesaleServicesQueryParameters parameters)
    {
        var timeSeriesPoints = new List<WholesaleTimeSeriesPoint>();
        DatabricksSqlRow? previousRow = null;
        var resultCount = 0;

        await foreach (var nextRow in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement, Format.JsonArray).ConfigureAwait(false))
        {
            var currentRow = new DatabricksSqlRow(nextRow);

            // If current row belongs to a new package, yield a package for the previous data and then start a new package
            if (previousRow != null && BelongsToDifferentResults(previousRow, currentRow))
            {
                yield return CreateFinishedPackage(parameters, previousRow, timeSeriesPoints);
                resultCount++;
                timeSeriesPoints = [];
            }

            timeSeriesPoints.Add(WholesaleTimeSeriesPointFactory.Create(currentRow));
            previousRow = currentRow;
        }

        if (previousRow != null)
        {
            yield return CreateFinishedPackage(parameters, previousRow, timeSeriesPoints);
            resultCount++;
        }

        _logger.LogDebug("Fetched {result_count} wholesale services results", resultCount);
    }

    private bool BelongsToDifferentResults(DatabricksSqlRow row, DatabricksSqlRow otherSqlRow)
    {
        return !row[WholesaleResultColumnNames.CalculationResultId]!.Equals(otherSqlRow[WholesaleResultColumnNames.CalculationResultId]);
    }

    private WholesaleServices CreateFinishedPackage(WholesaleServicesQueryParameters parameters, DatabricksSqlRow row, List<WholesaleTimeSeriesPoint> timeSeriesPoints)
    {
        var calculationWithPeriod = GetCalculationForRow(row, parameters.Calculations);
        return WholesaleServicesFactory.Create(row, calculationWithPeriod.Period, timeSeriesPoints, calculationWithPeriod.CalculationVersion);
    }

    private CalculationForPeriod GetCalculationForRow(DatabricksSqlRow row, IReadOnlyCollection<CalculationForPeriod> calculations)
    {
        return calculations.First(x => x.CalculationId == Guid.Parse(row[WholesaleResultColumnNames.CalculationId]!));
    }
}
