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
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class AggregatedTimeSeriesQueries : IAggregatedTimeSeriesQueries
{
    private readonly DatabricksSqlWarehouseQueryExecutor _databricksSqlWarehouseQueryExecutor;
    private readonly DeltaTableOptions _deltaTableOptions;

    public AggregatedTimeSeriesQueries(
        DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor,
        IOptions<DeltaTableOptions> deltaTableOptions)
    {
        _databricksSqlWarehouseQueryExecutor = databricksSqlWarehouseQueryExecutor;
        _deltaTableOptions = deltaTableOptions.Value;
    }

    public async IAsyncEnumerable<AggregatedTimeSeries> GetAsync(AggregatedTimeSeriesQueryParameters parameters)
    {
        if (!parameters.LatestCalculationForPeriod.Any())
        {
            yield break;
        }

        var sqlStatement = new AggregatedTimeSeriesQueryStatement(parameters, _deltaTableOptions);
        await foreach (var aggregatedTimeSeries in GetInternalAsync(sqlStatement, parameters.LatestCalculationForPeriod).ConfigureAwait(false))
            yield return aggregatedTimeSeries;
    }

    private async IAsyncEnumerable<AggregatedTimeSeries> GetInternalAsync(
        AggregatedTimeSeriesQueryStatement sqlStatement,
        IReadOnlyCollection<CalculationForPeriod> latestCalculationsForPeriod)
    {
        var timeSeriesPoints = new List<EnergyTimeSeriesPoint>();
        DatabricksSqlRow? previousRow = null;
        await foreach (var databricksCurrentRow in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(sqlStatement, Format.JsonArray).ConfigureAwait(false))
        {
            var currentRow = new DatabricksSqlRow(databricksCurrentRow);
            var timeSeriesPoint = EnergyTimeSeriesPointFactory.CreateTimeSeriesPoint(currentRow);

            if (previousRow != null && (BelongsToDifferentGridArea(currentRow, previousRow)
                                        || DifferentCalculationId(currentRow, previousRow)
                                        || HaveDifferentTimeSeriesType(currentRow, previousRow)))
            {
                var calculationForPeriod = GetCalculationForPeriod(previousRow, latestCalculationsForPeriod);
                yield return AggregatedTimeSeriesFactory.Create(
                    previousRow,
                    calculationForPeriod.Period.Start,
                    calculationForPeriod.Period.End,
                    timeSeriesPoints,
                    calculationForPeriod.CalculationVersion);
                timeSeriesPoints = new List<EnergyTimeSeriesPoint>();
            }

            timeSeriesPoints.Add(timeSeriesPoint);
            previousRow = currentRow;
        }

        if (previousRow != null)
        {
            var calculationForPeriod = GetCalculationForPeriod(previousRow, latestCalculationsForPeriod);
            yield return AggregatedTimeSeriesFactory.Create(
                previousRow,
                calculationForPeriod.Period.Start,
                calculationForPeriod.Period.End,
                timeSeriesPoints,
                calculationForPeriod.CalculationVersion);
        }
    }

    private CalculationForPeriod GetCalculationForPeriod(DatabricksSqlRow row, IReadOnlyCollection<CalculationForPeriod> latestCalculationForPeriod)
    {
        return latestCalculationForPeriod
            .First(x => x.CalculationId == Guid.Parse(row[EnergyResultColumnNames.CalculationId]!));
    }

    private static bool DifferentCalculationId(DatabricksSqlRow row, DatabricksSqlRow otherRow)
    {
        return row[EnergyResultColumnNames.CalculationId] != otherRow[EnergyResultColumnNames.CalculationId];
    }

    private static bool BelongsToDifferentGridArea(DatabricksSqlRow row, DatabricksSqlRow otherRow)
    {
        return row[EnergyResultColumnNames.GridArea] != otherRow[EnergyResultColumnNames.GridArea];
    }

    private static bool HaveDifferentTimeSeriesType(DatabricksSqlRow row, DatabricksSqlRow otherRow)
    {
        return row[EnergyResultColumnNames.TimeSeriesType] != otherRow[EnergyResultColumnNames.TimeSeriesType];
    }
}
