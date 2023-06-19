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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class CalculationResultQueries : ICalculationResultQueries
{
    private readonly ISqlStatementClient _sqlStatementClient;

    public CalculationResultQueries(ISqlStatementClient sqlStatementClient)
    {
        _sqlStatementClient = sqlStatementClient;
    }

    public async IAsyncEnumerable<CalculationResult> GetAsync(Guid batchId)
    {
        var sql = CreateBatchResultsSql(batchId);
        var timeSeriesPoints = new List<TimeSeriesPoint>();
        SqlResultRow? currentRow = null;

        await foreach (var nextRow in _sqlStatementClient.ExecuteAsync(sql))
        {
            var timeSeriesPoint = CreateTimeSeriesPoint(nextRow);

            if (currentRow != null && BelongsToDifferentResults(currentRow, nextRow))
            {
                yield return CreateCalculationResult(batchId, currentRow, timeSeriesPoints);
                timeSeriesPoints = new List<TimeSeriesPoint>();
            }

            timeSeriesPoints.Add(timeSeriesPoint);
            currentRow = nextRow;
        }

        if (currentRow != null)
            yield return CreateCalculationResult(batchId, currentRow, timeSeriesPoints);
    }

    private string CreateBatchResultsSql(Guid batchId)
    {
        return $@"
SELECT {string.Join(", ", SqlColumnNames)}
FROM wholesale_output.result
WHERE {ResultColumnNames.BatchId} = '{batchId}'
ORDER BY time
";
    }

    public static string[] SqlColumnNames { get; } =
    {
        ResultColumnNames.BatchId,
        ResultColumnNames.GridArea,
        ResultColumnNames.BatchProcessType,
        ResultColumnNames.TimeSeriesType,
        ResultColumnNames.EnergySupplierId,
        ResultColumnNames.BalanceResponsibleId,
        ResultColumnNames.Time,
        ResultColumnNames.Quantity,
        ResultColumnNames.QuantityQuality,
    };

    public static bool BelongsToDifferentResults(SqlResultRow row, SqlResultRow otherRow)
    {
        return row[ResultColumnNames.BatchId] != otherRow[ResultColumnNames.BatchId]
               || row[ResultColumnNames.GridArea] != otherRow[ResultColumnNames.GridArea]
               || row[ResultColumnNames.TimeSeriesType] != otherRow[ResultColumnNames.TimeSeriesType]
               || row[ResultColumnNames.EnergySupplierId] != otherRow[ResultColumnNames.EnergySupplierId]
               || row[ResultColumnNames.BalanceResponsibleId] != otherRow[ResultColumnNames.BalanceResponsibleId];
    }

    private static TimeSeriesPoint CreateTimeSeriesPoint(SqlResultRow row)
    {
        var time = SqlResultValueConverters.ToDateTimeOffset(row[ResultColumnNames.Time])!.Value;
        var quantity = SqlResultValueConverters.ToDecimal(row[ResultColumnNames.Quantity])!.Value;
        var quality = SqlResultValueConverters.ToQuantityQuality(row[ResultColumnNames.QuantityQuality]);
        return new TimeSeriesPoint(time, quantity, quality);
    }

    private static CalculationResult CreateCalculationResult(
        Guid batchId,
        SqlResultRow sqlResultRow,
        List<TimeSeriesPoint> timeSeriesPoints)
    {
        var timeSeriesType = SqlResultValueConverters.ToTimeSeriesType(sqlResultRow[ResultColumnNames.TimeSeriesType]);
        var energySupplierId = sqlResultRow[ResultColumnNames.EnergySupplierId];
        var balanceResponsibleId = sqlResultRow[ResultColumnNames.BalanceResponsibleId];
        var gridArea = sqlResultRow[ResultColumnNames.GridArea];
        return new CalculationResult(
            batchId,
            gridArea,
            timeSeriesType,
            energySupplierId,
            balanceResponsibleId,
            timeSeriesPoints.ToArray());
    }
}
