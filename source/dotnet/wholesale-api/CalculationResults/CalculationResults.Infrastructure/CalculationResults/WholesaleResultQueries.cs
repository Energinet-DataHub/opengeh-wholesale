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
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class WholesaleResultQueries : IWholesaleResultQueries
{
    private readonly IDatabricksSqlStatementClient _sqlStatementClient;
    private readonly IBatchesClient _batchesClient;
    private readonly DeltaTableOptions _deltaTableOptions;
    private readonly ILogger<WholesaleResultQueries> _logger;

    public WholesaleResultQueries(IDatabricksSqlStatementClient sqlStatementClient, IBatchesClient batchesClient, IOptions<DeltaTableOptions> deltaTableOptions, ILogger<WholesaleResultQueries> logger)
    {
        _sqlStatementClient = sqlStatementClient;
        _batchesClient = batchesClient;
        _deltaTableOptions = deltaTableOptions.Value;
        _logger = logger;
    }

    public async IAsyncEnumerable<WholesaleResult> GetAsync(Guid calculationId)
    {
        var calculation = await _batchesClient.GetAsync(calculationId).ConfigureAwait(false);
        var sql = CreateCalculationResultsSql(calculationId);
        await foreach (var calculationResult in GetInternalAsync(sql, calculation.PeriodStart.ToInstant(), calculation.PeriodEnd.ToInstant()).ConfigureAwait(false))
            yield return calculationResult;
        _logger.LogDebug("Fetched all wholesale calculation results for calculation {CalculationId}", calculationId);
    }

    private async IAsyncEnumerable<WholesaleResult> GetInternalAsync(string sql, Instant periodStart, Instant periodEnd)
    {
        var timeSeriesPoints = new List<WholesaleTimeSeriesPoint>();
        SqlResultRow? currentRow = null;
        var resultCount = 0;

        await foreach (var nextRow in _sqlStatementClient.ExecuteAsync(sql, sqlStatementParameters: null).ConfigureAwait(false))
        {
            var timeSeriesPoint = WholesaleTimeSeriesPointFactory.Create(nextRow);

            if (currentRow != null && BelongsToDifferentResults(currentRow, nextRow))
            {
                yield return WholesaleResultFactory.CreateWholesaleResult(currentRow, timeSeriesPoints, periodStart, periodEnd);
                resultCount++;
                timeSeriesPoints = new List<WholesaleTimeSeriesPoint>();
            }

            timeSeriesPoints.Add(timeSeriesPoint);
            currentRow = nextRow;
        }

        if (currentRow != null)
        {
            yield return WholesaleResultFactory.CreateWholesaleResult(currentRow, timeSeriesPoints, periodStart, periodEnd);
            resultCount++;
        }

        _logger.LogDebug("Fetched {ResultCount} calculation results", resultCount);
    }

    private string CreateCalculationResultsSql(Guid calculationId)
    {
        return $@"
SELECT {string.Join(", ", SqlColumnNames)}
FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.WHOLESALE_RESULTS_TABLE_NAME}
WHERE {WholesaleResultColumnNames.CalculationId} = '{calculationId}'
ORDER BY {WholesaleResultColumnNames.CalculationResultId}, {WholesaleResultColumnNames.Time}
";
    }

    public static string[] SqlColumnNames { get; } =
    {
        WholesaleResultColumnNames.CalculationId,
        WholesaleResultColumnNames.CalculationResultId,
        WholesaleResultColumnNames.CalculationType,
        WholesaleResultColumnNames.GridArea,
        WholesaleResultColumnNames.EnergySupplierId,
        WholesaleResultColumnNames.AmountType,
        WholesaleResultColumnNames.MeteringPointType,
        WholesaleResultColumnNames.SettlementMethod,
        WholesaleResultColumnNames.ChargeType,
        WholesaleResultColumnNames.ChargeCode,
        WholesaleResultColumnNames.ChargeOwnerId,
        WholesaleResultColumnNames.Resolution,
        WholesaleResultColumnNames.IsTax,
        WholesaleResultColumnNames.QuantityUnit,
        WholesaleResultColumnNames.Time,
        WholesaleResultColumnNames.Quantity,
        WholesaleResultColumnNames.QuantityQualities,
        WholesaleResultColumnNames.Price,
        WholesaleResultColumnNames.Amount,
    };

    public static bool BelongsToDifferentResults(SqlResultRow row, SqlResultRow otherRow)
    {
        return row[WholesaleResultColumnNames.CalculationResultId] != otherRow[WholesaleResultColumnNames.CalculationResultId];
    }
}
