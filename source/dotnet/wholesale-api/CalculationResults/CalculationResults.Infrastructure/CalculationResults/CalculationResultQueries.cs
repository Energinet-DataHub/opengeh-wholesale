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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Microsoft.Extensions.Options;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class CalculationResultQueries : ICalculationResultQueries
{
    private readonly ISqlStatementClient _sqlStatementClient;
    private readonly ICalculationsClient _calculationsClient;
    private readonly DeltaTableOptions _deltaTableOptions;

    public CalculationResultQueries(ISqlStatementClient sqlStatementClient, ICalculationsClient calculationsClient, IOptions<DeltaTableOptions> deltaTableOptions)
    {
        _sqlStatementClient = sqlStatementClient;
        _calculationsClient = calculationsClient;
        _deltaTableOptions = deltaTableOptions.Value;
    }

    public async IAsyncEnumerable<CalculationResult> GetAsync(Guid calculationId)
    {
        var calculation = await _calculationsClient.GetAsync(calculationId).ConfigureAwait(false);
        var sql = CreateCalculationResultsSql(calculationId);
        var timeSeriesPoints = new List<TimeSeriesPoint>();
        SqlResultRow? currentRow = null;

        await foreach (var nextRow in _sqlStatementClient.ExecuteAsync(sql))
        {
            var timeSeriesPoint = CreateTimeSeriesPoint(nextRow);

            if (currentRow != null && BelongsToDifferentResults(currentRow, nextRow))
            {
                yield return CreateCalculationResult(calculation, currentRow, timeSeriesPoints);
                timeSeriesPoints = new List<TimeSeriesPoint>();
            }

            timeSeriesPoints.Add(timeSeriesPoint);
            currentRow = nextRow;
        }

        if (currentRow != null)
            yield return CreateCalculationResult(calculation, currentRow, timeSeriesPoints);
    }

    private string CreateCalculationResultsSql(Guid calculationId)
    {
        return $@"
SELECT {string.Join(", ", SqlColumnNames)}
FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.RESULT_TABLE_NAME}
WHERE {ResultColumnNames.CalculationId} = '{calculationId}'
ORDER BY {ResultColumnNames.CalculationResultId}, {ResultColumnNames.Time}
";
    }

    public static string[] SqlColumnNames { get; } =
    {
        ResultColumnNames.CalculationId,
        ResultColumnNames.GridArea,
        ResultColumnNames.FromGridArea,
        ResultColumnNames.TimeSeriesType,
        ResultColumnNames.EnergySupplierId,
        ResultColumnNames.BalanceResponsibleId,
        ResultColumnNames.Time,
        ResultColumnNames.Quantity,
        ResultColumnNames.QuantityQuality,
        ResultColumnNames.CalculationResultId,
    };

    public static bool BelongsToDifferentResults(SqlResultRow row, SqlResultRow otherRow)
    {
        return row[ResultColumnNames.CalculationResultId] != otherRow[ResultColumnNames.CalculationResultId];
    }

    private static TimeSeriesPoint CreateTimeSeriesPoint(SqlResultRow row)
    {
        var time = SqlResultValueConverters.ToDateTimeOffset(row[ResultColumnNames.Time])!.Value;
        var quantity = SqlResultValueConverters.ToDecimal(row[ResultColumnNames.Quantity])!.Value;
        var quality = SqlResultValueConverters.ToQuantityQuality(row[ResultColumnNames.QuantityQuality]);
        return new TimeSeriesPoint(time, quantity, quality);
    }

    private static CalculationResult CreateCalculationResult(
        CalculationDto calculation,
        SqlResultRow sqlResultRow,
        List<TimeSeriesPoint> timeSeriesPoints)
    {
        var id = SqlResultValueConverters.ToGuid(sqlResultRow[ResultColumnNames.CalculationResultId]);
        var timeSeriesType = SqlResultValueConverters.ToTimeSeriesType(sqlResultRow[ResultColumnNames.TimeSeriesType]);
        var energySupplierId = sqlResultRow[ResultColumnNames.EnergySupplierId];
        var balanceResponsibleId = sqlResultRow[ResultColumnNames.BalanceResponsibleId];
        var gridArea = sqlResultRow[ResultColumnNames.GridArea];
        var fromGridArea = sqlResultRow[ResultColumnNames.FromGridArea];
        return new CalculationResult(
            id,
            calculation.CalculationId,
            gridArea,
            timeSeriesType,
            energySupplierId,
            balanceResponsibleId,
            timeSeriesPoints.ToArray(),
            calculation.ProcessType,
            calculation.PeriodStart.ToInstant(),
            calculation.PeriodEnd.ToInstant(),
            fromGridArea);
    }
}
