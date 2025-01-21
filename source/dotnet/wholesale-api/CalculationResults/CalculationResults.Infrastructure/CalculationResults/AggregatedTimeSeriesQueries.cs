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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.EnergyResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class AggregatedTimeSeriesQueries(
    DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor,
    AggregatedTimeSeriesQuerySnippetProviderFactory querySnippetProviderFactory,
    IOptions<DeltaTableOptions> deltaTableOptions)
    : RequestQueriesBase(databricksSqlWarehouseQueryExecutor), IAggregatedTimeSeriesQueries
{
    private readonly AggregatedTimeSeriesQuerySnippetProviderFactory _querySnippetProviderFactory = querySnippetProviderFactory;
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions = deltaTableOptions;

    public async IAsyncEnumerable<AggregatedTimeSeries> GetAsync(AggregatedTimeSeriesQueryParameters parameters)
    {
        /*
         * We retain the usage of 'TimeSeriesType' here, to reduce the number of changes required.
         * However, it is worth noting that the 'TimeSeriesType' is not used in the query per se,
         * but rather translated to 'MeteringPointType' and 'SettlementMethod',
         * as these are the proper values used in the raw data (and from the request from EDI).
         * The reason for this is the existence of logic wrt validation and determination of "all types for an actor role"
         * in the reception of the request from EDI that would have to be reimplemented to avoid using 'TimeSeriesType'.
         * Once requests are brought back home to EDI, we should leave TimeSeriesType behind.
         */
        foreach (var timeSeriesType in parameters.TimeSeriesTypes)
        {
            var querySnippetProvider = _querySnippetProviderFactory.Create(parameters, timeSeriesType);

            var calculationTypePerGridAreas =
                await GetCalculationTypeForGridAreasAsync(
                        querySnippetProvider.DatabricksContract.GetGridAreaCodeColumnName(),
                        querySnippetProvider.DatabricksContract.GetCalculationTypeColumnName(),
                        new AggregatedTimeSeriesCalculationTypeForGridAreasQueryStatement(
                            _deltaTableOptions.Value,
                            querySnippetProvider),
                        parameters.CalculationType)
                    .ConfigureAwait(false);

            var sqlStatement = new AggregatedTimeSeriesQueryStatement(
                calculationTypePerGridAreas,
                querySnippetProvider,
                timeSeriesType,
                _deltaTableOptions.Value);

            var calculationIdColumnName = querySnippetProvider.DatabricksContract.GetCalculationIdColumnName();

            await foreach (var aggregatedTimeSeries in CreateSeriesPackagesAsync(
                               (row, points) => AggregatedTimeSeriesFactory.Create(
                                   querySnippetProvider.DatabricksContract,
                                   timeSeriesType,
                                   row,
                                   points),
                               (currentRow, previousRow) =>
                                   querySnippetProvider.DatabricksContract.GetColumnsToAggregateBy()
                                       .Any(column => currentRow[column] != previousRow[column])
                                   || currentRow[calculationIdColumnName] != previousRow[calculationIdColumnName]
                                   || !ResultStartEqualsPreviousResultEnd(currentRow, previousRow, querySnippetProvider.DatabricksContract),
                               EnergyTimeSeriesPointFactory.CreateTimeSeriesPoint,
                               sqlStatement))
            {
                yield return aggregatedTimeSeries;
            }
        }
    }

    /// <summary>
    /// Checks if the current result follows the previous result based on time and resolution.
    /// </summary>
    private bool ResultStartEqualsPreviousResultEnd(
        DatabricksSqlRow currentResult,
        DatabricksSqlRow previousResult,
        IAggregatedTimeSeriesDatabricksContract databricksContract)
    {
        var endTimeFromPreviousResult = GetEndTimeOfPreviousResult(previousResult, databricksContract);
        var startTimeFromCurrentResult = SqlResultValueConverters
            .ToDateTimeOffset(currentResult[databricksContract.GetTimeColumnName()])!.Value;

        // The start time of the current result should be the same as the end time of the previous result if the result is in sequence with the previous result.
        return endTimeFromPreviousResult == startTimeFromCurrentResult;
    }

    private DateTimeOffset GetEndTimeOfPreviousResult(
        DatabricksSqlRow previousResult,
        IAggregatedTimeSeriesDatabricksContract databricksContract)
    {
        var resolutionOfPreviousResult = ResolutionMapper
            .FromDeltaTableValue(previousResult[databricksContract.GetResolutionColumnName()]!);

        var startTimeOfPreviousResult = SqlResultValueConverters
            .ToDateTimeOffset(previousResult[databricksContract.GetTimeColumnName()])!.Value;

        return PeriodHelper.GetDateTimeWithResolutionOffset(
            resolutionOfPreviousResult,
            startTimeOfPreviousResult);
    }
}
