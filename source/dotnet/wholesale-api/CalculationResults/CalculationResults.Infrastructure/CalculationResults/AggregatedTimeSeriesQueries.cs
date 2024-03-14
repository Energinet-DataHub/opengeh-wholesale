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
        RowData? previous = null;

        await foreach (var databricksCurrentRow in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(sqlStatement, Format.JsonArray).ConfigureAwait(false))
        {
            RowData current = new(
                new DatabricksSqlRow(databricksCurrentRow),
                GetCalculationForPeriod(new DatabricksSqlRow(databricksCurrentRow), latestCalculationsForPeriod));

            // Yield a package created from previous data, if the current row belongs to a new package
            if (previous != null && RowBelongsToNewPackage(current, previous))
            {
                yield return CreatePackageFromPreviousData(previous, timeSeriesPoints);
                timeSeriesPoints = [];
            }

            timeSeriesPoints.Add(EnergyTimeSeriesPointFactory.CreateTimeSeriesPoint(current.Row));
            previous = current;
        }

        // Yield the last package
        if (previous != null)
        {
            yield return AggregatedTimeSeriesFactory.Create(
                previous.Row,
                previous.CalculationPeriod.Period.Start,
                previous.CalculationPeriod.Period.End,
                timeSeriesPoints,
                previous.CalculationPeriod.CalculationVersion);
        }
    }

    private static bool RowBelongsToNewPackage(RowData current, RowData previous)
    {
        return BelongsToDifferentGridArea(current.Row, previous.Row)
               || HaveDifferentCalculationId(current.Row, previous.Row)
               || HaveDifferentTimeSeriesType(current.Row, previous.Row)
               || previous.CalculationPeriod != current.CalculationPeriod;
    }

    private static AggregatedTimeSeries CreatePackageFromPreviousData(RowData previous, List<EnergyTimeSeriesPoint> timeSeriesPoints)
    {
        return AggregatedTimeSeriesFactory.Create(
            previous.Row,
            previous.CalculationPeriod.Period.Start,
            previous.CalculationPeriod.Period.End,
            timeSeriesPoints,
            previous.CalculationPeriod.CalculationVersion);
    }

    private CalculationForPeriod GetCalculationForPeriod(DatabricksSqlRow row, IReadOnlyCollection<CalculationForPeriod> latestCalculationForPeriod)
    {
        var calculationId = Guid.Parse(row[EnergyResultColumnNames.CalculationId]!);
        var time = SqlResultValueConverters.ToInstant(row[EnergyResultColumnNames.Time])!.Value;

        return latestCalculationForPeriod
            .Single(x => x.CalculationId == calculationId && x.Period.Contains(time));
    }

    private static bool HaveDifferentCalculationId(DatabricksSqlRow row, DatabricksSqlRow otherRow)
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

    private record RowData(DatabricksSqlRow Row, CalculationForPeriod CalculationPeriod);
}
