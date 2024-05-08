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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TotalMonthlyAmountResults;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class TotalMonthlyAmountResultQueries : ITotalMonthlyAmountResultQueries
{
    private readonly ICalculationsClient _calculationsClient;
    private readonly DeltaTableOptions _deltaTableOptions;
    private readonly DatabricksSqlWarehouseQueryExecutor _databricksSqlWarehouseQueryExecutor;
    private readonly ILogger<WholesaleResultQueries> _logger;

    public TotalMonthlyAmountResultQueries(
        ICalculationsClient calculationsClient,
        IOptions<DeltaTableOptions> deltaTableOptions,
        DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor,
        ILogger<WholesaleResultQueries> logger)
    {
        _calculationsClient = calculationsClient;
        _deltaTableOptions = deltaTableOptions.Value;
        _databricksSqlWarehouseQueryExecutor = databricksSqlWarehouseQueryExecutor;
        _logger = logger;
    }

    public async IAsyncEnumerable<TotalMonthlyAmountResult> GetAsync(Guid calculationId)
    {
        var calculation = await _calculationsClient.GetAsync(calculationId).ConfigureAwait(false);
        var statement = new TotalMonthlyAmountResultQueryStatement(calculationId, _deltaTableOptions);
        await foreach (var calculationResult in GetInternalAsync(statement, calculation.PeriodStart.ToInstant(), calculation.PeriodEnd.ToInstant(), calculation.Version).ConfigureAwait(false))
            yield return calculationResult;
        _logger.LogDebug("Fetched all wholesale calculation results for calculation {calculation_id}", calculationId);
    }

    private async IAsyncEnumerable<TotalMonthlyAmountResult> GetInternalAsync(DatabricksStatement statement, Instant periodStart, Instant periodEnd, long version)
    {
        var resultCount = 0;
        await foreach (var row in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement, Format.JsonArray).ConfigureAwait(false))
        {
            var convertedNextRow = new DatabricksSqlRow(row);
            TotalMonthlyAmountResult totalMonthlyAmountResult = TotalMonthlyAmountResultFactory.CreateTotalMonthlyAmountResult(convertedNextRow, periodStart, periodEnd, version);
            yield return totalMonthlyAmountResult;
            resultCount++;
        }

        _logger.LogDebug("Fetched {result_count} calculation results", resultCount);
    }
}
